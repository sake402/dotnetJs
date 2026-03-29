using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        public static partial string GetUnixRelease()
        {
            return "10.0";
        }

    }
}
