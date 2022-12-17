using OrbisNeighborHood.Controls;
using OrbisSuite;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
using OrbisSuite.Common.Database.App;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for AppListView.xaml
    /// </summary>
    public partial class AppListView : UserControl
    {
        private static string _AppDatabaseRemotePath = @"/system_data/priv/mms/app.db";

        private static Mutex appDataBaseMtx = new Mutex();

        private static readonly List<string> TitleIdExlusionList = new List<string>()
        {
            "NPXS20108",
            "NPXS20110",
            "NPXS20111",
            "NPXS20118",
            "NPXS20104",
            "NPXS20117",
            "NPXS20107",
            "CUSA00960",
            "CUSA01697",
            "CUSA01780",
            "NPXS20105",
            "NPXS20106",
            "NPXS20114",
            "NPXS20120",

            // Destiny? lol
            "CUSA00219",
            "CUSA00568",
            "CUSA01000"
        };

        public AppListView()
        {
            InitializeComponent();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;

            // Refresh the info about the current target.
            RefreshTargetInfo();

            // Create task to periodically check for app.db changes.
            Task.Run(() => CheckAppDatabase());
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
                // Update title with the new target name.
                TitleString = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;

                // Re initialize the application list for the new target.
                // InitAppList();
            }
        }

        #endregion

        public void AddApp(string UserDirectoryPath, string AppVersion, AppBrowse App)
        {
            var currentTarget = OrbisLib.Instance.SelectedTarget;

            // Make sure the titleId format is correct. Helps weed out bad entries and folders.
            if (!Regex.IsMatch(App.titleId, @"[a-zA-Z]{4}\d{5}"))
                return;

            // Skip the Destiny entries that just exist for some reason even after a restore?... lol
            if ((App.titleId.Equals("CUSA00219") || App.titleId.Equals("CUSA00568") || App.titleId.Equals("CUSA01000")) && App.contentSize <= 0)
                return;

            // Skip some that aren't technically an app.
            if (TitleIdExlusionList.Contains(App.titleId))
                return;

            // Weed out some more bad entries created by default.
            if (App.titleName.Length <= 2)
                return;

            // Make sure only add ones with a category.
            if (App.uiCategory.Length <= 0 || App.category.Length <= 0)
                return;

            // Directory to cache stuff for app.
            string currentAppPath = Path.Combine(UserDirectoryPath, App.titleId);

            // Create Directory for current app.
            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }

            // Cache icon0.png for app if we have not already.
            if (!File.Exists(Path.Combine(currentAppPath, "icon0.png")) && !string.IsNullOrEmpty(App.metaDataPath) && currentTarget.Info.Details.IsAvailable) //TODO: Maybe add a isFTPAvailable.
            {
                currentTarget.FTP.DownloadFile($"{App.metaDataPath}/icon0.png", Path.Combine(currentAppPath, "icon0.png"));
            }

            // Add or update app list item.
            Dispatcher.Invoke(() =>
            {
                var panel = AppList.Items.Cast<AppPanel>().ToList().Find(x => x.App.titleId == App.titleId);
                if (panel != null)
                {
                    panel.Update(App, AppVersion);
                }
                else
                {
                    AppList.Items.Add(new AppPanel(App, AppVersion));
                }
            });
        }

        private async Task InitAppList()
        {
            // Clear lists so we can re-populate them.
            Dispatcher.Invoke(() => AppList.Items.Clear());

            // Make sure we have a target we can pull the db from.
            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            // Make sure the Target is online so we can pull the db.
            var currentTarget = OrbisLib.Instance.SelectedTarget;
            if (currentTarget == null)
            {
                Console.WriteLine("No current target we can use to load apps.");
                return;
            }

            // Make sure we have the appCache folder.
            string appCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Orbis Suite\AppCache\");
            if (!Directory.Exists(appCachePath))
            {
                Directory.CreateDirectory(appCachePath);
            }

            // Path where we will store this data.
            var ForegroundAccountId = currentTarget.Info.Details.ForegroundAccountId;
            string userDirectoryPath = Path.Combine(appCachePath, ForegroundAccountId.ToString("X"));

            // Create Directory for database.
            if (!Directory.Exists(userDirectoryPath))
            {
                Directory.CreateDirectory(userDirectoryPath);
            }

            // Check to see if we have a database.
            string appDataBasePath = Path.Combine(userDirectoryPath, "app.db");
            if (!File.Exists(appDataBasePath))
            {
                Console.WriteLine("No app.db is cached.");
                return;
            }

            appDataBaseMtx.WaitOne();

            // Rename the table we need in the app.db.
            AppBrowse.AppBrowseRemoveUserId(appDataBasePath, ForegroundAccountId);

            // Get all apps from the app.db
            var appList = AppBrowse.GetAppBrowseList(appDataBasePath);

            foreach (var app in appList)
            {
                Parallel.Invoke(() =>
                {
                    AddApp(userDirectoryPath, AppInfo.GetStringFromAppInfo(appDataBasePath, app.titleId, "APP_VER"), app);
                });
            }

            appDataBaseMtx.ReleaseMutex();
        }

        private async Task CheckAppDatabase()
        {
            // Update the current App list. Called here because it must be initialized before the update is called.
            await InitAppList();

            while (true)
            {
                try
                {
                    // Make sure we have a target we can pull the db from.
                    if (OrbisLib.Instance.TargetManagement.TargetList == null)
                        continue;

                    // Make sure the Target is online so we can pull the db.
                    var currentTarget = OrbisLib.Instance.SelectedTarget;
                    if (currentTarget == null || !currentTarget.Info.Details.IsAvailable)
                    {
                        continue;
                    }

                    // Path where we will store this data.
                    var ForegroundAccountId = currentTarget.Info.Details.ForegroundAccountId;
                    string userDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Orbis Suite\AppCache\", ForegroundAccountId.ToString("X"));

                    // Download database.
                    string appDataBasePath = Path.Combine(userDirectoryPath, "app.db");
                    if (File.Exists(appDataBasePath))
                    {
                        // Download the new app.db.
                        string newAppDataBasePath = Path.Combine(userDirectoryPath, "new_app.db");
                        if (currentTarget.FTP.DownloadFile(_AppDatabaseRemotePath, newAppDataBasePath))
                        {
                            // Get app.db versions.
                            var newVersion = AppBrowseVersion.GetAppBrowseVersion(newAppDataBasePath);
                            var oldVersion = AppBrowseVersion.GetAppBrowseVersion(appDataBasePath);

                            // Compare the app.db versions.
                            if (newVersion > oldVersion)
                            {
                                appDataBaseMtx.WaitOne();

                                // Remove the out dated database.
                                File.Delete(appDataBasePath);

                                // Rename the new database.
                                File.Move(newAppDataBasePath, appDataBasePath);

                                // Rename the table we need in the app.db.
                                AppBrowse.AppBrowseRemoveUserId(appDataBasePath, ForegroundAccountId);

                                // Get all apps from the app.db
                                var appList = AppBrowse.GetAppBrowseList(appDataBasePath);

                                // Check for deletions.
                                foreach (var app in AppList.Items.Cast<AppPanel>().ToList())
                                {
                                    if (appList.Find(x => x.titleId == app.App.titleId) == null)
                                    {
                                        Dispatcher.Invoke(() => AppList.Items.Remove(app));
                                    }
                                }

                                // Check for new apps / updates.
                                foreach (var app in appList)
                                {
                                    Parallel.Invoke(() =>
                                    {
                                        AddApp(userDirectoryPath, AppInfo.GetStringFromAppInfo(appDataBasePath, app.titleId, "APP_VER"), app);
                                    });
                                }

                                appDataBaseMtx.ReleaseMutex();
                            }
                            else
                            {
                                // Remove new database as ours is current.
                                File.Delete(newAppDataBasePath);
                            }
                        }
                    }
                    else if (currentTarget.Info.Details.IsAvailable)
                    {
                        // Download the app database for the first time.
                        if (OrbisLib.Instance.SelectedTarget.FTP.DownloadFile(_AppDatabaseRemotePath, appDataBasePath))
                        {
                            // Initialize the Application List now that we have an app.db.
                            InitAppList();
                        }
                    }
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
