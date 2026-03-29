using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial IntPtr GetTimeZoneData(string fileName, out int length)
        {
            length = 0;
            return IntPtr.Zero;
        }
    }
}
