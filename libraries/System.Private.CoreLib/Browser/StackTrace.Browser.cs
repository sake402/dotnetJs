using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Diagnostics
{
    public partial class StackTrace
    {
        [NetJs.MemberReplace(nameof(GetTrace))]
        internal static void GetTraceImpl(ObjectHandleOnStack ex, ObjectHandleOnStack res, int skipFrames, bool needFileInfo)
        {
            res.GetObjectHandleOnStack<MonoStackFrame[]?>() = [];
        }
    }
}
