//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace System
//{
//    public partial struct Nullable<T>
//    {
//        [dotnetJs.MemberReplace(nameof(HasValue))]
//        public extern readonly bool HasValueOverride
//        {
//            [dotnetJs.Template("{this} != null")]
//            get;
//        }

//        [dotnetJs.MemberReplace(nameof(Value))]
//        public extern readonly T ValueOverride
//        {
//            [dotnetJs.Template("{this}")]
//            get;
//        }

//        [NonVersionable]
//        public readonly T GetValueOrDefault() => value;

//        [NonVersionable]
//        public readonly T GetValueOrDefault(T defaultValue) =>
//            hasValue ? value : defaultValue;

//        public override bool Equals(object? other)
//        {
//            if (!hasValue) return other == null;
//            if (other == null) return false;
//            return value.Equals(other);
//        }

//        public override int GetHashCode() => hasValue ? value.GetHashCode() : 0;

//        public override string? ToString() => hasValue ? value.ToString() : "";

//        [NonVersionable]
//        public static implicit operator T?(T value) =>
//            new T?(value);

//        [NonVersionable]
//        public static explicit operator T(T? value) => value!.Value;

//    }
//}
