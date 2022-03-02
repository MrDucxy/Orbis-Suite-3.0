using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace OrbisSuite.Common
{
    public static class Helpers
    {
        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        /// <param name="obj">The Object to convert.</param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>
        /// <param name="arrBytes">The array to convert.</param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream);
            }
        }

        /// <summary>
        /// Sends an object and its size to the socket.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="obj">The object we would like to send. (Must be serializable.)</param>
        public static void SendObject(this Socket s, object obj)
        {
            var Data = ObjectToByteArray(obj);
            s.Send(BitConverter.GetBytes(Data.Length), sizeof(int), SocketFlags.None);
            s.Send(Data);
        }

        /// <summary>
        /// Recieve an object from a socket.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Returns the object we would like to receive.</returns>
        public static object RecvObject(this Socket s)
        {
            var ObjectSize = s.RecvInt32();

            var ObjectData = new byte[ObjectSize];
            s.Receive(ObjectData);

            return ByteArrayToObject(ObjectData);
        }

        /// <summary>
        /// Sends an int32 over socket.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="Data"></param>
        public static void SendInt32(this Socket s, int Data)
        {
            s.Send(BitConverter.GetBytes(Data));
        }


        /// <summary>
        /// Receives an int32 over sockets.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int RecvInt32(this Socket s)
        {
            var Data = new byte[sizeof(int)];
            s.Receive(Data);
            return BitConverter.ToInt32(Data, 0);
        }

        /// <summary>
        /// Easily connect to a socket and handle the time out.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="IPAddress">The address we would like to connect to.</param>
        /// <param name="Port">The port of the socket we would like to connect to.</param>
        /// <param name="TimeOut">The time we would like to wait for connection.</param>
        /// <returns></returns>
        public static bool EasyConnect(this Socket s, string IPAddress, int Port, int TimeOut)
        {
            s.ReceiveTimeout = s.SendTimeout = TimeOut;
            var result = s.BeginConnect(IPAddress, Port, null, null);

            result.AsyncWaitHandle.WaitOne(3000, true);

            if (!s.Connected)
            {
                Console.WriteLine("Failed to connect to socket.");

                s.Close();
                return false;
            }

            // we have connected
            s.EndConnect(result);

            return true;
        }

        /// <summary>
        /// Tests the availability of a tcp host.
        /// </summary>
        /// <param name="Host">Host Address.</param>
        /// <param name="Port">Host Port.</param>
        /// <returns></returns>
        public static bool TestTcpConnection(string Host, int Port)
        {
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(Host, Port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (!success)
                {
                    return false;
                }

                client.EndConnect(result);

                return true;
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// Attempts to ping a host.
        /// </summary>
        /// <param name="Host">Host Address</param>
        /// <returns></returns>
        public static bool PingHost(string Host)
        {
            try
            {
                var pingSender = new Ping();
                var options = new PingOptions();
                options.DontFragment = true;

                var reply = pingSender.Send(Host, 120, Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), options);
                return (reply.Status == IPStatus.Success);
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StructtoBytes(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="str"></param>
        public static void BytestoStruct<T>(byte[] arr, ref T str)
        {
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);
        }
    }
}
