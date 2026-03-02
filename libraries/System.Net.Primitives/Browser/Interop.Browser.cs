using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Sys
    {
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

        internal static unsafe partial Error GetSocketAddressSizes(int* ipv4SocketAddressSize, int* ipv6SocketAddressSize, int* udsSocketAddressSize, int* maxSocketAddressSize)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error GetAddressFamily(byte* socketAddress, int socketAddressLen, int* addressFamily)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error SetAddressFamily(byte* socketAddress, int socketAddressLen, int addressFamily)
        {
            return Error.ENOTSUP;
        }


        internal static unsafe partial Error GetPort(byte* socketAddress, int socketAddressLen, ushort* port)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error SetPort(byte* socketAddress, int socketAddressLen, ushort port)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error GetIPv4Address(byte* socketAddress, int socketAddressLen, uint* address)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error SetIPv4Address(byte* socketAddress, int socketAddressLen, uint address)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error GetIPv6Address(byte* socketAddress, int socketAddressLen, byte* address, int addressLen, uint* scopeId)
        {
            return Error.ENOTSUP;
        }

        internal static unsafe partial Error SetIPv6Address(byte* socketAddress, int socketAddressLen, byte* address, int addressLen, uint scopeId)
        {
            return Error.ENOTSUP;
        }

    }
}
