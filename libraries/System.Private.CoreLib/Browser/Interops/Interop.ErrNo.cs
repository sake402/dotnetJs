using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

internal static partial class Interop
{
    internal static unsafe partial class Sys
    {        

        static int errNo;
        internal static partial int GetErrNo()
        {
            return errNo;
        }

        internal static partial void SetErrNo(int errorCode)
        {
            errNo = errorCode;
        }

    }
}
