namespace OrbisSuite
{
    public class Events
    {
        /// <summary>
        /// The DBTouched Event gets invoked when the Database used to store target specific info is changed.
        /// </summary>
        public event EventHandler<DBTouchedEvent>? DBTouched;

        /// <summary>
        /// Even is fired when ever the state of the target changes.
        /// </summary>
        public event EventHandler<TargetStateChangedEvent>? TargetStateChanged;

        /// <summary>
        /// Will Fire the event for when the Database has been updated.
        /// </summary>
        internal void FireDBTouched()
        {
            DBTouched?.Invoke(null, new DBTouchedEvent());
        }

        /// <summary>
        /// Will Fire the event for Target Availability.
        /// </summary>
        /// <param name="Available"></param>
        /// <param name="TargetName"></param>
        internal void FireTargetAvailability(bool Available, string TargetName)
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
        internal void FireTargetAPIAvailability(bool Available, string TargetName)
        {
            if (Available)
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.APIAvailable));
            else
                TargetStateChanged?.Invoke(null, new TargetStateChangedEvent(TargetName, TargetStateChangedEvent.TargetState.APIUnAvailable));
        }
    }
}
