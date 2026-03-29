using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {     
        internal static partial IntPtr LoadLibrary(string filename)
        {
            return IntPtr.Zero;
        }
   
        internal static partial IntPtr GetLoadLibraryError()
        {
            return IntPtr.Zero;
        }
        
        internal static partial IntPtr GetProcAddress(IntPtr handle, byte* symbol)
        {
            return IntPtr.Zero;
        }

        internal static partial IntPtr GetProcAddress(IntPtr handle, string symbol)
        {
            return IntPtr.Zero;
        }

        internal static partial void FreeLibrary(IntPtr handle)
        {
        }

        internal static partial IntPtr GetDefaultSearchOrderPseudoHandle()
        {
            return IntPtr.Zero;
        }

    }
}
