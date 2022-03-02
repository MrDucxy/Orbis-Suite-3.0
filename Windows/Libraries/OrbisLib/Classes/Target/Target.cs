using OrbisSuite.Common;
using OrbisSuite.Common.DataBase;
using System.Net.Sockets;

namespace OrbisSuite
{
    public class Target
    {
        public bool Active = false;
        public TargetInfo Info;

        private OrbisLib PS4;
        public TargetEvents Events;
        public Debug Debug;
        public Payload Payload;
        public Process Process;
        public FTP FTP;

        public Target(OrbisLib PS4)
        {
            Active = false;
            Info = new TargetInfo();
            this.PS4 = PS4;
            Events = new TargetEvents(this);
            Debug = new Debug(this);
            Payload = new Payload(this);
            Process = new Process(PS4, this);
            FTP = new FTP(this);
        }

        public Target(OrbisLib PS4, TargetInfo TargetInfo)
        {
            Active = true;
            Info = TargetInfo;
            this.PS4 = PS4;
            Events = new TargetEvents(this);
            Debug = new Debug(this);
            Payload = new Payload(this);
            Process = new Process(PS4, this);
            FTP = new FTP(this);
        }

        public APIResults Shutdown()
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            return API.Call(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_SHUTDOWN });
        }

        public APIResults Reboot()
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            return API.Call(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_REBOOT });
        }

        public APIResults Suspend()
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            return API.Call(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_RESTMODE });
        }

        public APIResults Notify(string Message)
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            APIResults Result = API.CallLong(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_NOTIFY }, out Socket Sock);

            if (Result != APIResults.API_OK)
                return Result;

            Result = API.SendNextPacket(Sock, new TargetNotifyPacket() { Message = Message });

            Sock.Close();

            return Result;
        }

        public APIResults Notify(string IconURI, string Message)
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            APIResults Result = API.CallLong(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_NOTIFY }, out Socket Sock);

            if (Result != APIResults.API_OK)
                return Result;

            Result = API.SendNextPacket(Sock, new TargetNotifyPacket() { IconURI = IconURI, Message = Message });

            Sock.Close();

            return Result;
        }

        public APIResults Buzzer(BuzzerType Type)
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            APIResults Result = API.CallLong(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_BUZZER }, out Socket Sock);

            if (Result != APIResults.API_OK)
                return Result;

            Result = API.SendInt32(Sock, (int)Type);

            Sock.Close();

            return Result;
        }

        public APIResults SetLED(ConsoleLEDColours Colour)
        {
            if (!Info.APIAvailable)
            {
                Console.WriteLine($"Attempted to call '{System.Reflection.MethodBase.GetCurrentMethod().Name}' but API not Available on target '{Info.Name}'.");
                return APIResults.API_ERROR_NOT_CONNECTED;
            }

            APIResults Result = API.CallLong(Info.IPAddr, Settings.APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_TARGET_SET_LED }, out Socket Sock);

            if (Result != APIResults.API_OK)
                return Result;

            Result = API.SendInt32(Sock, (int)Colour);

            Sock.Close();

            return Result;
        }

        /*public APIResults DumpProcess(string ProcName, int Length, byte[] Out)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Length);

            UInt64 RealLength = 0;
            API_ERRORS Result = Imports.Target.DumpProcess(Info.IPAddr, ProcName, out RealLength, ptr);

            Marshal.Copy(ptr, Out, 0, Length);

            //free unmanageed memory.
            Marshal.FreeHGlobal(ptr);

            return Result;
        }*/
    }
}
