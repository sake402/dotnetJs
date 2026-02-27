using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial int Close(IntPtr fd)
        {
            return 0;
        }
    }
}
