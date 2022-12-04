using OrbisSuite.Common;
using OrbisSuite.Common.Database;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace OrbisSuite
{
    public class ProcessInfo
    {
        public Int32 PID;
        public bool Attached;
        public string Name;
        public string TitleID;
        public UInt64 TextSegmentBase;
        public UInt64 TextSegmentLen;
        public UInt64 DataSegmentBase;
        public UInt64 DataSegmentLen;

        public ProcessInfo(Int32 PID, string Name, string TitleID)
        {
            this.PID = PID;
            this.Name = Name;
            this.TitleID = TitleID;
        }

        public ProcessInfo()
        {
            this.Attached = false;
        }
    }

    public class Process
    {
        private OrbisLib PS4;
        private Target Target;
        public Memory Memory;

        public Process(OrbisLib PS4, Target Target)
        {
            this.PS4 = PS4;
            this.Target = Target;

            Memory = new Memory(Target, this);
        }

        public IEnumerable<ProcessInfo> List
        {
            get
            {
                if (!Target.Info.Details.IsAPIAvailable)
                {
                    yield break;
                }

                if (API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_PROC_GET_LIST }, out Socket Sock) != APIResults.API_OK)
                {
                    Sock.Close();

                    yield break;
                }

                var ProcCount = Sock.RecvInt32();

                for (var i = 0; i < ProcCount; i++)
                {
                    var Packet = new ProcPacket();
                    var RawPacket = new byte[Marshal.SizeOf(Packet)];
                    Sock.Receive(RawPacket);

                    Helpers.BytestoStruct(RawPacket, ref Packet);
                    yield return new ProcessInfo(Packet.ProcessID, Packet.Name, Packet.TitleID);
                }

                API.CompleteCall(Sock);
            }
        }
    }
}
