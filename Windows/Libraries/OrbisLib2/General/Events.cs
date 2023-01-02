namespace OrbisLib2.General
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

    public class SelectedTargetChangedEvent : EventArgs
    {
        public string Name { get; private set; }

        public SelectedTargetChangedEvent(string Name) 
        {
            this.Name = Name;
        }
    }

    public class Events
    {
        /// <summary>
        /// The DBTouched Event gets invoked when the Database used to store target specific info is changed.
        /// </summary>
        public static event EventHandler<DBTouchedEvent>? DBTouched;

        /// <summary>
        /// Even is fired when ever the state of the target changes.
        /// </summary>
        public static event EventHandler<TargetStateChangedEvent>? TargetStateChanged;

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandler<SelectedTargetChangedEvent>? SelectedTargetChanged;

        /// <summary>
        /// Will Fire the event for when the Database has been updated.
        /// </summary>
        internal static void FireDBTouched()
        {
            DBTouched?.Invoke(null, new DBTouchedEvent());
        }

        /// <summary>
        /// Will Fire the event for Target Availability.
        /// </summary>
        /// <param name="Available"></param>
        /// <param name="TargetName"></param>
        internal static void FireTargetAvailability(bool Available, string TargetName)
        {
            if (Available)
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.Available));
            else
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.UnAvailable));
        }

        /// <summary>
        /// Will Fire the event for Target API Availability.
        /// </summary>
        /// <param name="Available"></param>
        /// <param name="TargetName"></param>
        internal static void FireTargetAPIAvailability(bool Available, string TargetName)
        {
            if (Available)
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.APIAvailable));
            else
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.APIUnAvailable));
        }

        /// <summary>
        /// Will fire the event whent the selected target changes.
        /// </summary>
        /// <param name="Name">The name of the target.</param>
        internal static void FireSelectedTargetChanged(string Name)
        {
            SelectedTargetChanged?.Invoke(null, new SelectedTargetChangedEvent(Name));
        }
    }
}
