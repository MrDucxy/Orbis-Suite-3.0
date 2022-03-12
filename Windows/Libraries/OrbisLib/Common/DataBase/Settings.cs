using SQLite;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// Used to get/set the settings of Orbis Suite.
    /// </summary>
    [Table("Settings")]
    public class Settings
    {
        /// <summary>
        /// The API port that OrbisLib communicates on.
        /// </summary>
        private int _APIPort;

        public int APIPort
        {
            get { return _APIPort; }
            set 
            { 
                _APIPort = value;
                Save();
            }
        }

        /// <summary>
        /// The port that will be used to access the targets file system using ftp
        /// </summary>
        private int _FTPPort;

        public int FTPPort
        {
            get { return _FTPPort; }
            set 
            { 
                _FTPPort = value;
                Save();
            }
        }

        /// <summary>
        /// The port of a klog server that will be used to print console output similar to UART.
        /// </summary>
        private int _KlogPort;

        public int KlogPort
        {
            get { return _KlogPort; }
            set 
            { 
                _KlogPort = value;
                Save();
            }
        }

        /// <summary>
        /// The serial COM port we will listen to for UART output.
        /// </summary>
        private string? _COMPort;

        public string? COMPort
        {
            get { return _COMPort; }
            set 
            { 
                _COMPort = value;
                Save();
            }
        }

        /// <summary>
        /// Starts the Orbis Suite taskbar app when windows boots.
        /// </summary>
        private bool _StartOnBoot;

        public bool StartOnBoot
        {
            get { return _StartOnBoot; }
            set 
            { 
                _StartOnBoot = value;
                Save();
            }
        }

        /// <summary>
        /// Choose which theme will be used across Orbis Suite.
        /// </summary>
        private int _Theme;

        public int Theme
        {
            get { return _Theme; }
            set
            {
                _Theme = value;
                Save();
            }
        }

        /// <summary>
        /// Enables the accent colours to cycle through all colours of the rainbow.
        /// </summary>
        private bool _RainbowColours;

        public bool RainbowColours
        {
            get { return _RainbowColours; }
            set 
            { 
                _RainbowColours = value;
                Save();
            }
        }

        /// <summary>
        /// When viewd from the target details choose to censor the Target identifier.
        /// </summary>
        private bool _CensorIDPS;

        public bool MyProperty
        {
            get { return _CensorIDPS; }
            set 
            { 
                _CensorIDPS = value;
                Save();
            }
        }

        /// <summary>
        /// When viewd from the target details choose to censor the Target identifier.
        /// </summary>
        private bool _CensorPSID;

        public bool CensorPSID
        {
            get { return _CensorPSID; }
            set
            {
                _CensorPSID = value;
                Save();
            }
        }

        /// <summary>
        /// SHow timestamps on the console output.
        /// </summary>
        private bool _ShowTimestamps;

        public bool ShowTimestamps
        {
            get { return _ShowTimestamps; }
            set 
            {
                _ShowTimestamps = value;
                Save();
            }
        }

        /// <summary>
        /// Word wrap the console output window.
        /// </summary>
        private bool _WordWrap;

        public bool WordWrap
        {
            get { return _WordWrap; }
            set 
            { 
                _WordWrap = value;
                Save();
            }
        }

        public static Settings Instance
        {
            get
            {
                var db = new SQLiteConnection(Config.DataBasePath);
                var instance = db.Table<Settings>().First();
                db.Close();
                return instance;
            }
        }

        private static void Save()
        {
            var db = new SQLiteConnection(Config.DataBasePath);
            db.Insert(Instance);
            db.Close();
        }
    }
}
