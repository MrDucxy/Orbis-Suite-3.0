using OrbisLib2.Common.Database.Types;
using OrbisLib2.Common.Dispatcher;
using OrbisLib2.General;
using OrbisLib2.Targets;
using OrbisLib2.Dialog;
using SimpleUI.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Threading;

namespace OrbisLibraryManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SimpleWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherClient.Subscribe();

            Events.ProcAttach += Events_ProcAttach;
            Events.ProcDetach += Events_ProcDetach;
            Events.ProcDie += Events_ProcDie;
            Events.TargetStateChanged += Events_TargetStateChanged;

            // Update State
            Task.Run(() => EnableTargetOptions(TargetManager.SelectedTarget.Info.Status == TargetStatusType.APIAvailable));
        }

        private void RefreshLibraryList()
        {
            Task.Run(() =>
            {
                var libraryList = TargetManager.SelectedTarget.Debug.GetLibraries();

                if (libraryList != null && libraryList.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LibraryList.ItemsSource = libraryList;
                        LibraryList.Items.Refresh();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LibraryList.ItemsSource = null;
                    });
                }
            });
        }

        #region Events

        private void EnableProgram(bool Attached)
        {
            if(Attached)
            {
                RefreshLibraryList();
            }
            else
            {
                LibraryList.ItemsSource = null;
            }

            LoadPRX.IsEnabled = Attached;
            UnloadPRX.IsEnabled = Attached;
            ReloadPRX.IsEnabled = Attached;

            RefreshLibraries.IsEnabled = Attached;
            UnloadLibrary.IsEnabled = Attached;
            ReloadLibrary.IsEnabled = Attached;

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
            if(e.SendingTarget.IPAddress == TargetManager.SelectedTarget.IPAddress)
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
                if(proc != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CurrentDebuggingProccess.FieldText = $"{proc.Name}({e.NewProcessId})";
                        EnableProgram(true);
                    });
                }
            }
        }

        #endregion

        #region Context Menu

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshLibraryList();
        }

        private void UnloadLibrary_Click(object sender, RoutedEventArgs e)
        {
            var selectedLibrary = LibraryList.SelectedItems.Cast<LibraryInfo>().FirstOrDefault();
            if(selectedLibrary != null)
            {
                Task.Run(() =>
                {
                    TargetManager.SelectedTarget.Debug.UnloadLibrary((int)selectedLibrary.Handle);
                    Dispatcher.Invoke(() => RefreshLibraryList());
                });
            }
        }

        private void ReloadLibrary_Click(object sender, RoutedEventArgs e)
        {
            var selectedLibrary = LibraryList.SelectedItems.Cast<LibraryInfo>().FirstOrDefault();
            if (selectedLibrary != null)
            {
                Task.Run(() =>
                {
                    TargetManager.SelectedTarget.Debug.UnloadLibrary((int)selectedLibrary.Handle);
                    Thread.Sleep(2000);
                    TargetManager.SelectedTarget.Debug.LoadLibrary(selectedLibrary.Path);
                    Dispatcher.Invoke(() => RefreshLibraryList());
                });
            }
        }

        #endregion

        #region Buttons

        private void LoadPRX_Click(object sender, RoutedEventArgs e)
        {
            var libraryList = TargetManager.SelectedTarget.Debug.GetLibraries();
            var library = libraryList.Find(x => x.Path == SPRXPath.FieldText);
            if (library == null)
            {
                Task.Run(() =>
                {
                    TargetManager.SelectedTarget.Debug.LoadLibrary(SPRXPath.FieldText);
                    Dispatcher.Invoke(() => RefreshLibraryList());
                });
            }
            else
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), $"Could not load \"{SPRXPath}\" since it is already loaded.", "Error: Failed to load SPRX.");
            }
        }

        private void UnloadPRX_Click(object sender, RoutedEventArgs e)
        {
            var libraryList = TargetManager.SelectedTarget.Debug.GetLibraries();
            var library = libraryList.Find(x => x.Path == SPRXPath.FieldText);
            if (library != null)
            {
                Task.Run(() =>
                {
                    TargetManager.SelectedTarget.Debug.UnloadLibrary((int)library.Handle);
                    Dispatcher.Invoke(() => RefreshLibraryList());
                });
            }
            else
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), $"Could not unload \"{SPRXPath}\" since it is not loaded.", "Error: Failed to unload SPRX.");
            }
        }

        private void ReloadPRX_Click(object sender, RoutedEventArgs e)
        {
            var libraryList = TargetManager.SelectedTarget.Debug.GetLibraries();
            var library = libraryList.Find(x => x.Path == SPRXPath.FieldText);
            if (library != null)
            {
                Task.Run(() =>
                {
                    TargetManager.SelectedTarget.Debug.UnloadLibrary((int)library.Handle);
                    Thread.Sleep(2000);
                    TargetManager.SelectedTarget.Debug.LoadLibrary(library.Path);
                    Dispatcher.Invoke(() => RefreshLibraryList());
                });
            }
            else
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), $"Could not reload \"{SPRXPath}\" since it is not loaded.", "Error: Failed to reload SPRX.");
            }
        }

        private void AttachProcess_Click(object sender, RoutedEventArgs e)
        {
            SelectProcess.ShowDialog(GetWindow(this));
        }

        private void DetachProcess_Click(object sender, RoutedEventArgs e)
        {
            TargetManager.SelectedTarget.Debug.Detach();
        }

        private void LoadSomething_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            var currentTarget = TargetManager.SelectedTarget;
            var processList = TargetManager.SelectedTarget.Process.GetList();
            var process = processList.Find(x => x.ProcessId == currentTarget.Debug.GetCurrentProcessId());
            if(process != null)
            {
                if(process.AppId > 0)
                {
                    currentTarget.Application.Stop(process.TitleId);
                }
                else
                {
                    SimpleMessageBox.ShowError(Window.GetWindow(this), $"Could not kill process \"{process.Name}\" because Orbis Suite doesnt currently support killing processes with out an appId.", "Error: Could not kill this process.");
                }
            }
        }

        private void RestartTarget_Click(object sender, RoutedEventArgs e)
        {
            TargetManager.SelectedTarget.Reboot();
        }

        private void ShutdownTarget_Click(object sender, RoutedEventArgs e)
        {
            TargetManager.SelectedTarget.Shutdown();
        }

        #endregion
    }
}
