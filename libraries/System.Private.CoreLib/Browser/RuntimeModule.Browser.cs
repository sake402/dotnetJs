using dotnetJs;
using System.Runtime.Intrinsics;

namespace System.Reflection
{
    //Our simplified module implementation is on a per assembly basis
    //So a module handle is the same as an assembly handle
    //Most if the implemetations here are simply forwarded to the assembly
    [dotnetJs.ForcePartial(typeof(RuntimeModule))]
    [dotnetJs.Name(nameof(RuntimeModule))]
    [dotnetJs.Boot]
    [dotnetJs.Reflectable(false)]
    [dotnetJs.OutputOrder(int.MinValue + 1)] //make sure we emit this type immediately after AppDomain
    internal sealed partial class RuntimeModule_Partial : ForcedPartialBase<RuntimeModule>
    {
        public RuntimeModule_Partial(RuntimeAssembly_Partial assembly)
        {
            //_assembly = assembly;
            This._impl = (nint)assembly._model.Handle.Value;
            This.assembly = assembly.As<Assembly>();
        }

        [dotnetJs.MemberReplace]
        internal static int get_MetadataToken(Module module)
        {
            return (int)module.As<RuntimeModule>().assembly.As<RuntimeAssembly_Partial>()._model.Handle.Value;
        }

        [dotnetJs.MemberReplace]
        internal static int GetMDStreamVersion(IntPtr module)
        {
            return 1;
            //return AppDomain.GetAssemblyMetadata((uint)module).Version;
        }

        [dotnetJs.MemberReplace]
        internal static Type[] InternalGetTypes(IntPtr module)
        {
            return AppDomain.GetAssembly(new  ReflectionHandleModel { Value = (uint)module })?.GetTypes() ?? [];
        }

        [dotnetJs.MemberReplace]
        private static void GetGuidInternal(IntPtr module, byte[] guid)
        {
            return;
        }

        [dotnetJs.MemberReplace]
        internal static Type? GetGlobalType(IntPtr module)
        {
            return null;
        }

        [dotnetJs.MemberReplace]
        internal static IntPtr ResolveTypeToken(IntPtr module, int token, IntPtr[]? type_args, IntPtr[]? method_args, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return IntPtr.Zero;
        }

        [dotnetJs.MemberReplace]
        internal static IntPtr ResolveMethodToken(IntPtr module, int token, IntPtr[]? type_args, IntPtr[]? method_args, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return IntPtr.Zero;
        }

        [dotnetJs.MemberReplace]
        internal static IntPtr ResolveFieldToken(IntPtr module, int token, IntPtr[]? type_args, IntPtr[]? method_args, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return IntPtr.Zero;
        }

        [dotnetJs.MemberReplace]
        internal static string ResolveStringToken(IntPtr module, int token, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return null!;
        }

        [dotnetJs.MemberReplace]
        internal static MemberInfo ResolveMemberToken(IntPtr module, int token, IntPtr[]? type_args, IntPtr[]? method_args, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return null!;
        }

        [dotnetJs.MemberReplace]
        internal static byte[] ResolveSignature(IntPtr module, int metadataToken, out ResolveTokenError error)
        {
            error = ResolveTokenError.Other;
            return null!;
        }

        [dotnetJs.MemberReplace]
        internal static void GetPEKind(IntPtr module, out PortableExecutableKinds peKind, out ImageFileMachine machine)
        {
            peKind = PortableExecutableKinds.NotAPortableExecutableImage;
            machine = ImageFileMachine.I386;
        }
    }
}
