using OrbisSuite.Common;
using System.Net.Sockets;

namespace OrbisSuiteService.Service
{
    class ClientInstance
    {
        /// <summary>
        /// The port used to communicate with the client.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// The last known name of the client.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The last time the client sent a heartbeat packet.
        /// </summary>
        public int LastUpdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Port">Port of the client.</param>
        /// <param name="Name">Name of the client</param>
        /// <param name="LastUpdate">Set the last time we got a packet from the client.</param>
        public ClientInstance(int Port, string Name, int LastUpdate)
        {
            this.Port = Port;
            this.Name = Name;
            this.LastUpdate = LastUpdate;
        }

        /// <summary>
        /// Sends a seralizable object to the client.
        /// </summary>
        /// <param name="obj">A serializable object we would like to send.</param>
        /// <returns>Returns if the data was sent or not.</returns>
        public bool SendObject(object obj)
        {
            try
            {
                var Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (Sock.EasyConnect("127.0.0.1", Port, 1000))
                {
                    Sock.SendObject(obj);

                    Sock.Close();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }
    }
}
