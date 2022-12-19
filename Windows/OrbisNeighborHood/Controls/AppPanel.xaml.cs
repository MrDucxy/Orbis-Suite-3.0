using OrbisSuite;
using OrbisSuite.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for AppPanel.xaml
    /// </summary>
    public partial class AppPanel : UserControl
    {
        public AppInfo App;

        private AppState AppState = AppState.STATE_NOT_RUNNING;

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
            // 3 more TBD here.
            Delete.ToolTip = $"Delete {App.TitleName}.";
        }

        private void UpdateApp()
        {
            while (true)
            {
                var currentTarget = OrbisLib.Instance.SelectedTarget;

                // Get Current App status.
                var newAppState = currentTarget.Application.GetAppState(App.TitleId);
                if (newAppState >= 0)
                    AppState = newAppState;

                // App status.
                if (AppState == AppState.STATE_RUNNING || AppState == AppState.STATE_SUSPENDED)
                {
                    Dispatcher.Invoke(() => StartStop.ToolTip = $"Stop {App.TitleName}.");
                    Dispatcher.Invoke(() => StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Stop.png");
                }
                else
                {
                    Dispatcher.Invoke(() => StartStop.ToolTip = $"Start {App.TitleName}.");
                    Dispatcher.Invoke(() => StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Start.png");
                }

                // App Visibility.
                if (App.Visible == 0 || App.Visible == 2)
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
                var currentTarget = OrbisLib.Instance.SelectedTarget;
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

        private void Visibility_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeIcon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
