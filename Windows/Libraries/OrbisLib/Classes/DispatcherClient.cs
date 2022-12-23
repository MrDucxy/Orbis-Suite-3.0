using OrbisSuite.Common;
using OrbisSuite.Common.Dispatcher;
using System.Net.Sockets;
using System.Text;
using TinyIpc.Messaging;

namespace OrbisSuite
{
    public class DispatcherClient
    {
        private OrbisLib PS4;
        private TinyMessageBus _ServiceMessageBus;

        public DispatcherClient(OrbisLib PS4)
        {
            this.PS4 = PS4;

            _ServiceMessageBus = new TinyMessageBus("OrbisSuite");
            _ServiceMessageBus.MessageReceived += _ServiceMessageBus_MessageReceived; ;
        }

        private void _ServiceMessageBus_MessageReceived(object? sender, TinyMessageReceivedEventArgs e)
        {
            var Packet = (ForwardPacket)Helpers.ByteArrayToObject(e.Message.ToArray());

            switch (Packet.Type)
            {
                default:
                    Console.WriteLine("Invalid Packet...");
                    break;

                // Debugging
                case ForwardPacket.PacketType.Print:
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
                    break;
            }
        }
    }
}