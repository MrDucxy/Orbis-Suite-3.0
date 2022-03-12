using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbisSuite.Common.Database
{
    /// <summary>
    /// 
    /// </summary>
    [Table("Targets")]
    internal class TargetInfo2
    {
        private bool _IsDefault;
        private string? _TargetName;
        private string? _IPAddress;
        private int _Firmware;

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault
        {
            get { return _IsDefault; }
            set { _IsDefault = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string? TargetName
        {
            get { return _TargetName; }
            set { _TargetName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string? IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Firmware
        {
            get { return _Firmware; }
            set { _Firmware = value; }
        }

    }
}
