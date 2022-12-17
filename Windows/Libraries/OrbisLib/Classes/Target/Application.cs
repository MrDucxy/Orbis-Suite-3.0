﻿using OrbisSuite.Common;
using OrbisSuite.Common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OrbisSuite
{
    public class Application
    {
        private Target Target;

        public Application(Target Target)
        {
            this.Target = Target;
        }

        public AppState GetAppState(string TitleId)
        {
            if(TitleId.Length > 10)
            {
                Console.WriteLine($"Invaild titleId format {TitleId}");
                return AppState.STATE_ERROR;
            }

            if (!Target.Info.Details.IsAPIAvailable)
            {
                return AppState.STATE_NOT_RUNNING;
            }

            APIResults apiResult = API.CallLong(Target.Info.IPAddress, Settings.CreateInstance().APIPort, new APIPacket() { PacketVersion = Config.PacketVersion, Command = APICommands.API_APP_STATUS }, out Socket Sock);

            if (apiResult != APIResults.API_OK)
                return AppState.STATE_ERROR;

            // Send the titleId of the app.
            var bytes = Encoding.ASCII.GetBytes(TitleId.PadRight(9, '\0')).Take(9).ToArray();
            Sock.Send(bytes);

            // Get the state from API.
            var result = (AppState)Sock.RecvInt32();

            // close socket.
            Sock.Close();

            return result;
        }
    }
}