using OrbisSuite.Common.Database;

namespace OrbisSuite
{
    public class TargetManagement
    {
        OrbisLib PS4;
        public TargetManagement(OrbisLib PS4)
        {
            this.PS4 = PS4;
        }

        public TargetInfo DefaultTarget
        {
            get
            {
                return SavedTargets.DefaultTarget;
            }
        }

        public List<TargetInfo> TargetList
        {
            get
            {
                return SavedTargets.Targets;
            }
        }

        public bool DoesDefaultTargetExist()
        {
            return SavedTargets.DoesDefaultTargetExist();
        }

        public bool DoesTargetExist(string TargetName)
        {
            return SavedTargets.DoesTargetExist(TargetName);
        }

        public bool DoesTargetExistIP(string IPAddr)
        {
            return SavedTargets.DoesTargetExistIP(IPAddr);
        }

        public bool GetTarget(string TargetName, out TargetInfo Out)
        {
            return SavedTargets.GetTarget(TargetName, out Out);
        }

        public bool SetTarget(string TargetName, bool Default, string NewTargetName, string IPAddr, int Firmware, int PayloadPort)
        {
            return SavedTargets.SetTarget(TargetName, new TargetInfo(Default, NewTargetName, IPAddr, Firmware, PayloadPort));
        }

        public bool DeleteTarget(string TargetName)
        {
            return SavedTargets.DeleteTarget(TargetName);
        }

        public bool NewTarget(bool Default, string TargetName, string IPAddr, int Firmware, int PayloadPort)
        {
            return SavedTargets.NewTarget(Default, TargetName, IPAddr, Firmware, PayloadPort);
        }

        public int GetTargetCount()
        {
            return SavedTargets.GetTargetCount();
        }

        public void SetDefault(string TargetName)
        {
            SavedTargets.SetDefaultTarget(TargetName);
        }

        public void SetSelected(string TargetName)
        {
            TargetInfo Info;
            if (GetTarget(TargetName, out Info))
                PS4.SelectedTarget.Info = Info;
        }
    }
}
