namespace OrbisSuite
{
    public class DBTouchedEvent : EventArgs
    {
        public DBTouchedEvent() { }
    }

    public class TargetStateChangedEvent : EventArgs
    {
        public enum TargetState
        {
            None,
            Available,
            UnAvailable,
            APIAvailable,
            APIUnAvailable,
        };

        public TargetState State { get; private set; }

        public string Name { get; private set; }

        public TargetStateChangedEvent(string Name, TargetState State)
        {
            this.Name = Name;
            this.State = State;
        }
    }
}
