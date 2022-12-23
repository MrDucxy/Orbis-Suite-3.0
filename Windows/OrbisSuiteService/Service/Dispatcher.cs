using TinyIpc.Messaging;
using OrbisLib2.Common.Database;
using OrbisLib2.Common.Dispatcher;
using OrbisLib2.Common.Helpers;

namespace OrbisSuiteService.Service
{
    public class Dispatcher
    {
        private TinyMessageBus _ServiceMessageBus;

        private DBWatcher _DBWatcher = new DBWatcher();
        private Settings _Settings = Settings.CreateInstance();
        //private SerialComHelper _SerialMonitor = new SerialComHelper();
        private TargetWatcher _TargetWatcher;
        

        public Dispatcher()
        {
            _ServiceMessageBus = new TinyMessageBus("OrbisSuite");

            //Helpers
            _DBWatcher.DBChanged += _DBWatcher_DBChanged;
            /*_SerialMonitor.NewSerialDataRecieved += _SerialMonitor_NewSerialDataRecieved;
            _SerialMonitor.Settings.PortName = "";
            _SerialMonitor.StartListening();*/
            _TargetWatcher = new TargetWatcher(this);
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
            _ServiceMessageBus.PublishAsync(Helper.ObjectToByteArray(Packet));
        }
    }
}
