using NetJs;

namespace System.Reflection
{
    [NetJs.ForcePartial(typeof(RuntimeFieldInfo))]
    [NetJs.Boot]
    [NetJs.Reflectable(false)]
    internal sealed partial class RuntimeFieldInfo_Partial : ForcedPartialBase<RuntimeFieldInfo>
    {
        internal FieldModel _model;
        internal RuntimeFieldInfo_Partial()
        {
        }

        internal RuntimeFieldInfo_Partial(FieldModel model)
        {
            _model = model;
        }

        [NetJs.MemberReplace]
        internal object? UnsafeGetValue(object obj)
        {
            if (_model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
                return prototype![_model.Name];
            }
            return obj![_model.Name];
        }

        [NetJs.MemberReplace]
        private object? GetValueInternal(object? obj)
        {
            if (_model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
                return prototype![_model.Name];
            }
            return obj![_model.Name];
        }

        [NetJs.MemberReplace]
        private Type ResolveType()
        {
            return AppDomain.GetType(_model.FieldType)!;
        }

        [NetJs.MemberReplace]
        private Type GetParentType(bool declaring)
        {
            return AppDomain.GetType(_model.DeclaringType)!;
        }

        [NetJs.MemberReplace]
        internal int GetFieldOffset()
        {
            throw new NotImplementedException();
        }

        [NetJs.MemberReplace]
        private static void SetValueInternal(FieldInfo fi, object? obj, object? value)
        {
            var field = fi.As<RuntimeFieldInfo_Partial>();
            if (field._model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(field._model.FieldType)!._prototype;
                prototype![field._model.Name] = value;
            }
            obj![field._model.Name] = value;
        }

        [NetJs.MemberReplace]
        public object GetRawConstantValue()
        {
            var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
            return prototype![_model.Name]!;
        }

        [NetJs.MemberReplace]
        internal static int get_metadata_token(RuntimeFieldInfo monoField)
        {
            return (int)monoField.As<RuntimeFieldInfo_Partial>()._model.Handle.Value;
        }

        [NetJs.MemberReplace]
        private  Type[] GetTypeModifiers(bool optional, int genericArgumentPosition = -1)
        {
            return Type.EmptyTypes;
        }

    }
}
