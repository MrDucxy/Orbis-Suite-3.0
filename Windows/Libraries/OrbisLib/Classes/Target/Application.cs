using OrbisSuite.Common;
using OrbisSuite.Common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrbisSuite
{
    public record AppInfo(string TitleId, string ContentId, string TitleName, string MetaDataPath, DateTime LastAccessTime, 
        int Visible, int SortPriority, int DisplayLocation, bool CanRemove, string Category, int ContentSize, DateTime InstallDate, string UICategory);

    public class Application
    {
        private Target Target;

        public Application(Target Target)
        {
            this.Target = Target;
        }

        public List<AppInfo> GetAppList()
        {
            var AppList = new List<AppInfo>();

            // Check to see the API is online.
            if (!Target.Info.Details.IsAPIAvailable)
            {
                return AppList;
            }

            // Make the initial request.
            APIResults apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APPS_GET_LIST }, out Socket Sock);

            // Make sure the request was sucessful.
            if (apiResult != APIResults.API_OK)
                return AppList;

            // Get the number of apps installed.
            int Count = Sock.RecvInt32();

            // Itterate through the count to recieve all the apps details.
            for (int i = 0; i < Count; i++)
            {
                // Recieve the bytes of the struct.
                var Packet = new AppInfoPacket();
                var RawPacket = new byte[Marshal.SizeOf(Packet)];
                var bytes = Sock.Receive(RawPacket);

                if (bytes <= 0)
                    break;

                // Convert the recieved bytes to a struct.
                Helpers.BytestoStruct(RawPacket, ref Packet);

                // Try to parse the date time strings.
                if (!DateTime.TryParse(Packet.LastAccessTime, out DateTime LastAccessTime))
                    LastAccessTime = DateTime.MinValue;

                if (!DateTime.TryParse(Packet.LastAccessTime, out DateTime InstallDate))
                    InstallDate = DateTime.MinValue;

                // For some reason there is garbage after the string so this stops that :)
                var firstNullIndex = Array.FindIndex(Packet.TitleName, b => b == 0);
                string titleName = Encoding.UTF8.GetString(Packet.TitleName, 0, firstNullIndex);

                // Add the entry to the list.
                AppList.Add(new AppInfo(Packet.TitleId, Packet.ContentId, titleName, Packet.MetaDataPath, LastAccessTime, Packet.Visible, 
                    Packet.SortPriority, Packet.DispLocation, Packet.CanRemove == 1, Packet.Category, Packet.ContentSize, InstallDate, Packet.UICategory));
            }

            // close socket.
            Sock.Close();

            return AppList;
        }

        public string GetAppInfoString(string TitleId, string Key)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return string.Empty;
            }

            if (!Target.Info.Details.IsAPIAvailable)
            {
                return string.Empty;
            }

            var apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APPS_GET_INFO_STR }, out Socket Sock);

            if (apiResult != APIResults.API_OK)
                return string.Empty;

            // Send the titleId of the app.
            Sock.Send(Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray());

            // Send the bytes of the key string.
            Sock.Send(Encoding.ASCII.GetBytes(Key.PadRight(50, '\0')));

            var resultBuffer = new byte[200];
            Sock.Receive(resultBuffer);

            // close socket.
            Sock.Close();

            return Encoding.ASCII.GetString(resultBuffer);
        }

        public AppState GetAppState(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return AppState.STATE_ERROR;
            }

            if (!Target.Info.Details.IsAPIAvailable)
            {
                return AppState.STATE_NOT_RUNNING;
            }

            APIResults apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APPS_STATUS }, out Socket Sock);

            if (apiResult != APIResults.API_OK)
                return AppState.STATE_ERROR;

            // Send the titleId of the app.
            var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
            Sock.Send(bytes);

            // Get the state from API.
            var result = (AppState)Sock.RecvInt32();

            // close socket.
            Sock.Close();

            return result;
        }

        public bool Start(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            if (!Target.Info.Details.IsAPIAvailable)
            {
                return false;
            }

            APIResults apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APPS_START }, out Socket Sock);

            if (apiResult != APIResults.API_OK)
                return false;

            // Send the titleId of the app.
            var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
            Sock.Send(bytes);

            // Get the state from API.
            var result = Sock.RecvInt32();

            // close socket.
            Sock.Close();

            return (result == 1);
        }

        public bool Stop(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            if (!Target.Info.Details.IsAPIAvailable)
            {
                return false;
            }

            APIResults apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APPS_STOP }, out Socket Sock);

            if (apiResult != APIResults.API_OK)
                return false;

            // Send the titleId of the app.
            var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
            Sock.Send(bytes);

            // Get the state from API.
            var result = Sock.RecvInt32();

            // close socket.
            Sock.Close();

            return (result == 1);
        }
    }
}
