using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial SafeFileHandle ShmOpen(string name, OpenFlags flags, int mode)
        {
            throw new PlatformNotSupportedException();
        }

        internal static partial int ShmUnlink(string name)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
