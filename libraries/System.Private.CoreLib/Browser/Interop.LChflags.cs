using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial int LChflags(string path, uint flags)
        {
            return 0;
        }

        internal static partial int FChflags(SafeHandle fd, uint flags)
        {
            return 0;
        }

        private static partial int LChflagsCanSetHiddenFlag()
        {
            return -1;
        }

        private static partial int CanGetHiddenFlag()
        {
            return -1;
        }
    }
}
