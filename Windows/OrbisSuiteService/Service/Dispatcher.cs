using OrbisSuite.Common;
using OrbisSuite.Common.Dispatcher;
using OrbisSuite.Common.Database;
using System.Net.Sockets;

namespace OrbisSuiteService.Service
{
    public class Dispatcher
    {
        private static readonly int _maxClients = 20;
        private readonly int _clientMax = Config.DispatcherClientPort + _maxClients;
        private List<ClientInstance> _clients = new List<ClientInstance>();

        private Listener _DispatcherListener = new Listener(Config.DispatcherPort);
        private Listener _EventListener = new Listener(Config.EventPort);

        private Task _HeartbeatTask;
        private CancellationToken _HeartbeatCancellationToken;

        private DBWatcher _DBWatcher = new DBWatcher();
        private Settings _Settings = Settings.CreateInstance();
        //private SerialComHelper _SerialMonitor = new SerialComHelper();
        private TargetWatcher _TargetWatcher;
        

        public Dispatcher()
        {
            try
            {
                //Sets up dispatcher listener that will handle Client => Service requests.
                _DispatcherListener.SocketAccepted += _DispatcherListener_SocketAccepted;
                _DispatcherListener.Start();

                //Sets up Event Listener that will handle events from the Target Console => Service => All Clients.
                _EventListener.SocketAccepted += _EventListener_SocketAccepted;
                _EventListener.Start();

                //Starts up heartbeat task that will check for clients that have not gracefully shutdown and time them out.
                _HeartbeatTask = Task.Run(() => DoHeartbeat());

                //Helpers
                _DBWatcher.DBChanged += _DBWatcher_DBChanged;
                /*_SerialMonitor.NewSerialDataRecieved += _SerialMonitor_NewSerialDataRecieved;
                _SerialMonitor.Settings.PortName = "";
                _SerialMonitor.StartListening();*/
                _TargetWatcher = new TargetWatcher(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] Error: {ex.Message}");
            }
        }

        private byte[] _SerialDataBuffer = new byte[0];
        private void _SerialMonitor_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            _SerialDataBuffer = _SerialDataBuffer.Concat(e.Data).ToArray();

            //Buffer the data till we get a new line to prevent mangled output.
            if ((_SerialDataBuffer.Length > 0) && (_SerialDataBuffer[_SerialDataBuffer.Length - 1] == '\n'))
            {
                var Packet = new ForwardPacket(ForwardPacket.PacketType.SerialCom, "");
                Packet.SerialCom.Data = _SerialDataBuffer;
                ForwardPacketAll(Packet);

                //Clear buffer for next print.
                _SerialDataBuffer = new byte[0];
            }
        }

        private void _DBWatcher_DBChanged()
        {
            ForwardPacketAll(new ForwardPacket(ForwardPacket.PacketType.DBTouched, ""));
        }

        private void _EventListener_SocketAccepted(Socket e)
        {
            //This will be coming fromt the console which is c++ so we will have to marshal this when I figure out the packet structure.
            throw new NotImplementedException();
        }

        private void _DispatcherListener_SocketAccepted(Socket e)
        {
            try
            {
                var Packet = (DispatcherClientPacket)e.RecvObject();

                switch (Packet.Type)
                {
                    default:
                        Console.WriteLine("Invalid Packet...");
                        break;

                    case DispatcherClientPacket.PacketType.NewClient:
                        e.SendInt32(AddClient(Packet.ClientName));
                        break;

                    case DispatcherClientPacket.PacketType.RemoveClient:
                        RemoveClient(Packet.Port);
                        break;

                    case DispatcherClientPacket.PacketType.HeartBeat:
                        if (_clients.Any(p => p.Port == Packet.Port))
                        {
                            var Client = _clients.Where(p => p.Port == Packet.Port).First();
                            var CurrentTick = (Environment.TickCount & Int32.MaxValue);

                            Console.WriteLine($"[HeartBeat] in {(CurrentTick - Client.LastUpdate).ToString()}ms from {Packet.ClientName}({Packet.Port.ToString()})");

                            // Update last time we got a heart beat.
                            Client.LastUpdate = CurrentTick;

                            // Send Success.
                            e.SendInt32(1);
                        }
                        else // Send Failure.
                            e.SendInt32(0);


                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a client to the '_clients'.
        /// </summary>
        /// <param name="Name">The text name of the client. Should be the other programs name.</param>
        /// <returns>Returns the port the Dispatcher will send the data on.</returns>
        public int AddClient(string Name)
        {
            try
            {
                int Port = Config.DispatcherClientPort;
                while (Port < _clientMax)
                {
                    if (!_clients.Any(p => p.Port == Port))
                        break;

                    Port++;

                    if (Port == _clientMax)
                    {
                        Console.WriteLine("Client List Full.");

                        return -1;
                    }
                }

                Console.WriteLine($"New Client: {Name} {Port.ToString()}.");

                _clients.Add(new ClientInstance(Port, Name, Environment.TickCount & Int32.MaxValue));

                return Port;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return -1;
        }

        /// <summary>
        /// Removes the client specified from the '_clients'.
        /// </summary>
        /// <param name="Port">The port of the client you want to remove.</param>
        public void RemoveClient(int Port)
        {
            if (_clients.Any(p => p.Port == Port))
            {
                var Client = _clients.Where(p => p.Port == Port);
                _clients.Remove(Client.First());
            }
        }

        /// <summary>
        /// Forwards a packet to all the clients in the '_clients'.
        /// </summary>
        /// <param name="Packet">The packet data that should be sent.</param>
        public void ForwardPacketAll(ForwardPacket Packet)
        {
            try
            {
                foreach (ClientInstance Client in _clients)
                    Task.Factory.StartNew(() => Client.SendObject(Packet));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// This task will check for client time outs. This way it keeps the client list fresh and keeps from unneeded thread hold ups.
        /// </summary>
        /// <returns></returns>
        private async Task DoHeartbeat()
        {
            try
            {
                while (true)
                {
                    var CurrentTick = (Environment.TickCount & Int32.MaxValue);

                    foreach (var Client in _clients)
                    {
                        if (CurrentTick - Client.LastUpdate > 10000)
                        {
                            Console.WriteLine($"[Dispatcher] No response from client {Client.Name}({Client.Port.ToString()}) in more than 10000 ms. Last update was {(CurrentTick - Client.LastUpdate).ToString()} ms Timing Out!");

                            RemoveClient(Client.Port);
                        }
                    }

                    await Task.Delay(2000, _HeartbeatCancellationToken);

                    if (_HeartbeatCancellationToken.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }
        }
    }
}
