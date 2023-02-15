using Be.Windows.Forms;
using OrbisLib2.Common.Database.Types;
using OrbisLib2.Common.Dispatcher;
using OrbisLib2.Dialog;
using OrbisLib2.General;
using OrbisLib2.Targets;
using SimpleUI.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Shell;

namespace OrbisPeeknPoke
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SimpleWindow
    {
        private List<ulong> JumpList = new();

        public MainWindow()
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            InitializeComponent();
            DispatcherClient.Subscribe();

            Events.ProcAttach += Events_ProcAttach;
            Events.ProcDetach += Events_ProcDetach;
            Events.ProcDie += Events_ProcDie;
            Events.TargetStateChanged += Events_TargetStateChanged;
            Events.DBTouched += Events_DBTouched;
            Events.SelectedTargetChanged += Events_SelectedTargetChanged;

            HexBox.CurrentLineChanged += HexBox_CurrentLineChanged;
            HexBox.CurrentPositionInLineChanged += HexBox_CurrentPositionInLineChanged;
        }

        private void HexBox_CurrentPositionInLineChanged(object? sender, EventArgs e)
        {
            OffsetValue.Text = $"0x{(((HexBox.CurrentLine - 1) * 0x10) + (HexBox.CurrentPositionInLine - 1)).ToString("X")}";
        }

        private void HexBox_CurrentLineChanged(object? sender, EventArgs e)
        {
            OffsetValue.Text = $"0x{(((HexBox.CurrentLine - 1) * 0x10) + (HexBox.CurrentPositionInLine - 1)).ToString("X")}";
        }

        #region Events

        private void EnableProgram(bool Attached)
        {
            if (Attached)
            {
                // Try to load memory window.
            }
            else
            {
                // Clear memory window.
            }

            DetachProcess.IsEnabled = Attached;
            KillProcess.IsEnabled = Attached;
        }

        private void EnableTargetOptions(bool state)
        {
            Dispatcher.Invoke(() =>
            {
                AttachProcess.IsEnabled = state;
                DetachProcess.IsEnabled = state;
                LoadSomething.IsEnabled = state;
                RestartTarget.IsEnabled = state;
                ShutdownTarget.IsEnabled = state;

                EnableProgram(TargetManager.SelectedTarget.Debug.IsDebugging);
            });
        }

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            EnableTargetOptions(e.State == TargetStateChangedEvent.TargetState.APIAvailable);
        }

        private void Events_ProcDie(object? sender, ProcDieEvent e)
        {
            if (e.SendingTarget.IPAddress == TargetManager.SelectedTarget.IPAddress)
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentDebuggingProccess.FieldText = "N/A";
                    EnableProgram(false);
                });
            }
        }

        private void Events_ProcDetach(object? sender, ProcDetachEvent e)
        {
            if (e.SendingTarget.IPAddress == TargetManager.SelectedTarget.IPAddress)
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentDebuggingProccess.FieldText = "N/A";
                    EnableProgram(false);
                });
            }
        }

        private void Events_ProcAttach(object? sender, ProcAttachEvent e)
        {
            if (e.SendingTarget.IPAddress == TargetManager.SelectedTarget.IPAddress)
            {
                var processList = TargetManager.SelectedTarget.Process.GetList();
                var proc = processList.Find(x => x.ProcessId == e.NewProcessId);
                if (proc != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CurrentDebuggingProccess.FieldText = $"{proc.Name}({e.NewProcessId})";
                        EnableProgram(true);
                    });
                }
            }
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            var currentTarget = TargetManager.SelectedTarget;
            EnableTargetOptions(currentTarget.Info.Status == TargetStatusType.APIAvailable);
        }

        private void Events_SelectedTargetChanged(object? sender, SelectedTargetChangedEvent e)
        {
            var currentTarget = TargetManager.SelectedTarget;
            EnableTargetOptions(currentTarget.Info.Status == TargetStatusType.APIAvailable);
        }

        #endregion

        #region Buttons

        private bool TryConvertStringToUlong(string str, out ulong val)
        {
            if (str.StartsWith("0x"))
            {
                if (!ulong.TryParse(str.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val))
                {
                    return false;
                }
            }
            else
            {
                if (!ulong.TryParse(str, out val) && !ulong.TryParse(str, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val))
                {
                    return false;
                }
            }

            return true;
        }

        private bool GetPeekPokeInfo(out ulong address, out ulong length)
        {
            // Hex or decimal value of address
            if (!TryConvertStringToUlong(BaseAddress.FieldText, out address))
            {
                SimpleMessageBox.ShowError(this, "Failed to parse Base Address please ensure that it is a valid hex or decimal number.", "Failed to parse Base Address.");
                length = 0;
                return false;
            }

            // Hex or decimal value of offset
            ulong offset;
            if (TryConvertStringToUlong(Offset.FieldText, out offset))
            {
                address += offset;
            }

            // Hex or decimal value of length
            if (!TryConvertStringToUlong(Length.FieldText, out length))
            {
                SimpleMessageBox.ShowError(this, "Failed to parse Length please ensure that it is a valid hex or decimal number.", "Failed to parse Length.");
                return false;
            }

            return true;
        }

        private void SelectBase_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var mainExecutable = TargetManager.SelectedTarget.Debug.GetLibraries().FirstOrDefault();

                if (mainExecutable != null)
                {
                    Dispatcher.Invoke(() => BaseAddress.FieldText = $"0x{mainExecutable.Segments[0].Address.ToString("X")}");
                }
            });
        }

        private void Peek_Click(object sender, RoutedEventArgs e)
        {
            GetPeekPokeInfo(out var address, out var length);

            Task.Run(() =>
            {
                var data = TargetManager.SelectedTarget.Debug.ReadMemory(address, length);

                if (data != null && data.Length > 0)
                {
                    // Clear the jump list if were reading new memory.
                    if (JumpList.Count > 0 && address != JumpList.Last())
                    {
                        JumpList.Clear();
                        Dispatcher.Invoke(() => ReturnPointer.IsEnabled = false);
                    }

                    // Fill the hex box on the UI thread.
                    Dispatcher.Invoke(() =>
                    {
                        HexBox.VScrollBarVisible = true;
                        HexBox.ByteProvider = new DynamicByteProvider(data);
                        HexBox.VScrollBarVisible = false;
                    });
                }
            });
        }

        private void Poke_Click(object sender, RoutedEventArgs e)
        {
            if (HexBox.ByteProvider.Length <= 0)
                return;

            GetPeekPokeInfo(out var address, out var length);

            byte[] bytes = new byte[HexBox.ByteProvider.Length];

            //Grab the bytes from the hex window
            for (int i = 0; i < HexBox.ByteProvider.Length; i++)
                bytes[i] = HexBox.ByteProvider.ReadByte(i);

            // Write bytes in task so we dont lock up the UI thread.
            Task.Run(() => TargetManager.SelectedTarget.Debug.WriteMemory(address, bytes));
        }

        private void AttachProcess_Click(object sender, RoutedEventArgs e)
        {
            SelectProcess.ShowDialog(GetWindow(this));
        }

        private void DetachProcess_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => TargetManager.SelectedTarget.Debug.Detach());
        }

        private void LoadSomething_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var currentTarget = TargetManager.SelectedTarget;
                var processList = TargetManager.SelectedTarget.Process.GetList();
                var process = processList.Find(x => x.ProcessId == currentTarget.Debug.GetCurrentProcessId());
                if (process != null)
                {
                    if (process.AppId > 0)
                    {
                        currentTarget.Application.Stop(process.TitleId);
                    }
                    else
                    {
                        SimpleMessageBox.ShowError(Window.GetWindow(this), $"Could not kill process \"{process.Name}\" because Orbis Suite doesnt currently support killing processes with out an appId.", "Error: Could not kill this process.");
                    }
                }
            });
        }

        private void RestartTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => TargetManager.SelectedTarget.Reboot());
        }

        private void ShutdownTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => TargetManager.SelectedTarget.Shutdown());
        }

        #endregion

        #region Context Menu

        private void HexBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FormsHost.ContextMenu.IsOpen = true;
            }
        }

        private void CopyHex_Click(object sender, RoutedEventArgs e)
        {
            HexBox.CopyHex();
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            HexBox.Copy();
        }
        private void PasteText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HexBox.Paste();
            }
            catch (Exception ex)
            {
                SimpleMessageBox.ShowError(this, $"Paste failed becase {ex.Message}", "Paste Failed.");
            }
        }

        private void PasteHex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HexBox.PasteHex();
            }
            catch (Exception ex)
            {
                SimpleMessageBox.ShowError(this, $"Paste failed becase {ex.Message}", "Paste Failed.");
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            HexBox.SelectAll();
        }

        private void FollowPointer_Click(object sender, RoutedEventArgs e)
        {
            GetPeekPokeInfo(out var lastAddress, out var length);

            byte[] RawJumpAddress = new byte[8];

            //Grab 8 bytes
            for (int i = 7; i >= 0; i--)
                RawJumpAddress[i] = HexBox.ByteProvider.ReadByte(HexBox.SelectionStart + i);

            ulong address;
            try
            {
                address = BitConverter.ToUInt64(RawJumpAddress, 0);
            }
            catch
            {
                SimpleMessageBox.ShowError(this, "The memory was not a Pointer.", "The memory was not a Pointer.");
                return;
            }

            Task.Run(() =>
            {
                var data = TargetManager.SelectedTarget.Debug.ReadMemory(address, length);

                if (data != null && data.Length > 0)
                {
                    // Add the last address to the list.
                    JumpList.Add(lastAddress);

                    Dispatcher.Invoke(() =>
                    {
                        ReturnPointer.IsEnabled = true;

                        BaseAddress.FieldText = $"0x{address.ToString("X")}";
                        Offset.FieldText = $"0x0";

                        HexBox.VScrollBarVisible = true;
                        HexBox.ByteProvider = new DynamicByteProvider(data);
                        HexBox.VScrollBarVisible = false;
                    });
                }
            });
        }

        private void ReturnPointer_Click(object sender, RoutedEventArgs e)
        {
            if (JumpList.Count == 0)
                ReturnPointer.IsEnabled = false;

            GetPeekPokeInfo(out var address, out var length);
            Task.Run(() =>
            {
                var lastAddress = JumpList.Last();

                var data = TargetManager.SelectedTarget.Debug.ReadMemory(lastAddress, length);

                if (data != null && data.Length > 0)
                {
                    // Add the last address to the list.
                    JumpList.Remove(lastAddress);

                    Dispatcher.Invoke(() =>
                    {
                        // Disable if we are back where we started.
                        if (JumpList.Count == 0)
                            ReturnPointer.IsEnabled = false;

                        BaseAddress.FieldText = $"0x{lastAddress.ToString("X")}";
                        Offset.FieldText = $"0x0";

                        HexBox.VScrollBarVisible = true;
                        HexBox.ByteProvider = new DynamicByteProvider(data);
                        HexBox.VScrollBarVisible = false;
                    });
                }
            });
        }

        #endregion
    }
}
