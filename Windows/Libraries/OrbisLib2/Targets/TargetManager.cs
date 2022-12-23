using Microsoft.Extensions.Logging;
using OrbisLib2.Common.API;
using OrbisLib2.Common.Database;
using OrbisLib2.Common.Database.Types;
using OrbisLib2.Common.Helpers;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace OrbisLib2.Targets
{
    public class TargetManager
    {

        /// <summary>
        /// Returns a list of the Targets saved.
        /// </summary>
        public static List<Target> Targets
        {
            get
            {
                var temporaryList = new List<Target>();
                var savedTargets = SavedTarget.GetTargetList();

                if (savedTargets == null)
                    return temporaryList;

                foreach (var target in savedTargets)
                {
                    temporaryList.Add(new Target(target));
                }

                return temporaryList;
            }
        }

        private static Target _SelectedTarget;

        /// <summary>
        /// Gets the currently selected target.
        /// </summary>
        public static Target SelectedTarget
        {
            get
            {
                // Set initially as the default target.
                if (SelectedTarget == null)
                {
                    var defaultTarget = SavedTarget.FindDefaultTarget();

                    if (defaultTarget != null)
                    {
                        _SelectedTarget = new Target(defaultTarget);
                    }
                }

                return _SelectedTarget;
            }
            set { _SelectedTarget = value; }
        }

        /// <summary>
        /// Gets the specified target by name.
        /// </summary>
        /// <param name="TargetName">The specified target name.</param>
        /// <param name="Out">Target output.</param>
        /// <returns>Returns true if target is found.</returns>
        public static Target? GetTarget(string TargetName)
        {
            var saveedTarget = SavedTarget.FindTarget(x => x.Name == TargetName);

            if (saveedTarget == null)
            {
                return null;
            }

            return new Target(saveedTarget);
        }

        /// <summary>
        /// Deletes the specified target.
        /// </summary>
        /// <param name="TargetName">The specified target name.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool DeleteTarget(string TargetName)
        {
            var Target = SavedTarget.FindTarget(x => x.Name == TargetName);
            return Target.Remove();
        }

        /// <summary>
        /// Adds a new target.
        /// </summary>
        /// <param name="Default">If this target the new default.</param>
        /// <param name="TargetName">The mame for this target.</param>
        /// <param name="IPAddress">IP Address of this target.</param>
        /// <param name="PayloadPort">The payload port to be used for this target.</param>
        /// <returns>Returns true if successful.</returns>
        public static bool NewTarget(bool Default, string TargetName, string IPAddress, int PayloadPort)
        {
            return new SavedTarget { IsDefault = Default, Name = TargetName, IPAddress = IPAddress, PayloadPort = PayloadPort }.Add();
        }

        /// <summary>
        /// Updates extended information about the Target. *Requires the Target to be on and the API running.
        /// </summary>
        /// <param name="TargetName">The name of the target to update.</param>
        /// <returns>Returns weather or not the action was successful or not.</returns>
        public static bool UpdateTargetInfo(string TargetName)
        {
            Target? Target = GetTarget(TargetName);
            if (Target == null)
            {
                Console.WriteLine($"Couldn't Find Target \"{TargetName}\".");
                return false;
            }

            bool labdaReult = false;
            var result = API.SendCommand(Target, 3000, APICommands.API_TARGET_INFO, (Sock, Result) =>
            {
                var Packet = new TargetInfoPacket();
                var RawPacket = new byte[Marshal.SizeOf(Packet)];
                var bytes = Sock.Receive(RawPacket);

                if (bytes <= 0)
                {
                    return;
                }

                Helper.BytesToStruct(RawPacket, ref Packet);

                if (Packet.ConsoleName == null || Packet.ConsoleName == string.Empty)
                    return;

                Target.Info.SDKVersion = $"{(Packet.SDKVersion >> 24 & 0xFF).ToString("X1")}.{(Packet.SDKVersion >> 12 & 0xFFF).ToString("X3")}.{(Packet.SDKVersion & 0xFFF).ToString("X3")}";
                Target.Info.SoftwareVersion = $"{(Packet.SoftwareVersion >> 24 & 0xFF).ToString("X1")}.{(Packet.SoftwareVersion >> 16 & 0xFF).ToString("X2")}";
                Target.Info.FactorySoftwareVersion = $"{(Packet.FactorySoftwareVersion >> 24 & 0xFF).ToString("X1")}.{(Packet.FactorySoftwareVersion >> 12 & 0xFFF).ToString("X3")}.{(Packet.FactorySoftwareVersion & 0xFFF).ToString("X3")}";
                Target.Info.CurrentTitleID = Packet.CurrentTitleID;
                Target.Info.ConsoleName = Packet.ConsoleName;
                Target.Info.MotherboardSerial = Packet.MotherboardSerial;
                Target.Info.Serial = Packet.Serial;
                Target.Info.Model = Packet.Model;
                Target.Info.MACAddressLAN = Packet.MACAddressLAN.ToUpper();
                Target.Info.MACAddressWIFI = Packet.MACAddressWIFI.ToUpper();
                Target.Info.UART = Packet.UART > 0;
                Target.Info.IDUMode = Packet.IDUMode > 0;
                Target.Info.IDPS = BitConverter.ToString(Packet.IDPS).Replace("-", string.Empty);
                Target.Info.PSID = BitConverter.ToString(Packet.PSID).Replace("-", string.Empty);
                Target.Info.ConsoleType = (ConsoleType)Packet.ConsoleType;

                // Debugging.
                Target.Info.IsAttached = Packet.Attached > 0;
                // TODO: Implement this into the API.
                Target.Info.CurrentProcessId = 0;// TODO: Update this to process Id Packet.CurrentProc;

                // Storage.
                Target.Info.HDDUsedSpace = (long)(Packet.TotalSpace - Packet.FreeSpace);
                Target.Info.HDDFreeSpace = (long)Packet.FreeSpace;
                Target.Info.HDDTotalSpace = (long)Packet.TotalSpace;

                // Perf Stats.
                Target.Info.CPUTemp = Packet.CPUTemp;
                Target.Info.SOCTemp = Packet.SOCTemp;
                Target.Info.ThreadCount = Packet.ThreadCount;
                Target.Info.AverageCPUUsage = Packet.AverageCPUUsage;
                Target.Info.BusyCore = Packet.BusyCore;
                Target.Info.RamUsage = Packet.Ram.Used;
                Target.Info.VRamUsage = Packet.VRam.Used;

                labdaReult = true;
            });

            return result == APIResults.API_OK && labdaReult && Target.Info.Save();
        }
    }
}
