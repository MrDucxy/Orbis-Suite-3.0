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
using OrbisSuite;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetView : UserControl
    {
        public TargetView(string TargetName)
        {
            InitializeComponent();

            var Target = OrbisLib.Instance.Targets[TargetName];
            this.TargetName = TargetName;

            if (Target.Info.APIAvailable)
                TargetStatus = TargetStatusType.APIAvailable;
            else if (Target.Info.Available)
                TargetStatus = TargetStatusType.Online;
            else
                TargetStatus = TargetStatusType.Offline;

            // ConsoleModel
            // CUH-1XXXX Fat
            // CUH-2XXXX Slim
            // CUH-7XXXX Pro

            if (Target.Info.Model.Length > 4)//(Regex.Match(Target.Info.Model, @"CUH-\d{5}\w{4}").Success)
            {
                switch (char.IsDigit(Target.Info.Model[4]) ? int.Parse(Target.Info.Model[4].ToString()) : 0)
                {
                    case 1:
                        ConsoleModel = ConsoleModelType.Fat;
                        break;

                    case 2:
                        ConsoleModel = ConsoleModelType.Slim;
                        break;

                    case 7:
                        ConsoleModel = ConsoleModelType.Pro;
                        break;


                    default:
                        ConsoleModel = ConsoleModelType.Fat;
                        break;
                }
            }

            IsDefault = Target.Info.Default;
            FirmwareVersion = string.Format("{0:N2}", (double)Target.Info.Firmware / 100);
            SDKVersion = Target.Info.SDKVersion;
            IPAddress = Target.Info.IPAddr;
            ConsoleName = Target.Info.ConsoleName;
            ConsoleType = Target.Info.ConsoleType;
        }

        #region Properties

        private string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }

        private static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register("TargetName", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        private enum TargetStatusType
        {
            None,
            Offline,
            Online,
            APIAvailable
        };

        private TargetStatusType TargetStatus
        {
            get { return (TargetStatusType)GetValue(TargetStatusProperty); }
            set { SetValue(TargetStatusProperty, value); }
        }

        private static readonly DependencyProperty TargetStatusProperty =
            DependencyProperty.Register("TargetStatus", typeof(TargetStatusType), typeof(TargetView), new PropertyMetadata(TargetStatusType.None, TargetStatusProperty_Changed));

        private static void TargetStatusProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((TargetStatusType)e.NewValue)
            {
                case TargetStatusType.Offline:
                    ((TargetView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((TargetView)d).TargetStatusElement.ToolTip = "Offline";
                    break;

                case TargetStatusType.Online:
                    ((TargetView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    ((TargetView)d).TargetStatusElement.ToolTip = "Online";
                    break;

                case TargetStatusType.APIAvailable:
                    ((TargetView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    ((TargetView)d).TargetStatusElement.ToolTip = "Online & API Available";
                    break;

                default:
                    ((TargetView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((TargetView)d).TargetStatusElement.ToolTip = "Unknown";
                    break;
            }
        }

        private bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        private static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register("IsDefault", typeof(bool), typeof(TargetView), new PropertyMetadata(false, IsDefaultProperty_Changed));

        private static void IsDefaultProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                ((TargetView)d).DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Default.ico"));
            else
                ((TargetView)d).DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/NotDefault.ico"));
        }

        private enum ConsoleModelType
        {
            Unknown,
            Fat,
            Slim,
            Pro
        };

        private ConsoleModelType ConsoleModel
        {
            get { return (ConsoleModelType)GetValue(ConsoleModelProperty); }
            set { SetValue(ConsoleModelProperty, value); }
        }

        private static readonly DependencyProperty ConsoleModelProperty =
            DependencyProperty.Register("ConsoleModel", typeof(ConsoleModelType), typeof(TargetView), new PropertyMetadata(ConsoleModelType.Unknown, ConsoleModelProperty_Changed));

        private static void ConsoleModelProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((ConsoleModelType)e.NewValue)
            {
                case ConsoleModelType.Fat:
                    ((TargetView)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
                    break;

                case ConsoleModelType.Slim:
                    ((TargetView)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Slim.png"));
                    break;

                case ConsoleModelType.Pro:
                    ((TargetView)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Pro.png"));
                    break;

                default:
                    ((TargetView)d).ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
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
            DependencyProperty.Register("FirmwareVersion", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        private string SDKVersion
        {
            get { return (string)GetValue(SDKVersionProperty); }
            set { SetValue(SDKVersionProperty, value); }
        }

        private static readonly DependencyProperty SDKVersionProperty =
            DependencyProperty.Register("SDKVersion", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        private string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        private static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        private string ConsoleName
        {
            get { return (string)GetValue(ConsoleNameProperty); }
            set { SetValue(ConsoleNameProperty, value); }
        }

        private static readonly DependencyProperty ConsoleNameProperty =
            DependencyProperty.Register("ConsoleName", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        private string ConsoleType
        {
            get { return (string)GetValue(ConsoleTypeProperty); }
            set { SetValue(ConsoleTypeProperty, value); }
        }

        private static readonly DependencyProperty ConsoleTypeProperty =
            DependencyProperty.Register("ConsoleType", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        #endregion
    }
}
