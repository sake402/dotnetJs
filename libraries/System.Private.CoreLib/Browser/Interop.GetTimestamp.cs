using dotnetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial long GetTimestamp()
        {
            double time = Script.Write<double>("performance.now()");
            return (long)time;
        }

    }
}
