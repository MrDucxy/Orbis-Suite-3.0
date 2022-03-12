using SQLite;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// Used to get/set the settings of Orbis Suite.
    /// </summary>
    [Table("Settings")]
    public class Settings
    {
        private int _APIPort;
        private int _FTPPort;
        private int _KlogPort;
        private string? _COMPort;
        private bool _StartOnBoot;
        private int _Theme;
        private bool _RainbowColours;
        private bool _CensorIDPS;
        private bool _CensorPSID;
        private bool _ShowTimestamps;
        private bool _WordWrap;

        /// <summary>
        /// The API port that OrbisLib communicates on.
        /// </summary>
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
        public bool CensorIDPS
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
