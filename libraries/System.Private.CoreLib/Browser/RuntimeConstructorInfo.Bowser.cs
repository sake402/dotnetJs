using NetJs;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
    [NetJs.Boot]
    [NetJs.Reflectable(false)]
    internal sealed unsafe partial class RuntimeConstructorInfo
    {
        internal ConstructorModel _model;

        internal RuntimeConstructorInfo(ConstructorModel model)
        {
            mhandle = (IntPtr)model.Handle.Value;
            name = model.Name;
            reftype = model.ReturnType != null ? AppDomain.GetType(model.ReturnType.Value) : null;
            _model = model;
        }

        [NetJs.MemberReplace(nameof(InvokeClassConstructor))]
        internal static void InvokeClassConstructorIImpl(QCallTypeHandle type)
        {

        }

        [NetJs.MemberReplace(nameof(InternalInvoke))]
        internal object InternalInvokeImpl(object? obj, IntPtr* args, out Exception? exc)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(get_metadata_token))]
        internal static int get_metadata_tokenImpl(RuntimeConstructorInfo method)
        {
            return (int)method._model.Handle.Value;
        }

    }
}
