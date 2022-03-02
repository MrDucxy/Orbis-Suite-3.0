using OrbisSuite.Common;

namespace OrbisSuiteService.Service
{
    public class DBWatcher
    {
        public delegate void DBChangedHandler();
        public event DBChangedHandler DBChanged;

        public DBWatcher()
        {
            var watcher = new FileSystemWatcher(Config.OrbisPath);

            watcher.NotifyFilter = NotifyFilters.Attributes
                         | NotifyFilters.CreationTime
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.FileName
                         | NotifyFilters.LastAccess
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Security
                         | NotifyFilters.Size;

            watcher.Changed += OnChanged;

            watcher.Filter = Config.DataBaseName;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (DBChanged != null)
            {
                DBChanged();
            }
        }
    }
}
