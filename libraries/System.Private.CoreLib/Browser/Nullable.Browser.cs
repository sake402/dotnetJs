using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public partial struct Nullable<T>
    {
        [NetJs.MemberReplace(nameof(HasValue))]
        [NetJs.StaticCallConvention]
        public readonly bool HasValueOverride
        {
            get
            {
                var isNull = NetJs.Script.IsNull(this);
                if (isNull)
                    return false;
                var isNullable = Object.HasOwnProperty(this, nameof(hasValue));
                if (isNullable)
                {
                    return hasValue;
                }
                return true;
            }
        }

        [NetJs.MemberReplace(nameof(Value))]
        [NetJs.StaticCallConvention]
        public readonly T ValueOverride
        {
            get
            {
                var isNull = NetJs.Script.IsNull(this);
                if (isNull)
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_NoValue();
                var isNullable = Object.HasOwnProperty(this, nameof(hasValue));
                if (isNullable)
                {
                    if (!hasValue)
                    {
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_NoValue();
                    }
                    return value;
                }
                return this.As<T>();
            }
        }

        //[NonVersionable]
        //public readonly T GetValueOrDefault() => value;

        //[NonVersionable]
        //public readonly T GetValueOrDefault(T defaultValue) =>
        //    hasValue ? value : defaultValue;

        //public override bool Equals(object? other)
        //{
        //    if (!hasValue) return other == null;
        //    if (other == null) return false;
        //    return value.Equals(other);
        //}

        //public override int GetHashCode() => hasValue ? value.GetHashCode() : 0;

        //public override string? ToString() => hasValue ? value.ToString() : "";

        //[NonVersionable]
        //public static implicit operator T?(T value) =>
        //    new T?(value);

        //[NonVersionable]
        //public static explicit operator T(T? value) => value!.Value;

    }
}
