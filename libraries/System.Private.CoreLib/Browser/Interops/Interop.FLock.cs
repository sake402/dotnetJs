using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int FLock(SafeFileHandle fd, LockOperations operation)
        {
            return 0;
        }

        internal static partial int FLock(IntPtr fd, LockOperations operation)
        {
            return 0;
        }


    }
}
