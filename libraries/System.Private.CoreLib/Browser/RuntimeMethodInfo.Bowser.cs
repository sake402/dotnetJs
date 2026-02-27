using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection
{
    [dotnetJs.ForcePartial(typeof(MonoMethodInfo))]
    internal partial struct MonoMethodInfo_Partial
    {
        [dotnetJs.MemberReplace]
        private static int get_method_attributes(IntPtr handle)
        {
            MethodAttributes attrs = 0;
            var method = (MethodInfo)AppDomain.GetMember(new ReflectionHandleModel { Value = (uint)handle })!;
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsPublic))
            {
                attrs |= MethodAttributes.Public;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsPrivate))
            {
                attrs |= MethodAttributes.Private;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                attrs |= MethodAttributes.Static;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsVirtual))
            {
                attrs |= MethodAttributes.Virtual;
            }
            return (int)attrs;
        }

        [dotnetJs.MemberReplace]
        private static void get_method_info(IntPtr handle, out MonoMethodInfo info)
        {
            MonoMethodInfo minfo = default!;
            var method = (MethodInfo)AppDomain.GetMember(new ReflectionHandleModel { Value = (uint)handle })!;
            var dt = method.DeclaringType;
            dotnetJs.Script.Write("minfo.parent = dt");
            var rt = method.ReturnType;
            dotnetJs.Script.Write("minfo.ret = rt");
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsPublic))
            {
                minfo.attrs |= MethodAttributes.Public;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsPrivate))
            {
                minfo.attrs |= MethodAttributes.Private;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                minfo.attrs |= MethodAttributes.Static;
            }
            if (method._miMetadata.Flags.TypeHasFlag(MemberFlagsModel.IsVirtual))
            {
                minfo.attrs |= MethodAttributes.Virtual;
            }
            info = minfo;
        }

        [dotnetJs.MemberReplace]
        private static ParameterInfo[] get_parameter_info(IntPtr handle, MemberInfo member)
        {
            var method = member.As<RuntimeMethodInfo>();
            return method._model.Parameters?.Map((p, i, all) => new RuntimeParameterInfo_Partial(p, AppDomain.GetType(p.ParameterType) ?? throw new InvalidOperationException(), method, i).As<RuntimeParameterInfo>()) ?? Array.Empty<ParameterInfo>();
        }

        [dotnetJs.MemberReplace]
        private static MarshalAsAttribute get_retval_marshal(IntPtr handle)
        {
            return null!;
        }
    }

    internal sealed unsafe partial class RuntimeMethodInfo
    {
        internal MethodModel _model;

        //For generic method
        internal RuntimeMethodInfo? _genericMethod;
        internal Type[]? _typeArguments;

        internal RuntimeMethodInfo(MethodModel model)
        {
            mhandle = (IntPtr)model.Handle.Value;
            name = model.Name;
            reftype = model.ReturnType != null ? AppDomain.GetType(model.ReturnType.Value) : null;
            _model = model;
        }

        [dotnetJs.MemberReplace(nameof(GetMethodBodyInternal))]
        internal static MethodBody GetMethodBodyInternalImpl(IntPtr handle)
        {
            return null!;
        }

        [dotnetJs.MemberReplace(nameof(GetMethodFromHandleInternalType_native))]
        private static MethodBase GetMethodFromHandleInternalType_nativeImpl(IntPtr method_handle, IntPtr type_handle, bool genericCheck)
        {
            var member = AppDomain.GetMember(new ReflectionHandleModel { Value = (uint)method_handle });
            return (MethodBase)member!;
        }

        [dotnetJs.MemberReplace(nameof(get_name))]
        internal static string get_nameImpl(MethodBase method)
        {
            return method.As<RuntimeMethodInfo>()._model.Name;
        }

        [dotnetJs.MemberReplace(nameof(get_base_method))]
        internal static RuntimeMethodInfo get_base_methodImpl(RuntimeMethodInfo method, bool definition)
        {
            var baseType = method.DeclaringType.BaseType.As<RuntimeType>();
            return baseType.GetMethod(method.Name).As<RuntimeMethodInfo>();
        }

        [dotnetJs.MemberReplace(nameof(get_metadata_token))]
        internal static int get_metadata_tokenImpl(RuntimeMethodInfo method)
        {
            return (int)method._model.Handle.Value;
        }

        [dotnetJs.MemberReplace(nameof(InternalInvoke))]
        internal object? InternalInvokeImpl(object? obj, IntPtr* args, out Exception? exc)
        {
            throw new NotImplementedException();
        }

        [dotnetJs.MemberReplace(nameof(GetPInvoke))]
        internal void GetPInvokeImpl(out PInvokeAttributes flags, out string entryPoint, out string dllName)
        {
            throw new NotImplementedException();
        }

        [dotnetJs.MemberReplace(nameof(MakeGenericMethod_impl))]
        private MethodInfo MakeGenericMethod_implImpl(Type[] types)
        {
            return new RuntimeMethodInfo(_model)
            {
                _genericMethod = this,
                _typeArguments = types
            };
        }

        [dotnetJs.MemberReplace(nameof(GetGenericArguments))]
        public Type[] GetGenericArgumentsImpl()
        {
            return _typeArguments ?? Type.EmptyTypes;
        }

        [dotnetJs.MemberReplace(nameof(GetGenericMethodDefinition_impl))]
        private MethodInfo GetGenericMethodDefinition_implImpl()
        {
            return _genericMethod ?? this;
        }

        [dotnetJs.MemberReplace(nameof(IsGenericMethodDefinition))]
        public bool IsGenericMethodDefinitionImpl
        {
            get => _model.Flags.TypeHasFlag(MemberFlagsModel.IsGeneric) && _typeArguments == null;
        }

        [dotnetJs.MemberReplace(nameof(IsGenericMethod))]
        public bool IsGenericMethodImpl
        {
            get => _model.Flags.TypeHasFlag(MemberFlagsModel.IsGeneric);
        }
    }
}
