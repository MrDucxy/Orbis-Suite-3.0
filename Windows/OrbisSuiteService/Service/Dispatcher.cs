using OrbisLib2.Common.Dispatcher;
using Microsoft.Extensions.Logging;
using H.Pipes;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using H.Pipes.AccessControl;

namespace OrbisSuiteService.Service
{
    public class Dispatcher
    {
        private IPipeServer<ForwardPacket> _PipeServer;
        private ILogger _Logger;

        private DBWatcher _DBWatcher = new DBWatcher();
        //private SerialComHelper _SerialMonitor = new SerialComHelper();
        private TargetWatcher _TargetWatcher;
        private TargetEventListener _TargetEventListener;


        public Dispatcher(ILogger logger)
        {
            _Logger = logger;

            // Set up the named pipe server.
            _PipeServer = new PipeServer<ForwardPacket>("OrbisSuite");

            // Set the pipe security so userland can interact with us.
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            _PipeServer.SetPipeSecurity(pipeSecurity);

            // start the pipe server.
            _PipeServer.StartAsync();

            //Helpers
            _DBWatcher.DBChanged += _DBWatcher_DBChanged;
            /*_SerialMonitor.NewSerialDataRecieved += _SerialMonitor_NewSerialDataRecieved;
            _SerialMonitor.Settings.PortName = "";
            _SerialMonitor.StartListening();*/
            _TargetWatcher = new TargetWatcher(this);
            _TargetEventListener = new TargetEventListener(this, _Logger);
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
                PublishEvent(Packet);

                //Clear buffer for next print.
                _SerialDataBuffer = new byte[0];
            }
        }

        private void _DBWatcher_DBChanged()
        {
            PublishEvent(new ForwardPacket(ForwardPacket.PacketType.DBTouched, ""));
        }

        public void PublishEvent(ForwardPacket Packet)
        {
            _Logger.LogInformation($"Publishing Event: {Packet.Type}.");
            _PipeServer.WriteAsync(Packet);
        }
    }
}
