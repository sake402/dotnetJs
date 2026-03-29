using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {

        internal static partial int MkDir([MarshalUsing(typeof(SpanOfCharAsUtf8StringMarshaller))] ReadOnlySpan<char> path, int mode)
        {
            return 0;
        }
    }
}
