using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial int MUnmap(IntPtr addr, ulong len)
        {
            return 0;
        }
    }
}
