using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int LockFileRegion(SafeHandle fd, long offset, long length, LockType lockType)
        {
            return 0;
        }

    }
}
