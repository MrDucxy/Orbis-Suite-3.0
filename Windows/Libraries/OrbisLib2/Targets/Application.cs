using OrbisLib2.Common.API;
using OrbisLib2.Common.Helpers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace OrbisLib2.Targets
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

            var result = API.SendCommand(Target, 5, APICommands.API_APPS_GET_LIST, (Socket Sock, APIResults Result) =>
            {
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
                    Helper.BytesToStruct(RawPacket, ref Packet);

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
            });

            return AppList;
        }

        public string GetAppInfoString(string TitleId, string Key)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return string.Empty;
            }

            var resultBuffer = new byte[200];
            var result = API.SendCommand(Target, 5, APICommands.API_APPS_GET_INFO_STR, (Socket Sock, APIResults Result) => 
            {
                // Send the titleId of the app.
                Sock.Send(Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray());

                // Send the bytes of the key string.
                Sock.Send(Encoding.ASCII.GetBytes(Key.PadRight(50, '\0')));

                Sock.Receive(resultBuffer);
            });

            return Encoding.ASCII.GetString(resultBuffer);
        }

        public AppState GetAppState(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return AppState.STATE_ERROR;
            }

            AppState result = AppState.STATE_ERROR;
            API.SendCommand(Target, 5, APICommands.API_APPS_STATUS, (Socket Sock, APIResults Result) => 
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = (AppState)Sock.RecvInt32();
            });

            return result;
        }

        public bool Start(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_START, (Socket Sock, APIResults Result) => 
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }

        public bool Stop(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_STOP, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }

        public bool Suspend(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_SUSPEND, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }

        public bool Resume(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_RESUME, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }
    }
}
