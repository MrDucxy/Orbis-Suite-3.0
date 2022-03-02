using OrbisSuite.Common;
using OrbisSuite.Common.DataBase;
using OrbisSuite.Common.Dispatcher;

namespace OrbisSuiteService.Service
{
    public class TargetWatcher
    {
        private Dispatcher _dispatcher;
        private Task _TargetWatcherTask;
        private CancellationToken _TargetWatcherCancellationToken;

        public TargetWatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _TargetWatcherTask = Task.Run(() => DoTargetWatcher());
        }

        private async Task DoTargetWatcher()
        {
            while (true)
            {
                Parallel.ForEach(SavedTargets.Targets, Target =>
                {
                    var oldAvailable = Target.Available;
                    var OldAPIAvailable = Target.APIAvailable;

                    Target.Available = Helpers.PingHost(Target.IPAddr);
                    Target.APIAvailable = Helpers.TestTcpConnection(Target.IPAddr, Settings.APIPort);
                    SavedTargets.SetTargetExt(Target.Name, Target);

                    if (Target.APIAvailable)
                        SavedTargets.UpdateTargetExtInfo(Target.Name);

                    // Forward Target Availability.
                    if (oldAvailable != Target.Available)
                    {
                        var Packet = new ForwardPacket(ForwardPacket.PacketType.TargetAvailability, Target.IPAddr);
                        Packet.TargetAvailability.Available = Target.Available;
                        Packet.TargetAvailability.Name = Target.Name;
                        _dispatcher.ForwardPacketAll(Packet);
                    }

                    // Forward API Availability.
                    if (OldAPIAvailable != Target.APIAvailable)
                    {
                        var Packet = new ForwardPacket(ForwardPacket.PacketType.TargetAPIAvailability, Target.IPAddr);
                        Packet.TargetAPIAvailability.Available = Target.Available;
                        Packet.TargetAPIAvailability.Name = Target.Name;
                        _dispatcher.ForwardPacketAll(Packet);
                    }
                });

                await Task.Delay(1000, _TargetWatcherCancellationToken);

                if (_TargetWatcherCancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}
