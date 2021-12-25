using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XiaowuStudioObfuscator
{
    public class AssemblyDetails
    {
        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CPUVersion _CPUVersion;

        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _HasStrongName;

        public CPUVersion CPUVersion
        {
            get;
            set;
        }

        public bool HasStrongName
        {
            get;
            set;
        }
    }
}
