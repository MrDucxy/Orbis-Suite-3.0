using OrbisSuite;
using OrbisSuite.Common.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        #region Constructor

        public DashboardView()
        {
            InitializeComponent();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;

            RefreshTargetInfo();
        }

        #endregion

        #region Properties

        public string TitleString
        {
            get { return (string)GetValue(TitleStringProperty); }
            set
            {
                SetValue(TitleStringProperty, $"Dashboard ({value})");
            }
        }

        public static readonly DependencyProperty TitleStringProperty =
            DependencyProperty.Register("TitleString", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        #region Target Info

        public string TargetName
        {
            get { return (string)GetValue(TargetNameProperty); }
            set
            {
                SetValue(TargetNameProperty, $"{value} Info");
            }
        }

        public static readonly DependencyProperty TargetNameProperty =
            DependencyProperty.Register("TargetName", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        private TargetStatusType TargetStatus
        {
            get { return (TargetStatusType)GetValue(TargetStatusProperty); }
            set { SetValue(TargetStatusProperty, value); }
        }

        private static readonly DependencyProperty TargetStatusProperty =
            DependencyProperty.Register("TargetStatus", typeof(TargetStatusType), typeof(DashboardView), new PropertyMetadata(TargetStatusType.None, TargetStatusProperty_Changed));

        private static void TargetStatusProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch ((TargetStatusType)e.NewValue)
            {
                case TargetStatusType.Offline:
                    ((DashboardView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((DashboardView)d).TargetStatusElement.ToolTip = "Offline";
                    break;

                case TargetStatusType.Online:
                    ((DashboardView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    ((DashboardView)d).TargetStatusElement.ToolTip = "Online";
                    break;

                case TargetStatusType.APIAvailable:
                    ((DashboardView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    ((DashboardView)d).TargetStatusElement.ToolTip = "Online & API Available";
                    break;

                default:
                    ((DashboardView)d).TargetStatusElement.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    ((DashboardView)d).TargetStatusElement.ToolTip = "Unknown";
                    break;
            }
        }
        private string FirmwareVersion
        {
            get { return (string)GetValue(FirmwareVersionProperty); }
            set { SetValue(FirmwareVersionProperty, value); }
        }

        public static readonly DependencyProperty FirmwareVersionProperty =
            DependencyProperty.Register("FirmwareVersion", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        private string SDKVersion
        {
            get { return (string)GetValue(SDKVersionProperty); }
            set { SetValue(SDKVersionProperty, value); }
        }

        private static readonly DependencyProperty SDKVersionProperty =
            DependencyProperty.Register("SDKVersion", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        private string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        private static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        private string ConsoleName
        {
            get { return (string)GetValue(ConsoleNameProperty); }
            set { SetValue(ConsoleNameProperty, value); }
        }

        private static readonly DependencyProperty ConsoleNameProperty =
            DependencyProperty.Register("ConsoleName", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public string PayloadPort
        {
            get { return (string)GetValue(PayloadPortProperty); }
            set { SetValue(PayloadPortProperty, value); }
        }

        public static readonly DependencyProperty PayloadPortProperty =
            DependencyProperty.Register("PayloadPort", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        #endregion

        #region Title Info

        public string TitleName
        {
            get { return (string)GetValue(TitleNameProperty); }
            set { SetValue(TitleNameProperty, value); }
        }

        public static readonly DependencyProperty TitleNameProperty =
            DependencyProperty.Register("TitleName", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public string TitleId
        {
            get { return (string)GetValue(TitleIdProperty); }
            set { SetValue(TitleIdProperty, value); }
        }

        public static readonly DependencyProperty TitleIdProperty =
            DependencyProperty.Register("TitleId", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public string ProcessName
        {
            get { return (string)GetValue(ProcessNameProperty); }
            set { SetValue(ProcessNameProperty, value); }
        }

        public static readonly DependencyProperty ProcessNameProperty =
            DependencyProperty.Register("ProcessName", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        #endregion

        #endregion

        #region Events / Refresh Target

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargetInfo(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargetInfo(); });
        }

        private void RefreshTargetInfo()
        {
            var CurrentTarget = OrbisLib.Instance.SelectedTarget.Info;

            if (CurrentTarget != null)
            {
                TitleString = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;
                TargetName = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;
                TargetStatus = CurrentTarget.Status;
                FirmwareVersion = CurrentTarget.SoftwareVersion;
                SDKVersion = CurrentTarget.SDKVersion;
                IPAddress = CurrentTarget.IPAddress;
                ConsoleName = CurrentTarget.ConsoleName;
                PayloadPort = CurrentTarget.PayloadPort.ToString();

                // Storage Stats.
                HDDFreeSpace = Utilities.BytesToString(CurrentTarget.HDDFreeSpace);
                HDDUsedSpace = Utilities.BytesToString(CurrentTarget.HDDUsedSpace);
                HDDTotalSpace = Utilities.BytesToString(CurrentTarget.HDDTotalSpace);

                StorageUsagePercentage = (int)(((double)CurrentTarget.HDDUsedSpace / (double)CurrentTarget.HDDTotalSpace) * 100.0);

                // System Stats.
                CPUTemp = $"{CurrentTarget.CPUTemp} °C";
                SOCTemp = $"{CurrentTarget.SOCTemp} °C";
                ThreadCount = CurrentTarget.ThreadCount;
                AverageCPUUsage = CurrentTarget.AverageCPUUsage;
                TopCore = $"Core {CurrentTarget.BusyCore}";
                RamUsage = $"{CurrentTarget.RamUsage} MB";
                VRamUsage = $"{CurrentTarget.VRamUsage} MB";

                if (CurrentTarget.CurrentTitleID == null || !Regex.IsMatch(CurrentTarget.CurrentTitleID, @"CUSA\d{5}"))
                {
                    TitleName = "Unknown Title";
                    TitleId = "-";
                    ProcessName = "-";
                    TitleImage.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/DefaultTitleIcon.png"));
                }
                else
                {
                    var Title = new TMDB(CurrentTarget.CurrentTitleID);
                    Regex rgx = new Regex(@"[^0-9a-zA-Z +.:']");
                    TitleName = Title.Names.First();
                    TitleId = Title.NPTitleID;
                    ProcessName = "-"; // TODO: In the future we will need to pull the processname(processId) for the current big app.
                    var test = Title.BGM;
                    TitleImage.Source = new BitmapImage(new Uri(Title.Icons.First()));
                }
            }
        }

        #endregion

        #region Target Info

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Title Info

        private void LaunchDebugger_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Laucnh with select process.
        }

        private void LaunchPeekPoke_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Laucnh with select process.
        }

        private void LaunchLibraryManager_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Laucnh with select process.
        }

        #endregion

        #region HDD Info

        public string HDDFreeSpace
        {
            get { return (string)GetValue(HDDFreeSpaceProperty); }
            set { SetValue(HDDFreeSpaceProperty, value); }
        }

        public static readonly DependencyProperty HDDFreeSpaceProperty =
            DependencyProperty.Register("HDDFreeSpace", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public string HDDUsedSpace
        {
            get { return (string)GetValue(HDDUsedSpaceProperty); }
            set { SetValue(HDDUsedSpaceProperty, value); }
        }

        public static readonly DependencyProperty HDDUsedSpaceProperty =
            DependencyProperty.Register("HDDUsedSpace", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public string HDDTotalSpace
        {
            get { return (string)GetValue(HDDTotalSpaceProperty); }
            set { SetValue(HDDTotalSpaceProperty, value); }
        }

        public static readonly DependencyProperty HDDTotalSpaceProperty =
            DependencyProperty.Register("HDDTotalSpace", typeof(string), typeof(DashboardView), new PropertyMetadata(string.Empty));

        public int StorageUsagePercentage
        {
            get { return (int)GetValue(StorageUsagePercentageProperty); }
            set { SetValue(StorageUsagePercentageProperty, value); }
        }

        public static readonly DependencyProperty StorageUsagePercentageProperty =
            DependencyProperty.Register("StorageUsagePercentage", typeof(int), typeof(DashboardView), new PropertyMetadata(0));

        #endregion

        #region System Stats

        public string CPUTemp
        {
            get { return (string)GetValue(CPUTempProperty); }
            set { SetValue(CPUTempProperty, value); }
        }

        public static readonly DependencyProperty CPUTempProperty =
            DependencyProperty.Register("CPUTemp", typeof(string), typeof(DashboardView), new PropertyMetadata("- °C"));

        public string SOCTemp
        {
            get { return (string)GetValue(SOCTempProperty); }
            set { SetValue(SOCTempProperty, value); }
        }

        public static readonly DependencyProperty SOCTempProperty =
            DependencyProperty.Register("SOCTemp", typeof(string), typeof(DashboardView), new PropertyMetadata("- °C"));

        public int ThreadCount
        {
            get { return (int)GetValue(ThreadCountProperty); }
            set { SetValue(ThreadCountProperty, value); }
        }

        public static readonly DependencyProperty ThreadCountProperty =
            DependencyProperty.Register("ThreadCount", typeof(int), typeof(DashboardView), new PropertyMetadata(0));

        public float AverageCPUUsage
        {
            get { return (float)GetValue(AverageCPUUsageProperty); }
            set { SetValue(AverageCPUUsageProperty, value); }
        }

        public static readonly DependencyProperty AverageCPUUsageProperty =
            DependencyProperty.Register("AverageCPUUsage", typeof(float), typeof(DashboardView), new PropertyMetadata(0.0f));

        public string TopCore
        {
            get { return (string)GetValue(TopCoreProperty); }
            set { SetValue(TopCoreProperty, value); }
        }

        public static readonly DependencyProperty TopCoreProperty =
            DependencyProperty.Register("TopCore", typeof(string), typeof(DashboardView), new PropertyMetadata("-"));

        public string RamUsage
        {
            get { return (string)GetValue(RamUsageProperty); }
            set { SetValue(RamUsageProperty, value); }
        }

        public static readonly DependencyProperty RamUsageProperty =
            DependencyProperty.Register("RamUsage", typeof(string), typeof(DashboardView), new PropertyMetadata("-"));

        public string VRamUsage
        {
            get { return (string)GetValue(VRamUsageProperty); }
            set { SetValue(VRamUsageProperty, value); }
        }

        public static readonly DependencyProperty VRamUsageProperty =
            DependencyProperty.Register("VRamUsage", typeof(string), typeof(DashboardView), new PropertyMetadata("-"));

        #endregion

        #endregion

        #region App Launcher Buttons



        #endregion
    }
}
