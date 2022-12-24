using OrbisLib2.Common.API;
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
        public AppInfo App;

        private AppState AppState = AppState.STATE_NOT_RUNNING;
        private VisibilityType Visible = VisibilityType.VT_NONE;

        public AppPanel(AppInfo App, string AppVersion)
        {
            InitializeComponent();

            this.App = App;

            // Update Application information.
            Update(App, AppVersion);

            // Start background task to periodically check if this application is currently running.
            Task.Run(() => UpdateApp());
        }

        public void Update(AppInfo App, string AppVersion)
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

            InstallDateElement.FieldText = App.InstallDate.ToString("ddd dd MMM yyy hh:mm tt");

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

                // Get Current App status.
                var newAppState = currentTarget.Application.GetAppState(App.TitleId);
                if (newAppState >= 0)
                    AppState = newAppState;

                // App status.
                if (AppState == AppState.STATE_RUNNING || AppState == AppState.STATE_SUSPENDED)
                {
                    Dispatcher.Invoke(() => 
                    {
                        StartStop.ToolTip = $"Stop {App.TitleName}.";
                        StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Stop.png";

                        SuspendResume.IsEnabled = true;
                        if (AppState == AppState.STATE_SUSPENDED)
                        {
                            SuspendResume.ToolTip = $"Resume {App.TitleName}.";
                            SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/Start.png";
                        }
                        else
                        {
                            SuspendResume.ToolTip = $"Suspend {App.TitleName}.";
                            SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/Suspend.png";
                        }
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        StartStop.ToolTip = $"Start {App.TitleName}.";
                        StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Start.png";

                        SuspendResume.ToolTip = $"Not available while {App.TitleName} is not running.";
                        SuspendResume.ImageSource = "/OrbisNeighborHood;component/Images/UnAvailable.png";
                        SuspendResume.IsEnabled = false;
                    });
                }

                // App Visibility.
                Visible = currentTarget.Application.GetVisibility(App.TitleId);
                if (Visible == VisibilityType.VT_NONE || Visible == VisibilityType.VT_INVISIBLE)
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Show {App.TitleName} from Home Menu.");
                }
                else
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Hide {App.TitleName} from Home Menu.");
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
                    currentTarget.Application.Stop(App.TitleId);
                }
                else
                {
                    currentTarget.Application.Start(App.TitleId);
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
                    currentTarget.Application.Resume(App.TitleId);
                }
                else
                {
                    currentTarget.Application.Suspend(App.TitleId);
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
                    if(currentTarget.Application.SetVisibility(App.TitleId, VisibilityType.VT_VISIBLE))
                    {
                        Visible = VisibilityType.VT_VISIBLE;
                    }
                }
                else
                {
                    if(currentTarget.Application.SetVisibility(App.TitleId, VisibilityType.VT_INVISIBLE))
                    {
                        Visible = VisibilityType.VT_INVISIBLE;
                    }
                }
            });
        }

        private void ChangeIcon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Title = new TMDB(App.TitleId);
                var url = $"https://store.playstation.com/product/{Title.ContentID}/";

                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {

            }
        }

        private void MoreInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add are you sure?
            Task.Run(() => TargetManager.SelectedTarget.Application.Delete(App.TitleId));
        }

        #endregion
    }
}
