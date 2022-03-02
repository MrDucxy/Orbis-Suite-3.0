using OrbisSuite.Common;
using OrbisSuite.Common.Dispatcher;
using System.Net.Sockets;

namespace OrbisSuite
{
    public class DispatcherClient
    {
        private OrbisLib PS4;
        private Listener? _EventListener;
        private int _Port = -1;
        private Task _HeartbeatTask;
        private CancellationToken _HeartbeatCancellationToken;

        public DispatcherClient(OrbisLib PS4)
        {
            this.PS4 = PS4;

            _HeartbeatTask = Task.Run(() => DoHeartbeat());
        }

        private void _EventListener_SocketAccepted(Socket e)
        {
            try
            {
                var Packet = (ForwardPacket)e.RecvObject();

                switch (Packet.Type)
                {
                    default:
                        Console.WriteLine("Invalid Packet...");
                        break;

                    // Debugging
                    /*case ForwardPacket.PacketType.Print:
                        PS4.DefaultTarget.Events.RaiseProcPrintEvent(Packet.SenderIPAddress, Packet.Print.Sender, Packet.Print.Data);
                        PS4.SelectedTarget.Events.RaiseProcPrintEvent(Packet.SenderIPAddress, Packet.Print.Sender, Packet.Print.Data);
                        break;

                    case ForwardPacket.PacketType.SerialCom:
                        Console.Write(Encoding.UTF8.GetString(Packet.SerialCom.Data));
                        break;

                    case ForwardPacket.PacketType.Intercept:
                        PS4.DefaultTarget.Events.RaiseProcInterceptEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseProcInterceptEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.Continue:
                        PS4.DefaultTarget.Events.RaiseProcContinueEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseProcContinueEvent(Packet.SenderIPAddress);
                        break;

                    // Process States
                    case ForwardPacket.PacketType.ProcessDie:
                        PS4.DefaultTarget.Events.RaiseProcDieEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseProcDieEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.ProcessAttach:
                        PS4.DefaultTarget.Events.RaiseProcAttachEvent(Packet.SenderIPAddress, Packet.ProcessName);
                        PS4.SelectedTarget.Events.RaiseProcAttachEvent(Packet.SenderIPAddress, Packet.ProcessName);
                        break;

                    case ForwardPacket.PacketType.ProcessDetach:
                        PS4.DefaultTarget.Events.RaiseProcDetachEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseProcDetachEvent(Packet.SenderIPAddress);
                        break;

                    // Target State
                    case ForwardPacket.PacketType.TargetSuspend:
                        PS4.DefaultTarget.Events.RaiseTargetSuspendEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseTargetSuspendEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetResume:
                        PS4.DefaultTarget.Events.RaiseTargetResumeEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseTargetResumeEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetShutdown:
                        PS4.DefaultTarget.Events.RaiseTargetShutdownEvent(Packet.SenderIPAddress);
                        PS4.SelectedTarget.Events.RaiseTargetShutdownEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetNewTitle:
                        PS4.DefaultTarget.Events.RaiseTargetNewTitleEvent(Packet.SenderIPAddress, Packet.TitleChange.TitleID);
                        PS4.SelectedTarget.Events.RaiseTargetNewTitleEvent(Packet.SenderIPAddress, Packet.TitleChange.TitleID);
                        break;

                    case ForwardPacket.PacketType.TargetAvailability:
                        PS4.Events.FireTargetAvailability(Packet.TargetAvailability.Available, Packet.TargetAvailability.Name);
                        break;

                    case ForwardPacket.PacketType.TargetAPIAvailability:
                        PS4.Events.FireTargetAPIAvailability(Packet.TargetAvailability.Available, Packet.TargetAvailability.Name);
                        break;

                    // Misc
                    case ForwardPacket.PacketType.DBTouched:
                        PS4.Events.FireDBTouched();
                        break;*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to connect to the Dispatcher.
        /// </summary>
        /// <returns>Returns true if the connection was successful.</returns>
        private bool Connect()
        {
            try
            {
                if (_Port != -1)
                {
                    Console.WriteLine("[Connect] Already Connected!");

                    return true;
                }

                var Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (Sock.EasyConnect("127.0.0.1", Config.DispatcherPort, 1000))
                {
                    Sock.SendObject(new DispatcherClientPacket(DispatcherClientPacket.PacketType.NewClient, System.Diagnostics.Process.GetCurrentProcess().ProcessName, -1));

                    _Port = Sock.RecvInt32();
                    Sock.Close();

                    if (_Port != -1)
                    {
                        Console.WriteLine($"Starting up _EventListener on port { _Port }");
                        _EventListener = new Listener(_Port);
                        _EventListener.Start();
                        _EventListener.SocketAccepted += _EventListener_SocketAccepted;

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Sends a Heart beat Packet.
        /// </summary>
        /// <returns>Returns true if the packet was sent successfully.</returns>
        private bool DoHeartBeat()
        {
            try
            {
                if (_Port == -1)
                {
                    Console.WriteLine("[DoHeartBeat] Not Connected!");

                    return false;
                }

                var Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (Sock.EasyConnect("127.0.0.1", Config.DispatcherPort, 1000))
                {
                    Sock.SendObject(new DispatcherClientPacket(DispatcherClientPacket.PacketType.HeartBeat, System.Diagnostics.Process.GetCurrentProcess().ProcessName, _Port));

                    var Result = Sock.RecvInt32();
                    Sock.Close();

                    return (Result == 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Task will Send heartbeat packets to the Service. This will also ensure that we are always connected to the service.
        /// </summary>
        /// <returns></returns>
        private async Task DoHeartbeat()
        {
            while (!Connect())
            {
                await Task.Delay(500, _HeartbeatCancellationToken);

                if (_HeartbeatCancellationToken.IsCancellationRequested)
                    break;
            }

            if (_Port != -1)
                Console.WriteLine("[DispatcherClient] Connection Sucessful!");

            while (true)
            {
                await Task.Delay(5000, _HeartbeatCancellationToken);

                if (_HeartbeatCancellationToken.IsCancellationRequested)
                    break;

                if (!DoHeartBeat())
                {
                    _Port = -1;

                    while (!Connect())
                    {
                        await Task.Delay(500, _HeartbeatCancellationToken);

                        if (_HeartbeatCancellationToken.IsCancellationRequested)
                            break;
                    }

                    if (_Port != -1)
                        Console.WriteLine("[DispatcherClient] Reconnection Sucessful!");
                }
            }
        }
    }
}