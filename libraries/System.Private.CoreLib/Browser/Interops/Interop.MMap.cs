using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial IntPtr MMap(
            IntPtr addr, ulong len,
            MemoryMappedProtections prot, MemoryMappedFlags flags,
            SafeFileHandle fd, long offset)
        {
            return IntPtr.Zero;
        }

        internal static partial IntPtr MMap(
            IntPtr addr, ulong len,
            MemoryMappedProtections prot, MemoryMappedFlags flags,
            IntPtr fd, long offset)
        {
            return IntPtr.Zero;
        }
    }
}
