using OrbisSuite.Common.Database;
using OrbisSuite.Common.Database.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OrbisSuite.Common.Database
{
    public static class SavedTargets
    {
        /// <summary>
        /// Returns a list of the Targets saved.
        /// </summary>
        public static List<TargetInfo>? Targets
        {
            get
            {
                return TargetInfo.GetTargetList();
            }
        }

        /// <summary>
        /// Returns a count of all the Targets currently saved.
        /// </summary>
        public static int TargetCount
        {
            get
            {
                if(Targets == null)
                    return 0;

                return Targets.Count;
            }
        }

        /// <summary>
        /// Returns the target that is the current Default Target.
        /// </summary>
        public static TargetInfo? DefaultTarget
        {
            get
            {
                return TargetInfo.FindDefaultTarget();
            }
        }

        /// <summary>
        /// Updates extended information about the Target. *Requires the Target to be on and the API running.
        /// </summary>
        /// <param name="TargetName">The name of the target to update.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool UpdateTargetInfo(string TargetName)
        {
            var Target = TargetInfo.FindTarget(x => x.Name == TargetName);

            if (Target == null)
            {
                Console.WriteLine($"Couldn't Find Target \"{TargetName}\".");
                return false;
            }

            if (API.CallLong(Target.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_INFO }, out Socket Sock) != APIResults.API_OK)
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
            Target.CurrentTitleID = Packet.CurrentTitleID;
            Target.ConsoleName = Packet.ConsoleName;
            Target.MotherboardSerial = Packet.MotherboardSerial;
            Target.Serial = Packet.Serial;
            Target.Model = Packet.Model;
            Target.MACAddressLAN = Packet.MACAddressLAN.ToUpper();
            Target.MACAddressWIFI = Packet.MACAddressWIFI.ToUpper();
            Target.UART = Packet.UART > 0;
            Target.IDUMode = Packet.IDUMode > 0;
            Target.IDPS = BitConverter.ToString(Packet.IDPS).Replace("-", string.Empty);
            Target.PSID = BitConverter.ToString(Packet.PSID).Replace("-", string.Empty);
            Target.ConsoleType = (ConsoleType)Packet.ConsoleType;

            // Debugging.
            Target.IsAttached = Packet.Attached > 0;
            // TODO: Implement this into the API.
            Target.CurrentProcessId = 0;// TODO: Update this to process Id Packet.CurrentProc;

            // Storage.
            Target.HDDUsedSpace = (long)(Packet.TotalSpace - Packet.FreeSpace);
            Target.HDDFreeSpace = (long)Packet.FreeSpace;
            Target.HDDTotalSpace = (long)Packet.TotalSpace;

            Target.CPUTemp = Packet.CPUTemp;
            Target.SOCTemp = Packet.SOCTemp;
            Target.ThreadCount = Packet.ThreadCount;
            Target.AverageCPUUsage = Packet.AverageCPUUsage;

            return Target.Save();
        }
    }
}
