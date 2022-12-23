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
    }
}
