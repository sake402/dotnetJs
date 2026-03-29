using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Sys
    {
        //internal static partial SafeFileHandle Dup(SafeFileHandle oldfd)
        //{
        //    return oldfd;
        //}

        internal static unsafe partial int Write(SafeHandle fd, byte* buffer, int bufferSize)
        {
            return -1;
        }

        internal static unsafe partial int Write(IntPtr fd, byte* buffer, int bufferSize)
        {
            return -1;
        }

        internal static unsafe partial int WriteToNonblocking(SafeHandle fd, byte* buffer, int bufferSize)
        {
            return -1;
        }

        internal static partial Error ConvertErrorPlatformToPal(int platformErrno)
        {
            return (Error)platformErrno;
        }

        internal static partial int ConvertErrorPalToPlatform(Error error)
        {
            return (int)error;
        }

        private static unsafe partial byte* StrErrorR(int platformErrno, byte* buffer, int bufferSize)
        {
            return null;
        }
    }
}

