using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        private static partial int ReadLink([MarshalUsing(typeof(SpanOfCharAsUtf8StringMarshaller))] ReadOnlySpan<char> path, ref byte buffer, int bufferSize)
        {
            return -1;
        }

    }
}
