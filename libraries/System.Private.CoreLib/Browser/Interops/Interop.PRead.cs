using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static unsafe partial int PRead(SafeHandle fd, byte* buffer, int bufferSize, long fileOffset)
        {
            return -1;
        }
    }
}
