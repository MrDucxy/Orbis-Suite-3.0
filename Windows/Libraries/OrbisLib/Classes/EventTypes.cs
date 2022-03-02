namespace OrbisSuite
{
    public class DBTouchedEvent : EventArgs
    {
        public DBTouchedEvent() { }
    }

    public class TargetAvailableEvent : EventArgs
    {
        public string TargetName { get; private set; }

        public TargetAvailableEvent(string TargetName)
        {
            this.TargetName = TargetName;
        }
    }

    public class TargetUnAvailableEvent : EventArgs
    {
        public string TargetName { get; private set; }

        public TargetUnAvailableEvent(string TargetName)
        {
            this.TargetName = TargetName;
        }
    }

    public class TargetAPIAvailableEvent : EventArgs
    {
        public string TargetName { get; private set; }

        public TargetAPIAvailableEvent(string TargetName)
        {
            this.TargetName = TargetName;
        }
    }

    public class TargetAPIUnAvailableEvent : EventArgs
    {
        public string TargetName { get; private set; }

        public TargetAPIUnAvailableEvent(string TargetName)
        {
            this.TargetName = TargetName;
        }
    }
}
