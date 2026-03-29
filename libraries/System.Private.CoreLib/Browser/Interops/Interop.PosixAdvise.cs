using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int PosixFAdvise(SafeFileHandle fd, long offset, long length, FileAdvice advice)
        {
            return 0;
        }
    }
}
