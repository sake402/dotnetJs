using dotnetJs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    [dotnetJs.Base(typeof(Delegate))]
    internal class JSFunctionDelegate : ForcedPartialBase<Delegate>
    {
        public JSFunctionDelegate(object target, string method)
        {
            Script.Write("super(target, method)");
            Setup();
        }

        public JSFunctionDelegate(object target, MethodInfo method) : this(target, (parameters) =>
        {
            return method.Invoke(target, parameters);
        })
        {
            Script.Write("super(target, method.Name)");
            Script.Write("this.method_info = method");
            Setup();
        }

        public JSFunctionDelegate(object? target, Func<object[], object?> jsFunction)
        {
            //This._target = target
            Script.Write("this._target = target"); //assign a dummy handle to the method handle
            this["$jsFunction"] = jsFunction;
            Setup();
        }

        void Setup()
        {
            //This.method_info = method;
            Script.Write("this.method = 109848493483"); //assign a dummy handle to the method handle
            Script.Write("this.method_is_virtual = true"); //amke sure the GetVirtualMethod_internalImpl is called
        }
    }

    internal class JSFunctionMethodInfo : MethodInfo
    {
        object? _target;
        Func<object[], object?> _jsFunction;
        public JSFunctionMethodInfo(object? target, Func<object[], object?> jsFunction)
        {
            _target = target;
            _jsFunction = jsFunction;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => new EmptyCAHolder();
        public override MethodAttributes Attributes => MethodAttributes.Public;
        public override RuntimeMethodHandle MethodHandle => new RuntimeMethodHandle(IntPtr.Zero);
        public override string Name => "function";
        public override Type? DeclaringType => _target?.GetType() ?? typeof(object);
        public override Type? ReflectedType => _target?.GetType() ?? typeof(object);

        public override MethodInfo GetBaseDefinition()
        {
            return null!;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return [];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return [];
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return (MethodImplAttributes)0;
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
        {
            //var jsFunction = this["$jsFunction"].As<Func<object[], object?>>();
            //return jsFunction(parameters);
            return Script.Write<object>("this._jsFunction.apply(this._target, parameters)");
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }
    }

    public partial class Delegate
    {
        [dotnetJs.MemberReplace(nameof(AllocDelegateLike_internal))]
        private protected static MulticastDelegate AllocDelegateLike_internalImpl(Delegate d)
        {
            var prototype = typeof(MulticastDelegate).As<RuntimeType>()._prototype;
            var _delegate = prototype.CallDefaultConstructor().As<MulticastDelegate>();
            _delegate.bound = d.bound;
            _delegate.data = d.data;
            _delegate.delegate_trampoline = d.delegate_trampoline;
            _delegate.extra_arg = d.extra_arg;
            _delegate.interp_invoke_impl = d.interp_invoke_impl;
            _delegate.interp_method = d.interp_method;
            _delegate.invoke_impl = d.invoke_impl;
            _delegate.method = d.method;
            _delegate.method_code = d.method_code;
            _delegate.method_info = d.method_info;
            _delegate.method_is_virtual = d.method_is_virtual;
            _delegate.method_ptr = d.method_ptr;
            _delegate.original_method_info = d.original_method_info;
            _delegate._target = d._target;
            return _delegate;
        }

        [dotnetJs.MemberReplace(nameof(CreateDelegate_internal))]
        private static Delegate? CreateDelegate_internalImpl(QCallTypeHandle type, object? target, MethodInfo info, bool throwOnBindFailure)
        {
            var returnType = type.QCallTypeHandleToRuntimeType();
            return new JSFunctionDelegate(target, info).As<Delegate>();
        }

        [dotnetJs.MemberReplace(nameof(GetVirtualMethod_internal))]
        private MethodInfo GetVirtualMethod_internalImpl()
        {
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
            if (this is JSFunctionDelegate)
            {
                var jsFunction = this["$jsFunction"].As<Func<object[], object?>>();
                return new JSFunctionMethodInfo(_target, jsFunction);
            }
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
            throw new NotImplementedException();
        }

    }
}
