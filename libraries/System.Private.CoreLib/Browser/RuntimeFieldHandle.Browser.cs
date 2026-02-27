using dotnetJs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    [dotnetJs.ForcePartial(typeof(RuntimeFieldHandle))]
    public partial struct RuntimeFieldHandle_Partial
    {
        [dotnetJs.MemberReplace]
        private static void SetValueInternal(FieldInfo fi, object? obj, object? value)
        {
            var model = fi.As<RuntimeFieldInfo_Partial>()._model;
            if (model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(model.FieldType)!._prototype;
                prototype![model.Name] = value;
            }
            obj![model.Name] = value;
        }

        [dotnetJs.MemberReplace]
        internal static  unsafe object GetValueDirect(RuntimeFieldInfo field, RuntimeType fieldType, void* pTypedRef, RuntimeType? contextType)
        {
            var obj = *(object*)pTypedRef;
            var model = field.As<RuntimeFieldInfo_Partial>()._model;
            if (model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = AppDomain.GetType(model.FieldType)!._prototype;
                return prototype![model.Name];
            }
            return obj![model.Name];
        }

        [dotnetJs.MemberReplace]
        internal static  unsafe void SetValueDirect(RuntimeFieldInfo field, RuntimeType fieldType, void* pTypedRef, object value, RuntimeType? contextType)
        {
            var obj = *(object*)pTypedRef;
            var model = field.As<RuntimeFieldInfo_Partial>()._model;
            if (model.Flags.TypeHasFlag(MemberFlagsModel.IsStatic))
            {
                var prototype = fieldType._prototype;
                prototype![model.Name] = value;
            }
            obj![model.Name] = value;
        }

    }
}
