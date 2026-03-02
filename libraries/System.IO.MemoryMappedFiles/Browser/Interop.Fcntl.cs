using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial class Fcntl
        {
            internal static partial int DangerousSetIsNonBlocking(IntPtr fd, int isNonBlocking)
            {
                throw new PlatformNotSupportedException();
            }

            internal static partial int SetIsNonBlocking(SafeHandle fd, int isNonBlocking)
            {
                throw new PlatformNotSupportedException();
            }

            internal static partial int GetIsNonBlocking(SafeHandle fd, out bool isNonBlocking)
            {
                throw new PlatformNotSupportedException();
            }

            internal static partial int SetFD(SafeHandle fd, int flags)
            {
                throw new PlatformNotSupportedException();
            }

            internal static partial int GetFD(SafeHandle fd)
            {
                throw new PlatformNotSupportedException();
            }

            internal static partial int GetFD(IntPtr fd)
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}

