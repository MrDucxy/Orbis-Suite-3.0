using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// Information about the targets saved.
    /// </summary>
    [Table("Targets2")]
    internal class TargetInfo2
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }

        /// <summary>
        /// Weather or not this is our default target to be selected on start up.
        /// </summary>
        [NotNull]
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// The name given to the target.
        /// </summary>
        [NotNull, Unique]        
        public string? TargetName { get; set; } = "-";

        /// <summary>
        /// The IP Address as a string.
        /// </summary>
        [NotNull, Unique]
        public string? IPAddress { get; set; } = "-";

        /// <summary>
        /// The firmware version as an int32.
        /// </summary>
        [NotNull]
        public int Firmware { get; set; }

        /// <summary>
        /// The port used to send payloads to the saved IP Address.
        /// </summary>
        [NotNull]
        public int PayloadPort { get; set; } = 9020;

        /// <summary>
        /// Will be true if a tcp connection can be achieved on the saved IP Address.
        /// </summary>
        [NotNull]
        public bool Available { get; set; } = false;

        /// <summary>
        /// Will be true if the Orbis Suite API is running on the target.
        /// </summary>
        [NotNull]
        public bool APIAvailable { get; set; } = false;

        /// <summary>
        /// The power/API status of the target.
        /// </summary>
        public TargetStatusType Status
        {
            get 
            {
                if (APIAvailable)
                    return TargetStatusType.APIAvailable;
                else if (Available)
                    return TargetStatusType.Online;
                else
                    return TargetStatusType.Offline;
            }
        }

        /// <summary>
        /// The SDK Version of the software installed on the target.
        /// </summary>
        [NotNull]
        public string? SDKVersion { get; set; } = "-";

        /// <summary>
        /// The full software version installed on the target.
        /// </summary>
        [NotNull]
        public string? SoftwareVersion { get; set; } = "-";

        /// <summary>
        /// The software version first installed on the target when sold.
        /// </summary>
        [NotNull]
        public string? FactorySoftwareVersion { get; set; } = "-";

        /// <summary>
        /// The current big game titleId running on the target.
        /// </summary>
        [NotNull]
        public string? CurrentTitleID { get; set; } = "-";

        /// <summary>
        /// The name of the target as set on the target.
        /// </summary>
        [NotNull]
        public string? ConsoleName { get; set; } = "-";

        /// <summary>
        /// The serial number of the targets motherboard.
        /// </summary>
        [NotNull]
        public string? MotherboardSerial { get; set; } = "-";

        /// <summary>
        /// The seraial number of the target.
        /// </summary>
        [NotNull]
        public string? Serial { get; set; } = "-";

        /// <summary>
        /// The model number of the target.
        /// </summary>
        [NotNull]
        public string? Model { get; set; } = "-";

        public ConsoleModelType ModelType 
        { 
            get
            {
                if (Model == null || !Regex.Match(Model, @"CUH-\d{1}\w{4}").Success)
                    return ConsoleModelType.Fat;

                switch (char.IsDigit(Model[4]) ? int.Parse(Model[4].ToString()) : 0)
                {
                    case 1:
                        return ConsoleModelType.Fat;

                    case 2:
                        return ConsoleModelType.Slim;

                    case 7:
                        return ConsoleModelType.Pro;


                    default:
                        return ConsoleModelType.Fat;
                }
            }
        }

        /// <summary>
        /// The MAC address of the target LAN adapter.
        /// </summary>
        [NotNull]
        public string? MACAddressLAN { get; set; } = "-";

        /// <summary>
        /// The MAC address of the target WIFI adapter.
        /// </summary>
        [NotNull]
        public string? MACAddressWIFI { get; set; } = "-";

        /// <summary>
        /// Will be true if the UART flag is set in the targets flash.
        /// </summary>
        [NotNull]
        public bool UART { get; set; } = false;

        /// <summary>
        /// Will return true if the IDUMode flag is set int he targets flash.
        /// </summary>
        [NotNull]
        public bool IDUMode { get; set; } = false;

        /// <summary>
        /// A unique string used to identify the target.
        /// </summary>
        [NotNull]
        public string? IDPS { get; set; } = "-";

        /// <summary>
        /// A unique string used to identify the target.
        /// </summary>
        [NotNull]
        public string? PSID { get; set; } = "-";

        /// <summary>
        /// The console type like Retail/TestKit/Devkit.
        /// </summary>
        [NotNull]
        public int ConsoleType { get; set; } = 0;

        /// <summary>
        /// Will be true if the Orbis Suite Debugger is attached to a process.
        /// </summary>
        [NotNull]
        public bool Attached { get; set; } = false;

        /// <summary>
        /// The current processId being debugged by the OrbisSuite Debugger.
        /// </summary>
        [NotNull]
        public int CurrentProcessId { get; set; } = 0;

        /// <summary>
        /// The used space on the targets hard disk.
        /// </summary>
        [NotNull]
        public int HDDUsedSpace { get; set; } = 0;

        /// <summary>
        /// The free space on the targets hard disk.
        /// </summary>
        [NotNull]
        public int HDDFreeSpace { get; set; } = 0;

        /// <summary>
        /// The usable size of the targets hard disk.
        /// </summary>
        [NotNull]
        public int HDDTotalSpace { get; set; } = 0;

        /// <summary>
        /// Saves the current information about the target to the database.
        /// </summary>
        /// <returns>Returns true if any rows were effected.</returns>
        public bool Save()
        {
            var db = new SQLiteConnection(Config.DataBasePath);
            var result = db.Update(this);
            db.Close();
            return (result > 0);
        }

        /// <summary>
        /// Adds a this Target to the data base.
        /// </summary>
        /// <returns>Returns true if a row was added to the database.</returns>
        public bool Add()
        {
            var db = new SQLiteConnection(Config.DataBasePath);
            var result = db.InsertOrReplace(this);
            db.Close();
            return (result > 0);
        }

        /// <summary>
        /// Removes this current Target from the database.
        /// </summary>
        /// <returns>Returns true if a row was removed from the database.</returns>
        public bool Remove()
        {
            var db = new SQLiteConnection(Config.DataBasePath);
            var result = db.Delete(this);
            db.Close();
            return (result > 0);
        }

        public TargetInfo2 FindDefaultTarget()
        {

        }
    }
}
