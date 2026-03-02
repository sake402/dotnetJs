using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int MSync(IntPtr addr, ulong len, MemoryMappedSyncFlags flags)
        {
            throw new PlatformNotSupportedException();
        }

    }
}
