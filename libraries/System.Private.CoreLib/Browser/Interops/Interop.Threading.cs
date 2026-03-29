using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {

        internal static unsafe partial bool CreateThread(IntPtr stackSize, delegate* unmanaged<IntPtr, IntPtr> startAddress, IntPtr parameter)
        {
            return false;
        }

        internal static unsafe partial uint TryGetUInt32OSThreadId()
        {
            return 1;
        }
    }
}
