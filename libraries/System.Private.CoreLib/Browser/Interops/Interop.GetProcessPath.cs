using NetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static partial string? GetProcessPath()
        {
            return Script.Write<string>("window.location.host");
        }

    }
}
