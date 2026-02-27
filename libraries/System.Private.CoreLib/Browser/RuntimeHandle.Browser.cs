using System;
using System.Collections.Generic;
using System.Text;

namespace Mono
{
    [dotnetJs.ForcePartial(typeof(RuntimeClassHandle))]
    internal unsafe partial struct RuntimeClassHandle_Partial
    {
        [dotnetJs.MemberReplace]
        internal static unsafe IntPtr GetTypeFromClass(RuntimeStructs.MonoClass* klass)
        {
            return System.Runtime.InteropServices.Marshal.MarshalObject(klass);
        }

    }
}
