using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        
        internal static partial int FStat(SafeHandle fd, out FileStatus output)
        {
            output = default(FileStatus);
            return -1;
        }

        internal static partial int Stat(string path, out FileStatus output)
        {
            output = default(FileStatus);
            return -1;
        }

        internal static partial int LStat(string path, out FileStatus output)
        {
            output = default(FileStatus);
            return -1;
        }

    }
}
