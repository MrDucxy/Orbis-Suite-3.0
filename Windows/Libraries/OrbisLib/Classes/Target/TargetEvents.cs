namespace OrbisSuite
{
    public class TargetEvents
    {
        private Target Target;

        public event EventHandler<PrintEvent> Print;
        public event EventHandler<ProcInterceptEvent> ProcIntercept;
        public event EventHandler<ProcContinueEvent> ProcContinue;
        public event EventHandler<ProcDieEvent> ProcDie;
        public event EventHandler<ProcAttachEvent> ProcAttach;
        public event EventHandler<ProcDetachEvent> ProcDetach;
        public event EventHandler<TargetSuspendEvent> TargetSuspend;
        public event EventHandler<TargetResumeEvent> TargetResume;
        public event EventHandler<TargetShutdownEvent> TargetShutdown;
        public event EventHandler<TargetNewTitleEvent> TargetNewTitle;

        public TargetEvents(Target Target)
        {
            this.Target = Target;
        }

        /// <summary>
        /// Event that should be fired when we recieve a print from the remote target.
        /// </summary>
        /// <param name="SenderIPAddress">The sending Target Address.</param>
        /// <param name="Sender"></param>
        /// <param name="Data"></param>
        internal void RaiseProcPrintEvent(string SenderIPAddress, string Sender, string Data)
        {
            if (SenderIPAddress.Equals(Target.Info.IPAddress))
                Print?.Invoke(null, new PrintEvent(Sender, Data));
        }

        /// <summary>
        /// Fires an event for when the Target's proccess were attached to has reached an intercepted state.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseProcInterceptEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
            {
                ProcIntercept?.Invoke(null, new ProcInterceptEvent());
            }
        }

        /// <summary>
        /// Fires an event when a procees on the Target has gotten the signal to continue execution.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseProcContinueEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                ProcContinue?.Invoke(null, new ProcContinueEvent());
        }

        /// <summary>
        /// Fires an event for when a proccess is going to be shutting down on the Target.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseProcDieEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                ProcDie?.Invoke(null, new ProcDieEvent());
        }

        /// <summary>
        /// Fires an event for when the OrbisLib API attaches to a process.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        /// <param name="NewProcName">The name of the process were attaching to.</param>
        internal void RaiseProcAttachEvent(string IPAddr, string NewProcName)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                ProcAttach?.Invoke(null, new ProcAttachEvent(NewProcName));
        }

        /// <summary>
        /// Fires an event for when the OrbisLib API detaches to a process.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseProcDetachEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                ProcDetach?.Invoke(null, new ProcDetachEvent());
        }

        /// <summary>
        /// Fires event for when Target is entering the suspended state.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseTargetSuspendEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                TargetSuspend?.Invoke(null, new TargetSuspendEvent());
        }

        /// <summary>
        /// Fires event for when the Target is resuming from a suspended state.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseTargetResumeEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                TargetResume?.Invoke(null, new TargetResumeEvent());
        }

        /// <summary>
        /// Fires an event for when the Target is shutting down.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        internal void RaiseTargetShutdownEvent(string IPAddr)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                TargetShutdown?.Invoke(null, new TargetShutdownEvent());
        }

        /// <summary>
        /// Fires an event for when we change the current game on the Target.
        /// </summary>
        /// <param name="IPAddr">The sending Target Address.</param>
        /// <param name="TitleID">The next title index our Target will be running.</param>
        internal void RaiseTargetNewTitleEvent(string IPAddr, string TitleID)
        {
            if (IPAddr.Equals(Target.Info.IPAddress))
                TargetNewTitle?.Invoke(null, new TargetNewTitleEvent(TitleID));
        }
    }
}
