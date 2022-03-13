using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// The current status of the Target & the Orbis Suite API.
    /// </summary>
    public enum TargetStatusType
    {
        None,
        Offline,
        Online,
        APIAvailable
    };
}
