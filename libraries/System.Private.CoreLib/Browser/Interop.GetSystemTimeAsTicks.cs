using dotnetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial long GetSystemTimeAsTicks()
        {
            int jsMilliseconds = Script.Write<int>("Date.now()");
            // The number of .NET ticks at the Unix epoch (1970-01-01 UTC)
            const long epochTicks = 621355968000000000;

            // Calculate the total number of .NET ticks for the current date
            long currentNetTicks = epochTicks + (jsMilliseconds * TimeSpan.TicksPerMillisecond);

            return currentNetTicks;
        }

    }
}
