using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OrbisNeighborHood.MVVM.ViewModel;
using OrbisSuite;
using OrbisSuite.Common.Database;
using SimpleUI.Controls;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetPanel : UserControl
    {
        private TargetInfo _thisTarget;

        public event EventHandler<RoutedEventArgs>? TargetChanged;

        public TargetPanel(string TargetName)
        {
            InitializeComponent();

            var Target = OrbisLib.Instance.Targets[TargetName];
            _thisTarget = Target.Info;

            this.TargetName = Target.Info.Name;
            TargetStatus = Target.Info.Status;
            ConsoleModel = Target.Info.ModelType;
            IsDefault = Target.Info.IsDefault;
            FirmwareVersion = string.Format("{0:N2}", (double)Target.Info.Firmware / 100);
            SDKVersion = Target.Info.SDKVersion;
            IPAddress = Target.Info.IPAddress;
            ConsoleName = Target.Info.ConsoleName;
            ConsoleType = Target.Info.ConsoleType.ToString();
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
            /*if ((bool)e.NewValue)
                ((TargetPanel)d).DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Default.ico"));
            else
                ((TargetPanel)d).DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/NotDefault.ico"));*/

            if ((bool)e.NewValue)
            {
                ((TargetPanel)d).DefaultTargetElement.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                ((TargetPanel)d).DefaultTargetElement.Cursor = Cursors.Arrow;
            }
                
            else
            {
                ((TargetPanel)d).DefaultTargetElement.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
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
            set
            {
                SetValue(FirmwareVersionProperty, value);
            }
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

        private string ConsoleType
        {
            get { return (string)GetValue(ConsoleTypeProperty); }
            set { SetValue(ConsoleTypeProperty, value); }
        }

        private static readonly DependencyProperty ConsoleTypeProperty =
            DependencyProperty.Register("ConsoleType", typeof(string), typeof(TargetPanel), new PropertyMetadata(string.Empty));

        #endregion

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = SimpleMessageBox.ShowWarning(Window.GetWindow(this), "Are you sure you want to delete this target?", "Delete this Target?", SimpleUI.SimpleMessageBoxButton.YesNo);
            if(result == SimpleUI.SimpleMessageBoxResult.Yes)
            {
                _thisTarget.Remove();
                TargetChanged?.Invoke(this, e);
            }  
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance != null)
            {
                var editTargetViewModel = MainViewModel.Instance.EditTargetVM;
                editTargetViewModel.TargetChanged += EditTargetVM_TargetChanged;
                editTargetViewModel.CurrentTarget = _thisTarget;
                MainViewModel.Instance.CurrentView = editTargetViewModel;
            }
        }

        private void EditTargetVM_TargetChanged(object? sender, RoutedEventArgs e)
        {
            TargetChanged?.Invoke(this, e);
        }

        private void DefaultTargetElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_thisTarget.IsDefault)
            {
                _thisTarget.IsDefault = true;
                _thisTarget.Save();
                TargetChanged?.Invoke(this, e);
            }
        }
    }
}
