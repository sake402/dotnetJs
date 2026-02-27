using dotnetJs;
using Mono;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection
{
    public partial class AssemblyName
    {
        [dotnetJs.MemberReplace(nameof(FreeAssemblyName))]
        internal static void FreeAssemblyNameImpl(ref MonoAssemblyName name, bool freeStruct)
        {
            Marshal.Remove(name.name);
        }

        [dotnetJs.MemberReplace(nameof(GetNativeName))]
        private static unsafe MonoAssemblyName* GetNativeNameImpl(IntPtr assemblyPtr)
        {
            var assembly = AppDomain.GetAssembly(new ReflectionHandleModel { Value = (uint)assemblyPtr });
            MonoAssemblyName name = new MonoAssemblyName();
            var model = assembly.As<RuntimeAssembly_Partial>()._model;
            name.major = 1;
            name.minor = 0;
            name.build = -1;
            name.name = Marshal.MarshalObject(model.FullName);
            return &name;
        }
    }
}
