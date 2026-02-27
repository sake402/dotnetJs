using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public static partial class ThreadPool
    {
        [dotnetJs.MemberReplace(nameof(MainThreadScheduleBackgroundJob))]
        internal static unsafe void MainThreadScheduleBackgroundJobImpl(void* callback)
        {
            var call = *(Action*)callback;
            Global.SetTimeout(call, 1);
        }
    }
}
