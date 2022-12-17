using OrbisSuite;
using OrbisSuite.Common;
using OrbisSuite.Common.Database.App;
using SimpleUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using static System.Net.Mime.MediaTypeNames;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for AppPanel.xaml
    /// </summary>
    public partial class AppPanel : UserControl
    {
        public AppBrowse App;

        private AppState AppState = AppState.STATE_NOT_RUNNING;

        public AppPanel(AppBrowse App, string AppVersion)
        {
            InitializeComponent();

            this.App = App;

            // Update Application information.
            Update(App, AppVersion);

            // Start background task to periodically check if this application is currently running.
            Task.Run(() => UpdateApp());
        }

        public void Update(AppBrowse App, string AppVersion)
        {
            // Set the Info about this application.
            ApplicationNameElement.Text = App.titleName;
            TitleIdElement.FieldText = App.titleId;
            VersionElement.FieldText = AppVersion.TrimStart('0');
            TypeElement.FieldText = $"{App.uiCategory} ({App.category})";
            SizeElement.FieldText = App.contentSize <= 0 ? "N/A" : Utilities.BytesToString(App.contentSize);

            // Get the path to our icon and make sure it exists.
            string iconPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Orbis Suite\AppCache\{OrbisLib.Instance.SelectedTarget.Info.Details.ForegroundAccountId.ToString("X")}\{App.titleId}\icon0.png";
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

            if (DateTime.TryParse(App.installDate, out var dateTime))
            {
                InstallDateElement.FieldText = dateTime.ToString("ddd dd MMM yyy hh:mm tt");
            }

            // Set the tool tips.
            StartStop.ToolTip = $"Start {App.titleName}.";
            Visibility.ToolTip = $"Hide {App.titleName} from Home Menu.";
            ChangeIcon.ToolTip = $"Change the icon of {App.titleName}.";
            // 3 more TBD here.
            Delete.ToolTip = $"Delete {App.titleName}.";
        }

        private void UpdateApp()
        {
            while (true)
            {
                var currentTarget = OrbisLib.Instance.SelectedTarget;

                // Get Current App status.
                var newAppState = currentTarget.Application.GetAppState(App.titleId);
                if (newAppState >= 0)
                    AppState = newAppState;

                // App status.
                if (AppState == AppState.STATE_RUNNING || AppState == AppState.STATE_SUSPENDED)
                {
                    Dispatcher.Invoke(() => StartStop.ToolTip = $"Stop {App.titleName}.");
                    Dispatcher.Invoke(() => StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Stop.png");
                }
                else
                {
                    Dispatcher.Invoke(() => StartStop.ToolTip = $"Start {App.titleName}.");
                    Dispatcher.Invoke(() => StartStop.ImageSource = "/OrbisNeighborHood;component/Images/Start.png");
                }

                // App Visibility.
                if (App.visible == 0 || App.visible == 2)
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Show {App.titleName} from Home Menu.");
                }
                else
                {
                    Dispatcher.Invoke(() => Visibility.ToolTip = $"Hide {App.titleName} from Home Menu.");
                }

                Thread.Sleep(1000);
            }
        }

        #region Buttons

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"{App.titleId} AppStatus: {AppState}");
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
