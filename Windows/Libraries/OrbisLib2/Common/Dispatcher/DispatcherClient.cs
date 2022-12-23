using OrbisLib2.Common.Helpers;
using OrbisLib2.General;
using OrbisLib2.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OrbisLib2.Common.Dispatcher
{
    public class DispatcherClient
    {
        private Listener? _EventListener;
        private int _Port = -1;
        private Task _HeartbeatTask;
        private CancellationToken _HeartbeatCancellationToken;

        public DispatcherClient()
        {
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
                    case ForwardPacket.PacketType.Print:
                        TargetManager.SelectedTarget.Events.RaiseProcPrintEvent(Packet.SenderIPAddress, Packet.Print.Sender, Packet.Print.Data);
                        break;

                    case ForwardPacket.PacketType.SerialCom:
                        // TODO:
                        break;

                    case ForwardPacket.PacketType.Intercept:
                        TargetManager.SelectedTarget.Events.RaiseProcInterceptEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.Continue:
                        TargetManager.SelectedTarget.Events.RaiseProcContinueEvent(Packet.SenderIPAddress);
                        break;

                    // Process States
                    case ForwardPacket.PacketType.ProcessDie:
                        TargetManager.SelectedTarget.Events.RaiseProcDieEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.ProcessAttach:
                        TargetManager.SelectedTarget.Events.RaiseProcAttachEvent(Packet.SenderIPAddress, Packet.ProcessName);
                        break;

                    case ForwardPacket.PacketType.ProcessDetach:
                        TargetManager.SelectedTarget.Events.RaiseProcDetachEvent(Packet.SenderIPAddress);
                        break;

                    // Target State
                    case ForwardPacket.PacketType.TargetSuspend:
                        TargetManager.SelectedTarget.Events.RaiseTargetSuspendEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetResume:
                        TargetManager.SelectedTarget.Events.RaiseTargetResumeEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetShutdown:
                        TargetManager.SelectedTarget.Events.RaiseTargetShutdownEvent(Packet.SenderIPAddress);
                        break;

                    case ForwardPacket.PacketType.TargetNewTitle:
                        TargetManager.SelectedTarget.Events.RaiseTargetNewTitleEvent(Packet.SenderIPAddress, Packet.TitleChange.TitleID);
                        break;

                    case ForwardPacket.PacketType.TargetAvailability:
                        Events.FireTargetAvailability(Packet.TargetAvailability.Available, Packet.TargetAvailability.Name);
                        break;

                    case ForwardPacket.PacketType.TargetAPIAvailability:
                        Events.FireTargetAPIAvailability(Packet.TargetAvailability.Available, Packet.TargetAvailability.Name);
                        break;

                    // Misc
                    case ForwardPacket.PacketType.DBTouched:
                        Events.FireDBTouched();
                        break;
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
                        Console.WriteLine($"Starting up _EventListener on port {_Port}");
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
