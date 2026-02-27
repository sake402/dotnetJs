using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.InteropServices
{
    public partial struct GCHandle
    {
        [dotnetJs.MemberReplace(nameof(InternalAlloc))]
        internal static IntPtr InternalAllocImpl(object? value, GCHandleType type)
        {
            return Marshal.MarshalObject(value);
        }

        [dotnetJs.MemberReplace(nameof(InternalFree))]
        internal static void InternalFreeImpl(IntPtr handle)
        {
            Marshal.Remove(handle);
        }

        [dotnetJs.MemberReplace(nameof(InternalGet))]
        internal static object? InternalGetImpl(IntPtr handle)
        {
            return Marshal.MarshalObject(handle);
        }

        [dotnetJs.MemberReplace(nameof(InternalSet))]
        internal static void InternalSetImpl(IntPtr handle, object? value)
        {
            Marshal.MarshalObject(value, handle);
        }

    }
}
