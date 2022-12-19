using System.Net.Sockets;

namespace OrbisSuite.Common
{
    public static class API
    {
        /// <summary>
        /// Connects to the API.
        /// </summary>
        /// <param name="IPAddr">Address we would like to connnect to.</param>
        /// <param name="Sock">The socket we have connected on.</param>
        /// <returns>Returns true if we successfully connected.</returns>
        public static bool Connect(string IPAddr, int Port, out Socket Sock)
        {
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            return Sock.EasyConnect(IPAddr, Port, -1);
        }

        /// <summary>
        /// Short API Call used when you only need to send the initial packet and no additional data.
        /// </summary>
        /// <param name="IPAddr">Address to send to.</param>
        /// <param name="Packet">The initial Packet Data.</param>
        /// <returns>Returns the result of the action.</returns>
        public static APIResults Call(string IPAddr, int Port, APIPacket Packet)
        {
            try
            {
                if (Connect(IPAddr, Port, out Socket Sock))
                {
                    // Send Inital Packet.
                    Sock.Send(Helpers.StructtoBytes(Packet));

                    // Get API Response.
                    var Result = (APIResults)Sock.RecvInt32();

                    // Clean up.
                    Sock.Close();

                    return Result;
                }
                else
                    return APIResults.API_ERROR_COULDNT_CONNECT;
            }
            catch
            {

            }

            return APIResults.API_ERROR_GENERAL;
        }

        /// <summary>
        /// Long API Call used for when we need to send more than just the initial packet.
        /// </summary>
        /// <param name="IPAddr">Address to send to.</param>
        /// <param name="Packet">The initial Packet Data.</param>
        /// <param name="Sock">The soucket out used for sending more data.</param>
        /// <returns>Returns the result of the action.</returns>
        public static APIResults CallLong(string IPAddr, int Port, APIPacket Packet, out Socket Sock)
        {
            Sock = null;

            try
            {
                if (Connect(IPAddr, Port, out Sock))
                {
                    // Send Inital Packet.
                    Sock.Send(Helpers.StructtoBytes(Packet));

                    // Get API Response.
                    return (APIResults)Sock.RecvInt32();
                }
                else
                    return APIResults.API_ERROR_COULDNT_CONNECT;
            }
            catch (Exception ex)
            {

            }

            return APIResults.API_ERROR_GENERAL;
        }

        /// <summary>
        /// Sends additional data if required.
        /// </summary>
        /// <typeparam name="T">The packet type.</typeparam>
        /// <param name="Sock">The socket instance were using.</param>
        /// <param name="Packet">Any Packet structure.</param>
        /// <returns>Returns the result of the action.</returns>
        public static APIResults SendNextPacket<T>(Socket Sock, T Packet)
        {
            try
            {
                // Send Next Packet.
                Sock.Send(Helpers.StructtoBytes(Packet));
                
                // Get API Response.
                return (APIResults)Sock.RecvInt32();
            }
            catch
            {

            }

            return APIResults.API_ERROR_GENERAL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sock"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static APIResults SendInt32(Socket Sock, int Value)
        {
            try
            {
                // Send Next Packet.
                Sock.Send(BitConverter.GetBytes(Value));

                // Get API Response.
                return (APIResults)Sock.RecvInt32();
            }
            catch
            {

            }

            return APIResults.API_ERROR_GENERAL;
        }

        /// <summary>
        /// Completes the call and gets our final result.
        /// </summary>
        /// <param name="Sock">The socket used to communicate witht the API.</param>
        /// <returns>Returns the result of the action.</returns>
        public static APIResults CompleteCall(Socket Sock)
        {
            try
            {
                var Results = (APIResults)Sock.RecvInt32();

                Sock.Close();

                return Results;
            }
            catch
            {

            }

            return APIResults.API_ERROR_GENERAL;
        }
    }
}
