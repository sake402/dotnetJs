using dotnetJs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    [dotnetJs.ForcePartial(typeof(RuntimeMethodHandle))]
    public partial struct RuntimeMethodHandle_Partial
    {
        [dotnetJs.MemberReplace]
        private static IntPtr GetFunctionPointer(IntPtr m)
        {
            return m;
        }

        [dotnetJs.MemberReplace]
        private static void ReboxFromNullable(object? src, ObjectHandleOnStack res)
        {
            res.GetObjectHandleOnStack<object?>() = src;
        }

        [dotnetJs.MemberReplace]
        private static void ReboxToNullable(object? src, QCallTypeHandle destNullableType, ObjectHandleOnStack res)
        {
            res.GetObjectHandleOnStack<object?>() = src;
        }

    }
}
