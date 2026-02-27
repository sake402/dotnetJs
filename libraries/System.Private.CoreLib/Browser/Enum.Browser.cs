using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    public partial class Enum
    {
        [dotnetJs.MemberReplace(nameof(GetEnumValuesAndNames))]
        private static void GetEnumValuesAndNamesImpl(QCallTypeHandle enumType, out ulong[] values, out string[] names)
        {
            var prototype = enumType.QCallTypeHandleToRuntimeType()._prototype.As<EnumPrototype>();
            names = prototype.Map.Keys;
            values = prototype.Map.Values.As<ulong[]>();
        }

        [dotnetJs.MemberReplace(nameof(InternalGetCorElementType))]
        private static CorElementType InternalGetCorElementTypeImpl(QCallTypeHandle enumType)
        {
            var prototype = enumType.QCallTypeHandleToRuntimeType()._prototype.As<EnumPrototype>();
            var type = prototype.UnderlyingType.Type.As<RuntimeType>();
            return RuntimeTypeHandle.GetCorElementType(new QCallTypeHandle(ref type));
        }
        
        [dotnetJs.MemberReplace(nameof(InternalGetUnderlyingType))]
        private static void InternalGetUnderlyingTypeImpl(QCallTypeHandle enumType, ObjectHandleOnStack res)
        {
            var prototype = enumType.QCallTypeHandleToRuntimeType()._prototype.As<EnumPrototype>();
            res.GetObjectHandleOnStack<Type?>() = prototype.UnderlyingType.Type;
        }

    }
}
