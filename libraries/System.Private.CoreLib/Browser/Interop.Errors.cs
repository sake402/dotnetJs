using dotnetJs;
using System;

internal static partial class Interop
{
    internal static unsafe partial class Sys
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
            RefOrPointer<byte> ptr = Script.Ref(buffer);
            "Error".TryCopyTo(new Span<char>((void*)buffer, bufferSize));
            return buffer;
        }
    }
}