using OrbisNeighborHood.Controls;
using OrbisSuite;
using OrbisSuite.Common.Database;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for AppListView.xaml
    /// </summary>
    public partial class AppListView : UserControl
    {
        public AppListView()
        {
            InitializeComponent();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;

            RefreshTargetInfo();
            Task.Run(() => RefreshAppList());
        }

        #region Properties

        public string TitleString
        {
            get { return (string)GetValue(TitleStringProperty); }
            set
            {
                SetValue(TitleStringProperty, $"Applications ({value})");
            }
        }

        public static readonly DependencyProperty TitleStringProperty =
            DependencyProperty.Register("TitleString", typeof(string), typeof(AppListView), new PropertyMetadata(string.Empty));

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
            }
        }

        #endregion

        public void AddApp(string DirectoryPath, AppBrowse App, AppPanel panel)
        {
            // Make sure the titleId format is correct. Helps weed out bad entries and folders.
            if (!Regex.IsMatch(App.titleId, @"[a-zA-Z]{4}\d{5}"))
                return;

            // Weed out some more bad entries created by default.
            if (App.titleName.Length <= 2)
                return;

            // Make sure only add ones with a category.
            if (App.uiCategory.Length <= 0 || App.category.Length <= 0)
                return;

            // Directory to cache stuff for app.
            string currentAppPath = Path.Combine(DirectoryPath, App.titleId);

            // Create Directory for current app.
            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }

            // Cache param.sfo & icon0.png for all apps.

            Dispatcher.Invoke(() => AppList.Items.Add(panel));
        }

        public void RefreshAppList()
        {
            Dispatcher.Invoke(() => AppList.Items.Clear());

            // Make sure we have a target we can pull the db from.
            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            // Make sure the Target is online so we can pull the db.
            var currentTarget = OrbisLib.Instance.SelectedTarget.Info;
            if (currentTarget == null || !currentTarget.Details.IsAvailable)
            {
                return;
            }

            // Make sure we have the appCache folder.
            string appCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Orbis Suite\AppCache\");
            if (!Directory.Exists(appCachePath))
            {
                Directory.CreateDirectory(appCachePath);
            }

            // Path where we will store this data.
            string directoryPath = Path.Combine(appCachePath, currentTarget.Details.ForegroundAccountId.ToString("X"));

            // Create Directory for data base.
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Download data base.
            // TODO: Get the tbl_version and check update_index or sync_server number this will tell us if new apps are installed.

            // Rename the table we need in the app.db.
            AppBrowse.AppBrowseRemoveUserId(Path.Combine(directoryPath, "app.db"), currentTarget.Details.ForegroundAccountId);

            // Get all apps from the app.db
            var appList = AppBrowse.GetAppBrowseList(Path.Combine(directoryPath, "app.db"));

            foreach(var app in appList)
            {
                Parallel.Invoke(() =>
                {
                     AddApp(directoryPath, app, Dispatcher.Invoke(() => new AppPanel(app)));
                });
            }
        }
    }
}
