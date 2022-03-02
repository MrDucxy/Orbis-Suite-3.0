namespace OrbisSuite
{
    public class PrintEvent : EventArgs
    {
        public string Sender { get; private set; }
        public string Data { get; private set; }

        public PrintEvent(string Sender, string Data)
        {
            this.Sender = Sender;
            this.Data = Data;
        }
    }

    public class ProcInterceptEvent : EventArgs
    {
        public ProcInterceptEvent()
        {

        }
    }

    public class ProcContinueEvent : EventArgs
    {
        public ProcContinueEvent() { }
    }

    public class ProcDieEvent : EventArgs
    {
        public ProcDieEvent() { }
    }

    public class ProcAttachEvent : EventArgs
    {
        public string NewProcName { get; private set; }

        public ProcAttachEvent(string NewProcName)
        {
            this.NewProcName = NewProcName;
        }
    }

    public class ProcDetachEvent : EventArgs
    {
        public ProcDetachEvent() { }
    }

    public class TargetSuspendEvent : EventArgs
    {
        public TargetSuspendEvent() { }
    }

    public class TargetResumeEvent : EventArgs
    {
        public TargetResumeEvent() { }
    }

    public class TargetShutdownEvent : EventArgs
    {
        public TargetShutdownEvent() { }
    }

    public class TargetNewTitleEvent : EventArgs
    {
        public string TitleID { get; private set; }

        public TargetNewTitleEvent(string TitleID)
        {
            this.TitleID = TitleID;
        }
    }
}
