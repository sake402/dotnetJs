using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {
        [NetJs.Template("0")]
        internal static extern partial int LChflags(string path, uint flags);
        //{
        //    return 0;
        //}

        [NetJs.Template("0")]
        internal static extern partial int FChflags(SafeHandle fd, uint flags);
        //{
        //    return 0;
        //}

        [NetJs.Template("-1")]
        private static extern partial int LChflagsCanSetHiddenFlag();
        //{
        //    return -1;
        //}

        [NetJs.Template("-1")]
        private static extern partial int CanGetHiddenFlag();
        //{
        //    return -1;
        //}
    }
}
