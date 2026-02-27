using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static unsafe partial long PReadV(SafeHandle fd, IOVector* vectors, int vectorCount, long fileOffset)
        {
            return -1;
        }
    }
}
