using NetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        private static unsafe partial int GetHostName(byte* name, int nameLength)
        {
            var host = Script.Write<string>("window.location.host");
            host.CopyTo(new Span<char>((void*)name, nameLength));
            return 0;
        }

    }
}
