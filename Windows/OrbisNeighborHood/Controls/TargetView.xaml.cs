using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetView : UserControl
    {
        public TargetView()
        {
            InitializeComponent();
        }

        private enum TargetStatusType
        {
            Offline,
            Online,
            APIAvailable
        };

        // Target Info
        public int TargetStatus
        {
            get { return (int)GetValue(TargetStatusProperty); }
            set 
            { 
                SetValue(TargetStatusProperty, value); 
                switch (value)
                {
                    case (int)TargetStatusType.Offline:
                        TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        TargetStatusElement.ToolTip = "Offline";
                        break;

                    case (int)TargetStatusType.Online:
                        TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                        TargetStatusElement.ToolTip = "Online";
                        break;

                    case (int)TargetStatusType.APIAvailable:
                        TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                        TargetStatusElement.ToolTip = "Online &amp; API Available";
                        break;

                    default:
                        TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        TargetStatusElement.ToolTip = "Unknown";
                        break;
                }
            }
        }

        public static readonly DependencyProperty TargetStatusProperty =
            DependencyProperty.Register("TargetStatus", typeof(int), typeof(TargetView), new PropertyMetadata(0));

        public string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set { SetValue(TargetNameProperty, value); }
        }

        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register("TargetName", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        public bool IsDefault
        {
            get { return (bool)GetValue(IsDefaultProperty); }
            set 
            { 
                SetValue(IsDefaultProperty, value);
                if (value)
                    DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Default.ico"));
                else 
                    DefaultTargetElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/NotDefault.ico"));
            }
        }

        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register("IsDefault", typeof(bool), typeof(TargetView), new PropertyMetadata(false));


        // Info Field
        private enum ConsoleModelType
        {
            Fat,
            Slim,
            Pro
        };

        public int ConsoleModel
        {
            get { return (int)GetValue(ConsoleModelProperty); }
            set 
            { 
                SetValue(ConsoleModelProperty, value); 
                switch (value)
                {
                    case (int)ConsoleModelType.Fat:
                        ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
                        break;

                    case (int)ConsoleModelType.Slim:
                        ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Slim.png"));
                        break;

                    case (int)ConsoleModelType.Pro:
                        ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Pro.png"));
                        break;

                    default:
                        ConsoleImageElement.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/Consoles/Fat.png"));
                        break;
                }
            }
        }

        public static readonly DependencyProperty ConsoleModelProperty =
            DependencyProperty.Register("ConsoleModel", typeof(int), typeof(TargetView), new PropertyMetadata(0));

        public string FirmwareVersion
        {
            get { return (string)GetValue(FirmwareVersionProperty); }
            set 
            { 
                SetValue(FirmwareVersionProperty, value); 
            }
        }

        public static readonly DependencyProperty FirmwareVersionProperty =
            DependencyProperty.Register("FirmwareVersion", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        public string SDKVersion
        {
            get { return (string)GetValue(SDKVersionProperty); }
            set { SetValue(SDKVersionProperty, value); }
        }

        public static readonly DependencyProperty SDKVersionProperty =
            DependencyProperty.Register("SDKVersion", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        public string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        public static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        public string ConsoleName
        {
            get { return (string)GetValue(ConsoleNameProperty); }
            set { SetValue(ConsoleNameProperty, value); }
        }

        public static readonly DependencyProperty ConsoleNameProperty =
            DependencyProperty.Register("ConsoleName", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));

        public string ConsoleType
        {
            get { return (string)GetValue(ConsoleTypeProperty); }
            set { SetValue(ConsoleTypeProperty, value); }
        }

        public static readonly DependencyProperty ConsoleTypeProperty =
            DependencyProperty.Register("ConsoleType", typeof(string), typeof(TargetView), new PropertyMetadata(string.Empty));
    }
}
