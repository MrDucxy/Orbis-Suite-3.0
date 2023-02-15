using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OrbisNeighborHood.MVVM.ViewModel;
using SimpleUI.Controls;
using OrbisLib2.Common.Database.Types;
using OrbisLib2.Common.API;
using OrbisLib2.Targets;
using OrbisLib2.Common.Database;
using System.Threading.Tasks;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetPanel : UserControl
    {
        private readonly Target _thisTarget;

        public event EventHandler<RoutedEventArgs>? TargetChanged;

        public TargetPanel(string TargetName)
        {
            InitializeComponent();

            _thisTarget = TargetManager.GetTarget(TargetName);
            if(_thisTarget != null )
            {
                this.TargetName = _thisTarget.Name;
                TargetStatus = _thisTarget.Info.Status;
                ConsoleModel = _thisTarget.Info.ModelType;
                IsDefault = _thisTarget.IsDefault;
                FirmwareVersion = _thisTarget.Info.SoftwareVersion;
                SDKVersion = _thisTarget.Info.SDKVersion;
                IPAddress = _thisTarget.IPAddress;
                ConsoleName = _thisTarget.Info.ConsoleName;
                PayloadPort = _thisTarget.PayloadPort.ToString();

                LocateTarget.IsEnabled = _thisTarget.Info.IsAPIAvailable;
                SendPayload.IsEnabled = _thisTarget.Info.IsAvailable;
                RestartTarget.IsEnabled = _thisTarget.Info.IsAPIAvailable;
                ShutdownTarget.IsEnabled = _thisTarget.Info.IsAPIAvailable;
                SuspendTarget.IsEnabled = _thisTarget.Info.IsAPIAvailable;
            }
            else
            {
                throw new Exception("TargetPanel(): Target not found when it should have been!");
            }    
        }

        #region Properties

        private string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }

        private static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register("TargetName", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        private TargetStatusType TargetStatus
        {
            get { return (TargetStatusType)GetValue(TargetStatusProperty); }
            set { SetValue(TargetStatusProperty, value); }
        }

        private static readonly DependencyProperty TargetStatusProperty =
            DependencyProperty.Register("TargetStatus", typeof(TargetStatusType), typeof(TargetPanel), new PropertyMetadata(TargetStatusType.None, TargetStatusProperty_Changed));

        private static void TargetStatusProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((TargetStatusType)e.NewValue)
            {
                case TargetStatusType.Offline:
                    ((TargetPanel)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((TargetPanel)d).TargetStatusElement.ToolTip = "Offline";
                    break;

                case TargetStatusType.Online:
                    ((TargetPanel)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    ((TargetPanel)d).TargetStatusElement.ToolTip = "Online";
                    break;

                case TargetStatusType.APIAvailable:
                    ((TargetPanel)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    ((TargetPanel)d).TargetStatusElement.ToolTip = "Online & API Available";
                    break;

                default:
                    ((TargetPanel)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((TargetPanel)d).TargetStatusElement.ToolTip = "Unknown";
                    break;
            }
        }

        private bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        private static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register("IsDefault", typeof(bool), typeof(TargetPanel), new PropertyMetadata(false, IsDefaultProperty_Changed));

        private static void IsDefaultProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ((TargetPanel)d).DefaultTargetElement.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                ((TargetPanel)d).DefaultTargetElement.Cursor = Cursors.Arrow;
            }
                
            else
            {
                ((TargetPanel)d).DefaultTargetElement.Foreground = new SolidColorBrush(Color.FromRgb(69, 73, 74));
                ((TargetPanel)d).DefaultTargetElement.Cursor = Cursors.Hand;
            }
        }

        private ConsoleModelType ConsoleModel
        {
            get { return (ConsoleModelType)GetValue(ConsoleModelProperty); }
            set { SetValue(ConsoleModelProperty, value); }
        }

        private static readonly DependencyProperty ConsoleModelProperty =
            DependencyProperty.Register("ConsoleModel", typeof(ConsoleModelType), typeof(TargetPanel), new PropertyMetadata(ConsoleModelType.Unknown, ConsoleModelProperty_Changed));

        private static void ConsoleModelProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((ConsoleModelType)e.NewValue)
            {
                case ConsoleModelType.Fat:
                    ((TargetPanel)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
                    break;

                case ConsoleModelType.Slim:
                    ((TargetPanel)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Slim.png"));
                    break;

                case ConsoleModelType.Pro:
                    ((TargetPanel)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Pro.png"));
                    break;

                default:
                    ((TargetPanel)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
                    break;
            }
        }

        private string FirmwareVersion
        {
            get { return (string)GetValue(FirmwareVersionProperty); }
            set { SetValue(FirmwareVersionProperty, value); }
        }

        public static readonly DependencyProperty FirmwareVersionProperty =
            DependencyProperty.Register("FirmwareVersion", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        private string SDKVersion
        {
            get { return (string)GetValue(SDKVersionProperty); }
            set { SetValue(SDKVersionProperty, value); }
        }

        private static readonly DependencyProperty SDKVersionProperty =
            DependencyProperty.Register("SDKVersion", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        private string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        private static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        private string ConsoleName
        {
            get { return (string)GetValue(ConsoleNameProperty); }
            set { SetValue(ConsoleNameProperty, value); }
        }

        private static readonly DependencyProperty ConsoleNameProperty =
            DependencyProperty.Register("ConsoleName", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        public string PayloadPort
        {
            get { return (string)GetValue(PayloadPortProperty); }
            set { SetValue(PayloadPortProperty, value); }
        }

        public static readonly DependencyProperty PayloadPortProperty =
            DependencyProperty.Register("PayloadPort", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        #endregion

        #region Buttons

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = SimpleMessageBox.ShowWarning(Window.GetWindow(this), "Are you sure you want to delete this target?", "Delete this Target?", SimpleUI.SimpleMessageBoxButton.YesNo);
            if(result == SimpleUI.SimpleMessageBoxResult.Yes)
            {
                TargetManager.DeleteTarget(_thisTarget.Name);
                TargetChanged?.Invoke(this, e);
            }  
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance != null)
            {
                var editTargetViewModel = MainViewModel.Instance.EditTargetVM;
                editTargetViewModel.TargetChanged += EditTargetVM_TargetChanged;
                editTargetViewModel.CurrentTarget = _thisTarget.SavedTarget.Clone();
                editTargetViewModel.CallingVM = MainViewModel.Instance.TargetVM;
                MainViewModel.Instance.CurrentView = editTargetViewModel;
            }
        }

        private void EditTargetVM_TargetChanged(object? sender, RoutedEventArgs e)
        {
            TargetChanged?.Invoke(this, e);
        }

        private void DefaultTargetElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var savedTarget = _thisTarget.SavedTarget;
            if (!savedTarget.IsDefault)
            {
                savedTarget.IsDefault = true;
                savedTarget.Save();
                TargetChanged?.Invoke(this, e);
            }
        }

        private void LocateTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                _thisTarget.Buzzer(BuzzerType.RingThree);
            });
        }

        private void SendPayload_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    string PayloadPath = string.Empty;
                    var openFileDialog = new OpenFileDialog();

                    openFileDialog.Title = "Open BIN File...";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.InitialDirectory = Properties.Settings.Default.LastPayloadPath;
                    openFileDialog.Filter = "BIN files (*.BIN)|*.BIN";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == true)
                    {
                        PayloadPath = openFileDialog.FileName;
                        Properties.Settings.Default.LastPayloadPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                        Properties.Settings.Default.Save();
                    }
                    else
                        return;

                    FileStream fPayload = File.Open(PayloadPath, FileMode.Open);
                    if (fPayload.CanRead)
                    {
                        byte[] PayloadBuffer = new byte[fPayload.Length];

                        if (fPayload.Read(PayloadBuffer, 0, (int)fPayload.Length) == fPayload.Length)
                        {
                            if (!_thisTarget.Payload.InjectPayload(PayloadBuffer))
                            {
                                SimpleMessageBox.ShowError(Window.GetWindow(this), "Failed to send payload to target please try again.", "Error: Failed to inject payload.");
                            }
                            else
                            {
                                SimpleMessageBox.ShowInformation(Window.GetWindow(this), "The payload has been sucessfully sent.", "Payload Sent!");
                            }
                        }
                        else
                            SimpleMessageBox.ShowError(Window.GetWindow(this), "Failed read payload from disc to target please try again.", "Error: Failed to inject payload.");
                    }

                    fPayload.Close();
                }
                catch
                {

                }
            });
        }

        private void RestartTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => _thisTarget.Reboot());
        }

        private void ShutdownTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => _thisTarget.Shutdown());
        }

        private void SuspendTarget_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => _thisTarget.Suspend());
        }

        #endregion
    }
}
