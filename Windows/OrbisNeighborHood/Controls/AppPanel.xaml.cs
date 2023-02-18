using OrbisLib2.Common.API;
using OrbisLib2.Common.Database.App;
using OrbisLib2.Common.Database.Types;
using OrbisLib2.General;
using OrbisLib2.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static OrbisLib2.Targets.Application;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for AppPanel.xaml
    /// </summary>
    public partial class AppPanel : UserControl
    {
        public AppBrowse _App;
        private string _AppVersion;

        private AppState AppState = AppState.STATE_NOT_RUNNING;
        private VisibilityType Visible = VisibilityType.VT_NONE;

        public AppPanel(AppBrowse App, string AppVersion)
        {
            InitializeComponent();

            _App = App;
            _AppVersion = AppVersion;

            // Update Application information.
            Update(App, AppVersion);

            // Start background task to periodically check if this application is currently running.
            Task.Run(() => UpdateApp());

            var currentTarget = TargetManager.SelectedTarget;
            EnableTargetOptions(currentTarget.Info.Status == TargetStatusType.APIAvailable);

            Events.TargetStateChanged += Events_TargetStateChanged;
            Events.DBTouched += Events_DBTouched;
            Events.SelectedTargetChanged += Events_SelectedTargetChanged; ;
        }

        private void EnableTargetOptions(bool state)
        {
            Dispatcher.Invoke(() =>
            {
                if (!state)
                {
                    StartStop.IsEnabled = state;
                    SuspendResume.IsEnabled = state;
                }
                Visibility.IsEnabled = state;
                ChangeIcon.IsEnabled = state;
                SetAsFocus.IsEnabled = state;
                Delete.IsEnabled = state;

                if (!state) 
                {
                    Update(_App, _AppVersion);
                }
            });
        }

        private void Events_SelectedTargetChanged(object? sender, SelectedTargetChangedEvent e)
        {
            var currentTarget = TargetManager.SelectedTarget;
            EnableTargetOptions(currentTarget.Info.Status == TargetStatusType.APIAvailable);
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            var currentTarget = TargetManager.SelectedTarget;
            EnableTargetOptions(currentTarget.Info.Status == TargetStatusType.APIAvailable);
        }

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            EnableTargetOptions(e.State == TargetStateChangedEvent.TargetState.APIAvailable);
        }

        public void Update(AppBrowse App, string AppVersion)
        {
            // Set the Info about this application.
            ApplicationNameElement.Text = App.TitleName;
            TitleIdElement.FieldText = App.TitleId;
            VersionElement.FieldText = AppVersion.TrimStart('0');
            TypeElement.FieldText = $"{App.UICategory} ({App.Category})";
            SizeElement.FieldText = App.ContentSize <= 0 ? "N/A" : Utilities.BytesToString(App.ContentSize);

            // Get the path to our icon and make sure it exists.
            string iconPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Orbis Suite\AppCache\{App.TitleId}\icon0.png";
            if(!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath) && new FileInfo(iconPath).Length > 0)
            {
                // Load and cache image in memory.
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(iconPath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                // Set image to element.
                IconImage.Source = image;
            }

            if (!DateTime.TryParse(App.InstallDate, out DateTime InstallDate))
                InstallDate = DateTime.MinValue;

            InstallDateElement.FieldText = InstallDate.ToString("ddd dd MMM yyy hh:mm tt");

            // Set the tool tips.
            StartStop.ToolTip = $"Start {App.TitleName}.";
            Visibility.ToolTip = $"Hide {App.TitleName} from Home Menu.";
            ChangeIcon.ToolTip = $"Change the icon of {App.TitleName}.";
            SuspendResume.ToolTip = $"Not available while {App.TitleName} is not running.";
            // TODO: One TBD.
            MoreInfo.ToolTip = $"See more info about {App.TitleName}.";
            Delete.ToolTip = $"Delete {App.TitleName}.";
        }

        private void UpdateApp()
        {
            while (true)
            {
                var currentTarget = TargetManager.SelectedTarget;

                if (currentTarget.Info.Status != TargetStatusType.APIAvailable)
                {
                    Thread.Sleep(2000);
                    continue;
                }

                // Get Current App status.
                var newAppState = currentTarget.Application.GetAppState(_App.TitleId);
                if (newAppState >= 0)
                    AppState = newAppState;

                // App status.
                if (AppState == AppState.STATE_RUNNING || AppState == AppState.STATE_SUSPENDED)
                {
                    Dispatcher.Invoke(() => 
                    {
                        StartStop.ToolTip = $"Stop {_App.TitleName}.";
                        StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Stop.png";

                        SuspendResume.IsEnabled = true;
                        if (AppState == AppState.STATE_SUSPENDED)
                        {
                            SuspendResume.ToolTip = $"Resume {_App.TitleName}.";
                            SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/Start.png";
                        }
                        else
                        {
                            SuspendResume.ToolTip = $"Suspend {_App.TitleName}.";
                            SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/Suspend.png";
                        }
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        StartStop.ToolTip = $"Start {_App.TitleName}.";
                        StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Start.png";

                        SuspendResume.ToolTip = $"Not available while {_App.TitleName} is not running.";
                        SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/UnAvailable.png";
                        SuspendResume.IsEnabled = false;
                    });
                }

                // App Visibility. TODO: fix db corruption.
                //Visible = currentTarget.Application.GetVisibility(_App.TitleId);
                if (Visible == VisibilityType.VT_NONE || Visible == VisibilityType.VT_INVISIBLE)
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Show {_App.TitleName} from Home Menu.");
                }
                else
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Hide {_App.TitleName} from Home Menu.");
                }

                Thread.Sleep(1000);
            }
        }

        #region Buttons

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => 
            {
                var currentTarget = TargetManager.SelectedTarget;
                if (AppState == AppState.STATE_RUNNING || AppState == AppState.STATE_SUSPENDED)
                {
                    currentTarget.Application.Stop(_App.TitleId);
                }
                else
                {
                    currentTarget.Application.Start(_App.TitleId);
                }
            });
        }

        private void SuspendResume_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var currentTarget = TargetManager.SelectedTarget;
                if (AppState == AppState.STATE_SUSPENDED)
                {
                    currentTarget.Application.Resume(_App.TitleId);
                }
                else
                {
                    currentTarget.Application.Suspend(_App.TitleId);
                }
            });
        }

        private void Visibility_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var currentTarget = TargetManager.SelectedTarget;
                if (Visible == VisibilityType.VT_NONE || Visible == VisibilityType.VT_INVISIBLE)
                {
                    if(currentTarget.Application.SetVisibility(_App.TitleId, VisibilityType.VT_VISIBLE))
                    {
                        Visible = VisibilityType.VT_VISIBLE;
                    }
                }
                else
                {
                    if(currentTarget.Application.SetVisibility(_App.TitleId, VisibilityType.VT_INVISIBLE))
                    {
                        Visible = VisibilityType.VT_INVISIBLE;
                    }
                }
            });
        }

        private void ChangeIcon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetAsFocus_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MoreInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add are you sure?
            Task.Run(() => TargetManager.SelectedTarget.Application.Delete(_App.TitleId));
        }

        #endregion
    }
}
