﻿using OrbisLib2.Common.Database.Types;
using OrbisLib2.General;
using OrbisLib2.Targets;
using OrbisNeighborHood.MVVM.ViewModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            Events.DBTouched += Events_DBTouched;
            Events.TargetStateChanged += Events_TargetStateChanged;

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
            var CurrentTarget = TargetManager.SelectedTarget;

            if (CurrentTarget != null)
            {
                TitleString = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;
                TargetName = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;
                TargetStatus = CurrentTarget.Info.Status;
                FirmwareVersion = CurrentTarget.Info.SoftwareVersion;
                SDKVersion = CurrentTarget.Info.SDKVersion;
                IPAddress = CurrentTarget.IPAddress;
                ConsoleName = CurrentTarget.Info.ConsoleName;
                PayloadPort = CurrentTarget.PayloadPort.ToString();

                // Storage Stats.
                HDDFreeSpace = Utilities.BytesToString(CurrentTarget.Info.HDDFreeSpace);
                HDDUsedSpace = Utilities.BytesToString(CurrentTarget.Info.HDDUsedSpace);
                HDDTotalSpace = Utilities.BytesToString(CurrentTarget.Info.HDDTotalSpace);

                if (CurrentTarget.Info.HDDTotalSpace != 0)
                    StorageUsagePercentage = (int)(((double)CurrentTarget.Info.HDDUsedSpace / (double)CurrentTarget.Info.HDDTotalSpace) * 100.0);
                else
                    StorageUsagePercentage = 0;

                // System Stats.
                CPUTemp = $"{CurrentTarget.Info.CPUTemp} °C";
                SOCTemp = $"{CurrentTarget.Info.SOCTemp} °C";
                ThreadCount = CurrentTarget.Info.ThreadCount;
                AverageCPUUsage = CurrentTarget.Info.AverageCPUUsage;
                TopCore = $"Core {CurrentTarget.Info.BusyCore}";
                RamUsage = $"{CurrentTarget.Info.RamUsage} MB";
                VRamUsage = $"{CurrentTarget.Info.VRamUsage} MB";

                if (CurrentTarget.Info.BigAppTitleID == null || !Regex.IsMatch(CurrentTarget.Info.BigAppTitleID, @"CUSA\d{5}"))
                {
                    TitleName = "Unknown Title";
                    TitleId = "-";
                    ProcessName = "-";
                    TitleImage.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/DefaultTitleIcon.png"));
                }
                else
                {
                    var Title = new TMDB(CurrentTarget.Info.BigAppTitleID);
                    Regex rgx = new Regex(@"[^0-9a-zA-Z +.:']");
                    TitleName = Title.Names.First();
                    TitleId = Title.NPTitleID;
                    ProcessName = $"{CurrentTarget.Info.BigAppProcessName} ({CurrentTarget.Info.BigAppPid})";
                    var test = Title.BGM;
                    TitleImage.Source = new BitmapImage(new Uri(Title.Icons.First()));
                }
            }
        }

        #endregion

        #region Target Info

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var CurrentTarget = TargetManager.SelectedTarget;

            if (MainViewModel.Instance != null)
            {
                var editTargetViewModel = MainViewModel.Instance.EditTargetVM;
                editTargetViewModel.CurrentTarget = CurrentTarget.SavedTarget.Clone();
                editTargetViewModel.CallingVM = MainViewModel.Instance.DashboardHomeVM;
                MainViewModel.Instance.CurrentView = editTargetViewModel;
            }
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
