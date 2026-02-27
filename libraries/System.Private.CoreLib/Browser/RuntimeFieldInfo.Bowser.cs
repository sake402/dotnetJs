using dotnetJs;

namespace System.Reflection
{
    [dotnetJs.ForcePartial(typeof(RuntimeFieldInfo))]
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

        [dotnetJs.MemberReplace]
        internal object? UnsafeGetValue(object obj)
        {
            if (_model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
                return prototype![_model.Name];
            }
            return obj![_model.Name];
        }

        [dotnetJs.MemberReplace]
        private object? GetValueInternal(object? obj)
        {
            if (_model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
                return prototype![_model.Name];
            }
            return obj![_model.Name];
        }

        [dotnetJs.MemberReplace]
        private Type ResolveType()
        {
            return AppDomain.GetType(_model.FieldType)!;
        }

        [dotnetJs.MemberReplace]
        private Type GetParentType(bool declaring)
        {
            return AppDomain.GetType(_model.DeclaringType)!;
        }

        [dotnetJs.MemberReplace]
        internal int GetFieldOffset()
        {
            throw new NotImplementedException();
        }

        [dotnetJs.MemberReplace]
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

        [dotnetJs.MemberReplace]
        public object GetRawConstantValue()
        {
            var prototype = AppDomain.GetType(_model.FieldType)!._prototype;
            return prototype![_model.Name]!;
        }

        [dotnetJs.MemberReplace]
        internal static int get_metadata_token(RuntimeFieldInfo monoField)
        {
            return (int)monoField.As<RuntimeFieldInfo_Partial>()._model.Handle.Value;
        }

        [dotnetJs.MemberReplace]
        private  Type[] GetTypeModifiers(bool optional, int genericArgumentPosition = -1)
        {
            return Type.EmptyTypes;
        }

    }
}
