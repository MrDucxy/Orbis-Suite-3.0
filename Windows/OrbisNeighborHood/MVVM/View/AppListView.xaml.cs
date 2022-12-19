using OrbisNeighborHood.Controls;
using OrbisSuite;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
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
            "NPXS20109",
            "NPXS20112",
            "NPXS20133",
            "NPXS20132",

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

            Task.Run(() => 
            {
                // Initially populate the app list.
                InitAppList();

                // Create task to periodically check for app.db changes.
                // Task.Run(() => CheckAppDatabase());
            });
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

        public void AddApp(string appCachePath, AppInfo App)
        {
            var currentTarget = OrbisLib.Instance.SelectedTarget;

            // Make sure the titleId format is correct. Helps weed out bad entries and folders.
            if (!Regex.IsMatch(App.TitleId, @"[a-zA-Z]{4}\d{5}"))
                return;

            // Skip the Destiny entries that just exist for some reason even after a restore?... lol
            if ((App.TitleId.Equals("CUSA00219") || App.TitleId.Equals("CUSA00568") || App.TitleId.Equals("CUSA01000")) && App.ContentSize <= 0)
                return;

            // Skip some that aren't technically an app.
            if (TitleIdExlusionList.Contains(App.TitleId))
                return;

            // Weed out some more bad entries created by default.
            if (App.TitleName.Length <= 2)
                return;

            // Make sure only add ones with a category.
            if (App.UICategory.Length <= 0 || App.Category.Length <= 0)
                return;

            // Directory to cache stuff for app.
            string currentAppPath = Path.Combine(appCachePath, App.TitleId);

            // Create Directory for current app.
            if (!Directory.Exists(currentAppPath))
            {
                Directory.CreateDirectory(currentAppPath);
            }

            // Cache icon0.png for app if we have not already.
            if (!File.Exists(Path.Combine(currentAppPath, "icon0.png")) && !string.IsNullOrEmpty(App.MetaDataPath) && currentTarget.Info.Details.IsAvailable) //TODO: Maybe add a isFTPAvailable.
            {
                currentTarget.FTP.DownloadFile($"{App.MetaDataPath}/icon0.png", Path.Combine(currentAppPath, "icon0.png"));
            }

            // Fetch the App version.
            var appVersion = currentTarget.Application.GetAppInfoString(App.TitleId, "APP_VER");

            // Add or update app list item.
            Dispatcher.Invoke(() =>
            {
                var panel = AppList.Items.Cast<AppPanel>().ToList().Find(x => x.App.TitleId == App.TitleId);
                if (panel != null)
                {
                    panel.Update(App, appVersion);
                }
                else
                {
                    AppList.Items.Add(new AppPanel(App, appVersion));
                }
            });
        }

        private void InitAppList()
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

            var appList = currentTarget.Application.GetAppList();

            foreach (var app in appList)
            {
                Parallel.Invoke(() =>
                {
                    AddApp(appCachePath, app);
                });
            }
        }

        private async Task CheckAppDatabase()
        {
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

                    // Appcache location.
                    string appCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Orbis Suite\AppCache\");

                    // Get the current app list.
                    var appList = currentTarget.Application.GetAppList();

                    // Check for deletions.
                    foreach (var app in AppList.Items.Cast<AppPanel>().ToList())
                    {
                        if (appList.Find(x => x.TitleId == app.App.TitleId) == null)
                        {
                            Dispatcher.Invoke(() => AppList.Items.Remove(app));
                        }
                    }

                    // Check for new apps / updates.
                    foreach (var app in appList)
                    {
                        Parallel.Invoke(() =>
                        {
                            AddApp(appCachePath, app);
                        });
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
