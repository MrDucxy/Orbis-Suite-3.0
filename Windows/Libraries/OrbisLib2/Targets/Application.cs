using OrbisLib2.Common.API;
using OrbisLib2.Common.Helpers;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
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
        public enum VisibilityType : int
        {
            VT_NONE,
            VT_VISIBLE,
            VT_INVISIBLE,
        };

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
                var Count = Sock.RecvInt32();

                // Recieve all of the arrary as one large packet.
                var dataSize = Count * Marshal.SizeOf(typeof(AppInfoPacket));
                var data = new byte[dataSize];
                Sock.RecvLarge(data);

                // Allocate and copy the packet to begin marshaling it.
                IntPtr ptr = Marshal.AllocHGlobal(dataSize);
                Marshal.Copy(data, 0, ptr, dataSize);

                for(int i = 0; i < Count; i++)
                {
                    // Marshal each part of the buffer to a struct.
                    var Packet = new AppInfoPacket();
                    Packet = (AppInfoPacket)Marshal.PtrToStructure(IntPtr.Add(ptr, i * Marshal.SizeOf(typeof(AppInfoPacket))), typeof(AppInfoPacket));

                    // Try to parse the date time strings.
                    if (!DateTime.TryParse(Packet.LastAccessTime, out DateTime LastAccessTime))
                        LastAccessTime = DateTime.MinValue;

                    if (!DateTime.TryParse(Packet.LastAccessTime, out DateTime InstallDate))
                        InstallDate = DateTime.MinValue;

                    // For some reason there is garbage after the string so this stops that :)
                    var firstNullIndex = Array.FindIndex(Packet.TitleName, b => b == 0);
                    string titleName = Encoding.UTF8.GetString(Packet.TitleName, 0, firstNullIndex);

                    AppList.Add(new AppInfo(Packet.TitleId, Packet.ContentId, titleName, Packet.MetaDataPath, LastAccessTime, Packet.Visible,
                        Packet.SortPriority, Packet.DispLocation, Packet.CanRemove == 1, Packet.Category, Packet.ContentSize, InstallDate, Packet.UICategory));
                }

                Marshal.FreeHGlobal(ptr);
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

        public bool Delete(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_DELETE, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }

        public bool SetVisibility(string TitleId, VisibilityType Visibility)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return false;
            }

            int result = 0;
            API.SendCommand(Target, 5, APICommands.API_APPS_SET_VISIBILITY, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Send the visibility state.
                Sock.SendInt32((int)Visibility);

                // Get the state from API.
                result = Sock.RecvInt32();
            });

            return (result == 1);
        }

        public VisibilityType GetVisibility(string TitleId)
        {
            if (!Regex.IsMatch(TitleId, @"[a-zA-Z]{4}\d{5}"))
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return VisibilityType.VT_NONE;
            }

            VisibilityType result = VisibilityType.VT_NONE;
            API.SendCommand(Target, 5, APICommands.API_APPS_GET_VISIBILITY, (Socket Sock, APIResults Result) =>
            {
                // Send the titleId of the app.
                var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(10, '\0')).Take(10).ToArray();
                Sock.Send(bytes);

                // Get the state from API.
                result = (VisibilityType)Sock.RecvInt32();
            });

            return result;
        }
    }
}
