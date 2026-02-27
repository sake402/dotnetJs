using dotnetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {       
        internal static unsafe partial void Log(byte* buffer, int count)
        {
            var str = Encoding.UTF8.GetString(buffer, count);
            Script.Write("console.log(str)");
        }

        internal static unsafe partial void LogError(byte* buffer, int count)
        {
            var str = Encoding.UTF8.GetString(buffer, count);
            Script.Write("console.error(str)");
        }

    }
}
