﻿using OrbisLib2.Common.Database;
using OrbisLib2.Common.Dispatcher;
using OrbisLib2.Common.Helpers;
using OrbisLib2.Targets;

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
                if(TargetManager.Targets.Count <= 0)
                {
                    await Task.Delay(1000, _TargetWatcherCancellationToken);
                }

                Parallel.ForEach(SavedTarget.GetTargetList(), Target =>
                {
                    var oldAvailable = Target.Info.IsAvailable;
                    var OldAPIAvailable = Target.Info.IsAPIAvailable;

                    if(Sockets.PingHost(Target.IPAddress))
                    {
                        var detail = Target.Info;
                        detail.IsAvailable = true;
                    }

                    Target.Info.IsAvailable = Sockets.PingHost(Target.IPAddress);
                    Target.Info.IsAPIAvailable = Sockets.TestTcpConnection(Target.IPAddress, Settings.CreateInstance().APIPort);
                    Target.Info.Save();

                    if (Target.Info.IsAPIAvailable)
                        TargetManager.UpdateTargetInfo(Target);
                    else
                    {
                        Target.Info.CPUTemp = 0;
                        Target.Info.SOCTemp = 0;
                        Target.Info.ThreadCount = 0;
                        Target.Info.AverageCPUUsage = 0;
                        Target.Info.BusyCore = 0;
                        Target.Info.RamUsage = 0;
                        Target.Info.VRamUsage = 0;
                        Target.Info.BigAppTitleID = "-";
                        Target.Info.Save();
                    }

                    // Forward Target Availability.
                    if (oldAvailable != Target.Info.IsAvailable)
                    {
                        var Packet = new ForwardPacket(ForwardPacket.PacketType.TargetAvailability, Target.IPAddress);
                        Packet.TargetAvailability.Available = Target.Info.IsAvailable;
                        Packet.TargetAvailability.Name = Target.Name;
                        _dispatcher.PublishEvent(Packet);
                    }

                    // Forward API Availability.
                    if (OldAPIAvailable != Target.Info.IsAPIAvailable)
                    {
                        var Packet = new ForwardPacket(ForwardPacket.PacketType.TargetAPIAvailability, Target.IPAddress);
                        Packet.TargetAPIAvailability.Available = Target.Info.IsAvailable;
                        Packet.TargetAPIAvailability.Name = Target.Name;
                        _dispatcher.PublishEvent(Packet);
                    }
                });

                await Task.Delay(1000, _TargetWatcherCancellationToken);

                if (_TargetWatcherCancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}
