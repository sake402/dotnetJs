using dotnetJs;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial void SysLog(SysLogPriority priority, string message, string arg1)
        {
            Script.Write("console.log(message + ':' + arg1)");
        }

    }
}
