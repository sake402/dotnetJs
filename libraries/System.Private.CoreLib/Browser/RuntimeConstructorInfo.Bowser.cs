using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
    [NetJs.Boot]
    //[NetJs.Reflectable(false)]
    internal sealed unsafe partial class RuntimeConstructorInfo
    {
        internal RuntimeConstructorInfo(ConstructorModel model)
        {
            mhandle = model.Handle.As<IntPtr>();
            name = model.Name;
            //reftype = model.ReturnType != null ? AppDomain.GetType(model.ReturnType.Value) : null;
            _model = model;
        }

        [NetJs.MemberReplace(nameof(InvokeClassConstructor))]
        internal static void InvokeClassConstructorIImpl(QCallTypeHandle type)
        {
            //var mtype = type.QCallTypeHandleToRuntimeType();
            //var prototype = mtype.DeclaringType.As<RuntimeType>()._prototype;
            //var dobject = NetJs.Script.Write<object>("new prototype()");
            //var ctor = dobject[_model.OutputName!];
            //NetJs.Script.Write("ctor.apply(dobject, parameters)");
            //return dobject;

        }

        [NetJs.MemberReplace(nameof(InternalInvoke))]
        internal object InternalInvokeImpl(object? obj, IntPtr* args, out Exception? exc)
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace(nameof(get_metadata_token))]
        internal static int get_metadata_tokenImpl(RuntimeConstructorInfo method)
        {
            return (int)method._model.Handle;
        }

        [NetJs.MemberReplace(nameof(Invoke) + "(BindingFlags, Binder?, object?[]?, CultureInfo?)")]
        public object InvokeImpl(BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
        {
            var prototype = DeclaringType.As<RuntimeType>()._prototype;
            var dobject = NetJs.Script.Write<object>("new prototype()");
            var outputName = _model.OutputName!.NativeReplace("@", _model.Name);
            var ctor = dobject[outputName];
            NetJs.Script.Write("ctor.apply(dobject, parameters)");
            return dobject;
        }
    }
}
