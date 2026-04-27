using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        internal static unsafe partial byte** GetEnviron()
        {
            return null;
        }

        internal static unsafe partial void FreeEnviron(byte** environ)
        {
        }

    }
}
