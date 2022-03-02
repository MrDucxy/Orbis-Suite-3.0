namespace OrbisSuite.Common.DataBase
{
    public static class Settings
    {
        #region General

        /// <summary>
        /// The API port that OrbisLib communicates on.
        /// </summary>
        public static int APIPort
        {
            get
            {
                return Database.GetSetting<int>("APIPort");
            }

            set
            {
                Database.SetSetting("APIPort", value);
            }
        }

        /// <summary>
        /// The port that will be used to access the targets file system using ftp
        /// </summary>
        public static int FTPPort
        {
            get
            {
                return Database.GetSetting<int>("FTPPort");
            }

            set
            {
                Database.SetSetting("FTPPort", value);
            }
        }

        /// <summary>
        /// The port of a klog server that will be used to print console output similar to UART.
        /// </summary>
        public static int KlogPort
        {
            get
            {
                return Database.GetSetting<int>("KlogPort");
            }

            set
            {
                Database.SetSetting("KlogPort", value);
            }
        }

        /// <summary>
        /// The serial COM port we will listen to for UART output.
        /// </summary>
        public static string COMPort
        {
            get
            {
                return Database.GetSetting<string>("COMPort");
            }

            set
            {
                Database.SetSetting("COMPort", value);
            }
        }

        /// <summary>
        /// Starts the Orbis Suite taskbar app when windows boots.
        /// </summary>
        public static bool StartOnBoot
        {
            get
            {
                return Database.GetSetting<bool>("StartOnBoot");
            }

            set
            {
                Database.SetSetting("StartOnBoot", value);

                //Get windows registry key to set app on start up.
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                //add or remove the value.
                if (value)
                    key.SetValue("OrbisTaskbarApp", AppDomain.CurrentDomain.BaseDirectory + "OrbisTaskbarApp.exe");
                else
                    key.DeleteValue("OrbisTaskbarApp", false);

                //Close the key after use.
                key?.Close();
            }
        }

        #endregion

        #region Themes

        /// <summary>
        /// Choose which theme will be used across Orbis Suite.
        /// </summary>
        public static int Theme
        {
            get
            {
                return Database.GetSetting<int>("Theme");
            }

            set
            {
                Database.SetSetting("Theme", value);
            }
        }

        /// <summary>
        /// Enables the accent colours to cycle through all colours of the rainbow.
        /// </summary>
        public static bool RainbowColours
        {
            get
            {
                return Database.GetSetting<bool>("RainbowColours");
            }

            set
            {
                Database.SetSetting("RainbowColours", value);
            }
        }

        #endregion

        #region TargetInfo

        /// <summary>
        /// When viewd from the target details choose to censor the Target identifier.
        /// </summary>
        public static bool CensorIDPS
        {
            get
            {
                return Database.GetSetting<bool>("CensorIDPS");
            }

            set
            {
                Database.SetSetting("CensorIDPS", value);
            }
        }

        /// <summary>
        /// When viewd from the target details choose to censor the Target identifier.
        /// </summary>
        public static bool CensorPSID
        {
            get
            {
                return Database.GetSetting<bool>("CensorPSID");
            }

            set
            {
                Database.SetSetting("CensorPSID", value);
            }
        }

        #endregion

        #region ConsoleOutput

        /// <summary>
        /// SHow timestamps on the console output.
        /// </summary>
        public static bool ShowTimestamps
        {
            get
            {
                return Database.GetSetting<int>("ShowTimestamps");
            }

            set
            {
                Database.SetSetting("ShowTimestamps", value);
            }
        }

        /// <summary>
        /// Word wrap the console output window.
        /// </summary>
        public static bool WordWrap
        {
            get
            {
                return Database.GetSetting<int>("WordWrap");
            }

            set
            {
                Database.SetSetting("WordWrap", value);
            }
        }

        #endregion
    }
}
