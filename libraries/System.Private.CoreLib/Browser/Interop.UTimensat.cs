using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static unsafe partial int UTimensat(string path, TimeSpec* times)
        {
            return -1;
        }

        internal static unsafe partial int FUTimens(SafeHandle fd, TimeSpec* times)
        {
            return -1;
        }

    }
}
