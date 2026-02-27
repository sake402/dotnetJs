using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
       
        internal static partial int Stat([MarshalUsing(typeof(SpanOfCharAsUtf8StringMarshaller))] ReadOnlySpan<char> path, out FileStatus output)
        {
            output = default(FileStatus);
            return -1;
        }

        internal static partial int LStat([MarshalUsing(typeof(SpanOfCharAsUtf8StringMarshaller))] ReadOnlySpan<char> path, out FileStatus output)
        {
            output = default(FileStatus);
            return -1;
        }

    }
}
