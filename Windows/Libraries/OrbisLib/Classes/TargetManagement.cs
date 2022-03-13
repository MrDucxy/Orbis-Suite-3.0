﻿using OrbisSuite.Common.Database;

namespace OrbisSuite
{
    public class TargetManagement
    {
        OrbisLib PS4;
        public TargetManagement(OrbisLib PS4)
        {
            this.PS4 = PS4;
        }

        public TargetInfo? DefaultTarget
        {
            get
            {
                return SavedTargets.DefaultTarget;
            }
        }

        public List<TargetInfo>? TargetList
        {
            get
            {
                return SavedTargets.Targets;
            }
        }

        public bool DoesDefaultTargetExist()
        {
            return (DefaultTarget != null);
        }

        public bool GetTarget(string TargetName, out TargetInfo Out)
        {
            Out = TargetInfo.FindTarget(x => x.Name == TargetName);
            return (Out != null);
        }

        public bool SetTarget(string TargetName, bool Default, string NewTargetName, string IPAddress, int Firmware, int PayloadPort)
        {
            var Target = TargetInfo.FindTarget(x => x.Name == TargetName);

            Target.IsDefault = Default;
            Target.Name = NewTargetName;
            Target.IPAddress = IPAddress;
            Target.Firmware = Firmware;
            Target.PayloadPort = PayloadPort;

            return Target.Save();
        }

        public bool DeleteTarget(string TargetName)
        {
            var Target = TargetInfo.FindTarget(x => x.Name == TargetName);
            return Target.Remove();
        }

        public bool NewTarget(bool Default, string TargetName, string IPAddress, int Firmware, int PayloadPort)
        {
            return new TargetInfo { IsDefault = Default, Name = TargetName, IPAddress = IPAddress, Firmware = Firmware, PayloadPort = PayloadPort }.Add();
        }

        public int GetTargetCount()
        {
            return SavedTargets.TargetCount;
        }

        public void SetSelected(string TargetName)
        {
            TargetInfo Info;
            if (GetTarget(TargetName, out Info))
                PS4.SelectedTarget.Info = Info;
        }
    }
}
