using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrbisSuiteService
{
    public partial class OrbisSuiteService : ServiceBase
    {
        bool RunService = true;

        public OrbisSuiteService()
        {
            InitializeComponent();
        }

        public void DebugStart()
        {
            OnStart(null);
            while (RunService) { Thread.Sleep(10); }
        }

        protected override void OnStart(string[] args)
        {
            RunService = true;
        }

        protected override void OnStop()
        {
            RunService = false;

        }
    }
}
