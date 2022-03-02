namespace OrbisSuite
{
    public class Events
    {
        /// <summary>
        /// The DBTouched Event gets invoked when the Database used to store target specific info is changed.
        /// </summary>
        public event EventHandler<DBTouchedEvent>? DBTouched;
        /// <summary>
        /// The TargetAvailable Event gets invoked when a target becomes available.
        /// </summary>
        public event EventHandler<TargetAvailableEvent>? TargetAvailable;
        /// <summary>
        /// The TargetUnAvailable Event gets invoked when a target becomes no longer available.
        /// </summary>
        public event EventHandler<TargetUnAvailableEvent>? TargetUnAvailable;
        /// <summary>
        /// The TargetAPIAvailable Event gets invoked when a target's API becomes available.
        /// </summary>
        public event EventHandler<TargetAPIAvailableEvent>? TargetAPIAvailable;
        /// <summary>
        /// The TargetAPIUnAvailable Event gets invoked when a target's API becomes no longer available.
        /// </summary>
        public event EventHandler<TargetAPIUnAvailableEvent>? TargetAPIUnAvailable;

        public Events()
        {

        }

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
                TargetAvailable?.Invoke(null, new TargetAvailableEvent(TargetName));
            else
                TargetUnAvailable?.Invoke(null, new TargetUnAvailableEvent(TargetName));
        }

        /// <summary>
        /// Will Fire the event for Target API Availability.
        /// </summary>
        /// <param name="Available"></param>
        /// <param name="TargetName"></param>
        internal void FireTargetAPIAvailability(bool Available, string TargetName)
        {
            if (Available)
                TargetAPIAvailable?.Invoke(null, new TargetAPIAvailableEvent(TargetName));
            else
                TargetAPIUnAvailable?.Invoke(null, new TargetAPIUnAvailableEvent(TargetName));
        }
    }
}
