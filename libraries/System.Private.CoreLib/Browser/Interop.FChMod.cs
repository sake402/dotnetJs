using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int FChMod(SafeFileHandle fd, int mode)
        {
            return 0;
        }

    }
}
