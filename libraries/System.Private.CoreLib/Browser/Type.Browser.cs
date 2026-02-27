using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public abstract partial class Type
    {
        [dotnetJs.MemberReplace(nameof(internal_from_handle))]
        private static Type internal_from_handleImpl(IntPtr handle)
        {
            return RuntimeType.GetTypeFromHandle(new ReflectionHandleModel { Value = (uint)handle }) ?? throw new InvalidOperationException("Invalid handle");
        }
    }
}
