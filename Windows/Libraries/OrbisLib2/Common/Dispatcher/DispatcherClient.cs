using OrbisLib2.Common.Helpers;
using OrbisLib2.General;
using OrbisLib2.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TinyIpc.Messaging;

namespace OrbisLib2.Common.Dispatcher
{
    public class DispatcherClient
    {
        private static TinyMessageBus _ServiceMessageBus;

        /// <summary>
        /// Call this to subscribe for events from the windows service.
        /// </summary>
        public static void Subscribe()
        {
            _ServiceMessageBus = new TinyMessageBus("OrbisSuite");
            _ServiceMessageBus.MessageReceived += _ServiceMessageBus_MessageReceived;
        }

        private static void _ServiceMessageBus_MessageReceived(object? sender, TinyMessageReceivedEventArgs e)
        {
            var Packet = (ForwardPacket)Helper.ByteArrayToObject(e.Message.ToArray());

            switch (Packet.Type)
            {
                default:
                    Console.WriteLine("Invalid Packet...");
                    break;

                case ForwardPacket.PacketType.SerialCom:
                    // TODO:
                    break;

                case ForwardPacket.PacketType.Intercept:
                    Events.RaiseProcInterceptEvent(Packet.SenderIPAddress);
                    break;

                case ForwardPacket.PacketType.Continue:
                    Events.RaiseProcContinueEvent(Packet.SenderIPAddress);
                    break;

                // Process States
                case ForwardPacket.PacketType.ProcessDie:
                    Events.RaiseProcDieEvent(Packet.SenderIPAddress);
                    break;

                case ForwardPacket.PacketType.ProcessAttach:
                    Events.RaiseProcAttachEvent(Packet.SenderIPAddress, Packet.ProcessId);
                    break;

                case ForwardPacket.PacketType.ProcessDetach:
                    Events.RaiseProcDetachEvent(Packet.SenderIPAddress);
                    break;

                // Target State
                case ForwardPacket.PacketType.TargetSuspend:
                    Events.RaiseTargetSuspendEvent(Packet.SenderIPAddress);
                    break;

                case ForwardPacket.PacketType.TargetResume:
                    Events.RaiseTargetResumeEvent(Packet.SenderIPAddress);
                    break;

                case ForwardPacket.PacketType.TargetShutdown:
                    Events.RaiseTargetShutdownEvent(Packet.SenderIPAddress);
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
    }
}
