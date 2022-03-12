using System.Data.SQLite;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace OrbisSuite.Common.Database
{
    public class TargetInfo
    {
        public bool Default { get; set; }

        public string Name { get; set; } = "-";

        public string IPAddr { get; set; } = "-";

        public int Firmware { get; set; }

        public int PayloadPort { get; set; }

        public bool Available { get; set; }

        public bool APIAvailable { get; set; }

        public string SDKVersion { get; set; } = "-";

        public string SoftwareVersion { get; set; } = "-";

        public string FactorySoftwareVersion { get; set; } = "-";

        public string CurrentTitleID { get; set; } = "-";

        public string ConsoleName { get; set; } = "-";

        public string MotherboardSerial { get; set; } = "-";

        public string Serial { get; set; } = "-";

        public string Model { get; set; } = "-";

        public string MACAddressLAN { get; set; } = "-";

        public string MACAddressWIFI { get; set; } = "-";

        public bool UART { get; set; }

        public bool IDUMode { get; set; }

        public string IDPS { get; set; } = "-";

        public string PSID { get; set; } = "-";

        public string ConsoleType { get; set; } = "-";

        public bool Attached { get; set; }

        public int CurrentProcessId { get; set; }

        public int HDDUsedSpace { get; set; }

        public int HDDFreeSpace { get; set; }

        public int HDDTotalSpace { get; set; }

        public TargetInfo()
        {

        }

        public TargetInfo(bool Default, string Name, string IPAddr, int Firmware, int PayloadPort)
        {
            this.Default = Default;
            this.Name = Name;
            this.IPAddr = IPAddr;
            this.Firmware = Firmware;
            this.PayloadPort = PayloadPort;
        }
    }

    public static class SavedTargets
    {
        /// <summary>
        /// Used to Identify the index of the column for specific data.
        /// </summary>
        private enum DataColumns : int
        {
            Id,
            DefaultTarget,
            TargetName,
            IPAddress,
            Firmware,
            PayloadPort,
            Available,
            APIAvailable,
            SDKVersion,
            SoftwareVersion,
            FactorySoftwareVersion,
            CurrentTitleID,
            ConsoleName,
            MotherboardSerial,
            Serial,
            Model,
            MACAddressLAN,
            MACAddressWIFI,
            UART,
            IDUMode,
            IDPS,
            PSID,
            ConsoleType,
            Attached,
            CurrentProcessId,
            HDDUsedSpace,
            HDDFreeSpace,
            HDDTotalSpace,
        };

        /// <summary>
        /// Returns a list of the Targets saved.
        /// </summary>
        public static List<TargetInfo> Targets
        {
            get
            {
                if (GetTargetList(out List<TargetInfo> Out))
                    return Out;
                else
                    return new List<TargetInfo>();
            }
        }

        /// <summary>
        /// Returns a count of all the Targets currently saved.
        /// </summary>
        public static int TargetCount
        {
            get
            {
                return GetTargetCount();
            }
        }

        /// <summary>
        /// Returns the target that is the current Default Target.
        /// </summary>
        public static TargetInfo DefaultTarget
        {
            get
            {
                if (GetDefaultTarget(out TargetInfo Out))
                    return Out;
                else
                    return new TargetInfo();
            }
        }

        /// <summary>
        /// Returns a bool if a Default Target is set or not.
        /// </summary>
        /// <returns></returns>
        public static bool DoesDefaultTargetExist()
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT COUNT(*)
                        FROM Targets
                        WHERE DefaultTarget=1
                    ";
                    return (Convert.ToInt32(command.ExecuteScalar()) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns a bool if Target is saved or not.
        /// </summary>
        /// <param name="TargetName">The string name of the target saved.</param>
        /// <returns></returns>
        public static bool DoesTargetExist(string TargetName)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT COUNT(*)
                        FROM Targets
                        WHERE TargetName=@Target
                    ";
                    command.Parameters.AddWithValue("@Target", TargetName);

                    return (Convert.ToInt32(command.ExecuteScalar()) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Checks if target with a specific IP Address is already saved.
        /// </summary>
        /// <param name="IPAddr">The IP Address as a string which we want to look for.</param>
        /// <returns></returns>
        public static bool DoesTargetExistIP(string IPAddr)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT COUNT(*)
                        FROM Targets
                        WHERE IPaddress=@IPAddr
                    ";
                    command.Parameters.AddWithValue("@IPAddr", IPAddr);

                    return (Convert.ToInt32(command.ExecuteScalar()) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns info on the target specified.
        /// </summary>
        /// <param name="TargetName">The string name of the Target to look up.</param>
        /// <param name="Out">The output of the Target's information.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool GetTarget(string TargetName, out TargetInfo Out)
        {
            Out = new TargetInfo();

            if (!DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} doesn't exist in Targets Table.");

                return false;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM Targets WHERE TargetName=@Target";
                    command.Parameters.AddWithValue("@Target", TargetName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Out.Default = reader.GetBoolean((int)DataColumns.DefaultTarget);
                            Out.Name = reader.GetString((int)DataColumns.TargetName);
                            Out.IPAddr = reader.GetString((int)DataColumns.IPAddress);
                            Out.Firmware = reader.GetInt32((int)DataColumns.Firmware);
                            Out.PayloadPort = reader.GetInt32((int)DataColumns.PayloadPort);
                            Out.Available = reader.GetBoolean((int)DataColumns.Available);
                            Out.APIAvailable = reader.GetBoolean((int)DataColumns.APIAvailable);
                            Out.SDKVersion = reader.GetString((int)DataColumns.SDKVersion);
                            Out.SoftwareVersion = reader.GetString((int)DataColumns.SoftwareVersion);
                            Out.FactorySoftwareVersion = reader.GetString((int)DataColumns.FactorySoftwareVersion);
                            Out.CurrentTitleID = reader.GetString((int)DataColumns.CurrentTitleID);
                            Out.ConsoleName = reader.GetString((int)DataColumns.ConsoleName);
                            Out.MotherboardSerial = reader.GetString((int)DataColumns.MotherboardSerial);
                            Out.Serial = reader.GetString((int)DataColumns.Serial);
                            Out.Model = reader.GetString((int)DataColumns.Model);
                            Out.MACAddressLAN = reader.GetString((int)DataColumns.MACAddressLAN);
                            Out.MACAddressWIFI = reader.GetString((int)DataColumns.MACAddressWIFI);
                            Out.UART = reader.GetBoolean((int)DataColumns.UART);
                            Out.IDUMode = reader.GetBoolean((int)DataColumns.IDUMode);
                            Out.IDPS = reader.GetString((int)DataColumns.IDPS);
                            Out.PSID = reader.GetString((int)DataColumns.PSID);
                            Out.ConsoleType = reader.GetString((int)DataColumns.ConsoleType);
                            Out.Attached = reader.GetBoolean((int)DataColumns.Attached);
                            Out.CurrentProcessId = reader.GetInt32((int)DataColumns.CurrentProcessId);
                            Out.HDDUsedSpace = reader.GetInt32((int)DataColumns.HDDUsedSpace);
                            Out.HDDFreeSpace = reader.GetInt32((int)DataColumns.HDDFreeSpace);
                            Out.HDDTotalSpace = reader.GetInt32((int)DataColumns.HDDTotalSpace);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Updates the saved information about the Target specified.
        /// </summary>
        /// <param name="TargetName">The string name of the Target to update.</param>
        /// <param name="In">The input data to change.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool SetTarget(string TargetName, TargetInfo In)
        {
            if (!DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} doesn't exist in Targets Table.");

                return false;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE Targets SET TargetName=@NewTargetName, IPAddress=@NewIPAddress, Firmware=@NewFirmware, PayloadPort=@NewPort WHERE TargetName=@Target";
                    command.Parameters.AddWithValue("@Target", TargetName);

                    command.Parameters.AddWithValue("@NewTargetName", In.Name);
                    command.Parameters.AddWithValue("@NewIPAddress", In.IPAddr);
                    command.Parameters.AddWithValue("@NewFirmware", In.Firmware);
                    command.Parameters.AddWithValue("@NewPort", In.PayloadPort);

                    if (In.Default)
                        SetDefaultTarget(In.Name);

                    return (command.ExecuteNonQuery() > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Updates the saved information about the Target specified but only extednded data.
        /// </summary>
        /// <param name="TargetName">The string name of the Target to update.</param>
        /// <param name="In">The input data to change.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool SetTargetExt(string TargetName, TargetInfo In)
        {
            if (!DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} doesn't exist in Targets Table.");

                return false;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE Targets SET Available=@NewAvailable, APIAvailable=@NewAPIAvailable WHERE TargetName=@Target";
                    command.Parameters.AddWithValue("@Target", TargetName);

                    command.Parameters.AddWithValue("@NewAvailable", In.Available);
                    command.Parameters.AddWithValue("@NewAPIAvailable", In.APIAvailable);

                    if (In.Default)
                        SetDefaultTarget(In.Name);

                    return (command.ExecuteNonQuery() > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        public static string[] ConsoleTypeNames = {
            "-",
            "Diag",
            "Devkit",
            "Testkit",
            "Retail",
            "Kratos",
        };

        /// <summary>
        /// Updates extended information about the Target. *Requires the Target to be on and the API running.
        /// </summary>
        /// <param name="ID">The Index of the target to update.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool UpdateTargetExtInfo(string TargetName)
        {
            var Target = Targets.Find(x => x.Name == TargetName);

            if (Target == null)
            {
                Console.WriteLine($"Couldn't Find Target \"{TargetName}\".");
                return false;
            }

            if (API.CallLong(Target.IPAddr, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_INFO }, out Socket Sock) != APIResults.API_OK)
            {
                Sock.Close();

                return false;
            }

            var Packet = new TargetInfoPacket();
            var RawPacket = new byte[Marshal.SizeOf(Packet)];
            var bytes = Sock.Receive(RawPacket);

            if (bytes <= 0)
                return false;

            Helpers.BytestoStruct(RawPacket, ref Packet);
            API.CompleteCall(Sock);

            if (Packet.ConsoleName == null || Packet.ConsoleName == string.Empty)
                return false;

            Target.SDKVersion = $"{((Packet.SDKVersion >> 24) & 0xFF).ToString("X1")}.{((Packet.SDKVersion >> 12) & 0xFFF).ToString("X3")}.{(Packet.SDKVersion & 0xFFF).ToString("X3")}";
            Target.SoftwareVersion = $"{((Packet.SoftwareVersion >> 24) & 0xFF).ToString("X1")}.{((Packet.SoftwareVersion >> 16) & 0xFF).ToString("X2")}";
            Target.FactorySoftwareVersion = $"{((Packet.FactorySoftwareVersion >> 24) & 0xFF).ToString("X1")}.{((Packet.FactorySoftwareVersion >> 12) & 0xFFF).ToString("X3")}.{(Packet.FactorySoftwareVersion & 0xFFF).ToString("X3")}";
            Target.CurrentTitleID = Packet.CurrentTitleID; // TODO: Implement this on console
            Target.ConsoleName = Packet.ConsoleName;
            Target.MotherboardSerial = Packet.MotherboardSerial;
            Target.Serial = Packet.Serial;
            Target.Model = Packet.Model;
            Target.MACAddressLAN = BitConverter.ToString(Packet.MACAddressLAN).Replace("-", ":");
            Target.MACAddressWIFI = "-"; // TODO: Find this on the console. BitConverter.ToString(Packet.MACAddressWIFI).Replace("-", ":");
            Target.UART = Packet.UART > 0;
            Target.IDUMode = Packet.IDUMode > 0;
            Target.IDPS = BitConverter.ToString(Packet.IDPS).Replace("-", string.Empty);
            Target.PSID = BitConverter.ToString(Packet.PSID).Replace("-", string.Empty);
            Target.ConsoleType = ConsoleTypeNames[Packet.ConsoleType];
            Target.Attached = Packet.Attached > 0;
            Target.CurrentProcessId = 0;// TODO: Update this to process Id Packet.CurrentProc;

            // Write to Database.
            using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $@"UPDATE Targets SET 
                                        SDKVersion=@SDKVersion, 
                                        SoftwareVersion=@SoftwareVersion, 
                                        FactorySoftwareVersion=@FactorySoftwareVersion, 
                                        CurrentTitleID=@CurrentTitleID, 
                                        ConsoleName=@ConsoleName,
                                        MotherboardSerial=@MotherboardSerial, 
                                        Serial=@Serial, 
                                        Model=@Model, 
                                        MACAddressLAN=@MACAddressLAN, 
                                        MACAddressWIFI=@MACAddressWIFI, 
                                        UART=@UART, 
                                        IDUMode=@IDUMode, 
                                        IDPS=@IDPS, 
                                        PSID=@PSID, 
                                        ConsoleType=@ConsoleType, 
                                        Attached=@Attached, 
                                        CurrentProcessId=@CurrentProcessId,
                                        HDDUsedSpace=@HDDUsedSpace,
                                        HDDFreeSpace=@HDDFreeSpace,
                                        HDDTotalSpace=@HDDTotalSpace 
                                        WHERE TargetName=@Target";
                command.Parameters.AddWithValue("@Target", TargetName);

                command.Parameters.AddWithValue("@SDKVersion", Target.SDKVersion);
                command.Parameters.AddWithValue("@SoftwareVersion", Target.SoftwareVersion);
                command.Parameters.AddWithValue("@FactorySoftwareVersion", Target.FactorySoftwareVersion);
                command.Parameters.AddWithValue("@CurrentTitleID", Target.CurrentTitleID);
                command.Parameters.AddWithValue("@ConsoleName", Target.ConsoleName);
                command.Parameters.AddWithValue("@MotherboardSerial", Target.MotherboardSerial);
                command.Parameters.AddWithValue("@Serial", Target.Serial);
                command.Parameters.AddWithValue("@Model", Target.Model);
                command.Parameters.AddWithValue("@MACAddressLAN", Target.MACAddressLAN);
                command.Parameters.AddWithValue("@MACAddressWIFI", Target.MACAddressWIFI);
                command.Parameters.AddWithValue("@UART", Target.UART);
                command.Parameters.AddWithValue("@IDUMode", Target.IDUMode);
                command.Parameters.AddWithValue("@IDPS", Target.IDPS);
                command.Parameters.AddWithValue("@PSID", Target.PSID);
                command.Parameters.AddWithValue("@ConsoleType", Target.ConsoleType);
                command.Parameters.AddWithValue("@Attached", Target.Attached);
                command.Parameters.AddWithValue("@CurrentProcessId", Target.CurrentProcessId);
                command.Parameters.AddWithValue("@HDDUsedSpace", Target.HDDUsedSpace);
                command.Parameters.AddWithValue("@HDDFreeSpace", Target.HDDFreeSpace);
                command.Parameters.AddWithValue("@HDDTotalSpace", Target.HDDTotalSpace);
                // TODO: Possibly have CPU stats here SOC/CPU Temp, RAM, VRAM, ThreadCount, CPUAverageUsage & CPUUsage[8].

                return (command.ExecuteNonQuery() > 0);
            }
        }

        /// <summary>
        /// Removes the Target specified.
        /// </summary>
        /// <param name="TargetName">The string name of the Target to update.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool DeleteTarget(string TargetName)
        {
            if (!DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} doesn't exist in Targets Table.");

                return false;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"DELETE FROM Targets WHERE TargetName=@Target";
                    command.Parameters.AddWithValue("@Target", TargetName);

                    return (command.ExecuteNonQuery() > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Add's a new saved Target to the database.
        /// </summary>
        /// <param name="Default"></param>
        /// <param name="TargetName"></param>
        /// <param name="IPAddr"></param>
        /// <param name="Firmware"></param>
        /// <param name="PayloadPort"></param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool NewTarget(bool Default, string TargetName, string IPAddr, int Firmware, int PayloadPort)
        {
            if (DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} already exists.");

                return false;
            }

            if (GetTargetCount() >= Config.MaxTargets)
            {
                Console.WriteLine($"Maximum number of stored targets reached {Config.MaxTargets}.");

                return false;
            }

            //Makes sure there is always a default target.
            if (!DoesDefaultTargetExist())
                Default = true;

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO Targets (TargetName, IPAddress, Firmware, PayloadPort) VALUES (@NewTargetName, @NewIPAddress, @NewFirmware, @NewPort)";

                    command.Parameters.AddWithValue("@NewTargetName", TargetName);
                    command.Parameters.AddWithValue("@NewIPAddress", IPAddr);
                    command.Parameters.AddWithValue("@NewFirmware", Firmware);
                    command.Parameters.AddWithValue("@NewPort", PayloadPort);

                    if (Default)
                        SetDefaultTarget(TargetName);

                    return (command.ExecuteNonQuery() > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Sets the current Default Target.
        /// </summary>
        /// <param name="TargetName">The string name of the Target to look up.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool SetDefaultTarget(string TargetName)
        {
            if (!DoesTargetExist(TargetName))
            {
                Console.WriteLine($"Target {TargetName} doesn't exist in Targets Table.");

                return false;
            }

            if (DefaultTarget.Name?.Equals(TargetName) ?? false)
                return true;

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Targets Set DefaultTarget=0 WHERE DefaultTarget=1";
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Targets SET DefaultTarget=1 WHERE TargetName=@Target";
                    command.Parameters.AddWithValue("@Target", TargetName);
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Gets the Default saved Target.
        /// </summary>
        /// <param name="Out"></param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool GetDefaultTarget(out TargetInfo Out)
        {
            Out = new TargetInfo();

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM Targets WHERE DefaultTarget=1";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Out.Default = reader.GetBoolean((int)DataColumns.DefaultTarget);
                            Out.Name = reader.GetString((int)DataColumns.TargetName);
                            Out.IPAddr = reader.GetString((int)DataColumns.IPAddress);
                            Out.Firmware = reader.GetInt32((int)DataColumns.Firmware);
                            Out.PayloadPort = reader.GetInt32((int)DataColumns.PayloadPort);
                            Out.Available = reader.GetBoolean((int)DataColumns.Available);
                            Out.APIAvailable = reader.GetBoolean((int)DataColumns.APIAvailable);
                            Out.SDKVersion = reader.GetString((int)DataColumns.SDKVersion);
                            Out.SoftwareVersion = reader.GetString((int)DataColumns.SoftwareVersion);
                            Out.FactorySoftwareVersion = reader.GetString((int)DataColumns.FactorySoftwareVersion);
                            Out.CurrentTitleID = reader.GetString((int)DataColumns.CurrentTitleID);
                            Out.ConsoleName = reader.GetString((int)DataColumns.ConsoleName);
                            Out.MotherboardSerial = reader.GetString((int)DataColumns.MotherboardSerial);
                            Out.Serial = reader.GetString((int)DataColumns.Serial);
                            Out.Model = reader.GetString((int)DataColumns.Model);
                            Out.MACAddressLAN = reader.GetString((int)DataColumns.MACAddressLAN);
                            Out.MACAddressWIFI = reader.GetString((int)DataColumns.MACAddressWIFI);
                            Out.UART = reader.GetBoolean((int)DataColumns.UART);
                            Out.IDUMode = reader.GetBoolean((int)DataColumns.IDUMode);
                            Out.IDPS = reader.GetString((int)DataColumns.IDPS);
                            Out.PSID = reader.GetString((int)DataColumns.PSID);
                            Out.ConsoleType = reader.GetString((int)DataColumns.ConsoleType);
                            Out.Attached = reader.GetBoolean((int)DataColumns.Attached);
                            Out.CurrentProcessId = reader.GetInt32((int)DataColumns.CurrentProcessId);
                            Out.HDDUsedSpace = reader.GetInt32((int)DataColumns.HDDUsedSpace);
                            Out.HDDFreeSpace = reader.GetInt32((int)DataColumns.HDDFreeSpace);
                            Out.HDDTotalSpace = reader.GetInt32((int)DataColumns.HDDTotalSpace);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Gets the number of saved Targets.
        /// </summary>
        /// <returns>Returns the number of saved Targets.</returns>
        public static int GetTargetCount()
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT COUNT(*)
                        FROM Targets
                    ";

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Gets a list of saved Targets and their information.
        /// </summary>
        /// <param name="Out">Array of Targets.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool GetTargetList(out List<TargetInfo> Out)
        {
            Out = new List<TargetInfo>();
            if (TargetCount <= 0)
            {
                Console.WriteLine("There is no Targets saved...");

                return true;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM Targets";

                    using (var reader = command.ExecuteReader())
                    {
                        var Counter = 0;

                        while (reader.Read())
                        {
                            var temp = new TargetInfo();
                            temp.Default = reader.GetBoolean((int)DataColumns.DefaultTarget);
                            temp.Name = reader.GetString((int)DataColumns.TargetName);
                            temp.IPAddr = reader.GetString((int)DataColumns.IPAddress);
                            temp.Firmware = reader.GetInt32((int)DataColumns.Firmware);
                            temp.PayloadPort = reader.GetInt32((int)DataColumns.PayloadPort);
                            temp.Available = reader.GetBoolean((int)DataColumns.Available);
                            temp.APIAvailable = reader.GetBoolean((int)DataColumns.APIAvailable);
                            temp.SDKVersion = reader.GetString((int)DataColumns.SDKVersion);
                            temp.SoftwareVersion = reader.GetString((int)DataColumns.SoftwareVersion);
                            temp.FactorySoftwareVersion = reader.GetString((int)DataColumns.FactorySoftwareVersion);
                            temp.CurrentTitleID = reader.GetString((int)DataColumns.CurrentTitleID);
                            temp.ConsoleName = reader.GetString((int)DataColumns.ConsoleName);
                            temp.MotherboardSerial = reader.GetString((int)DataColumns.MotherboardSerial);
                            temp.Serial = reader.GetString((int)DataColumns.Serial);
                            temp.Model = reader.GetString((int)DataColumns.Model);
                            temp.MACAddressLAN = reader.GetString((int)DataColumns.MACAddressLAN);
                            temp.MACAddressWIFI = reader.GetString((int)DataColumns.MACAddressWIFI);
                            temp.UART = reader.GetBoolean((int)DataColumns.UART);
                            temp.IDUMode = reader.GetBoolean((int)DataColumns.IDUMode);
                            temp.IDPS = reader.GetString((int)DataColumns.IDPS);
                            temp.PSID = reader.GetString((int)DataColumns.PSID);
                            temp.ConsoleType = reader.GetString((int)DataColumns.ConsoleType);
                            temp.Attached = reader.GetBoolean((int)DataColumns.Attached);
                            temp.CurrentProcessId = reader.GetInt32((int)DataColumns.CurrentProcessId);
                            temp.HDDUsedSpace = reader.GetInt32((int)DataColumns.HDDUsedSpace);
                            temp.HDDFreeSpace = reader.GetInt32((int)DataColumns.HDDFreeSpace);
                            temp.HDDTotalSpace = reader.GetInt32((int)DataColumns.HDDTotalSpace);

                            Out.Add(temp);

                            Counter++;
                        }

                        if(Counter != TargetCount)
                        {
                            Console.WriteLine($"Failed to get Target List. Read count didnt match rows. Count {Counter.ToString()} != Actual {TargetCount.ToString()}.");

                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }
    }
}
