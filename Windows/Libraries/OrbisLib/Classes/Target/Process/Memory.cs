namespace OrbisSuite
{
    public class Memory
    {
        private Target Target;
        private Process Process;
        public Extension Ext;

        public Memory(Target Target, Process Process)
        {
            this.Target = Target;
            this.Process = Process;

            Ext = new Extension(Target, this);
        }
    }
}
