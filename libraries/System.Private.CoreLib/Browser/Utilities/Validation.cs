using System;
using System.Collections;

namespace dotnetJs
{
    [dotnetJs.Convention(Member = dotnetJs.ConventionMember.Field | dotnetJs.ConventionMember.Method, Notation = dotnetJs.Notation.CamelCase)]
    public sealed class Validation
    {
        public static bool IsNull(object value) => value == null;

        public static bool IsEmpty(object value) => value == null || value.ToString().Length == 0 || (value is IEnumerable e /*&& e.Count() == 0*/);

        public static extern bool IsNotEmptyOrWhitespace(string value);

        public static extern bool IsNotNull(object value);

        public static extern bool IsNotEmpty(object value);

        public static extern bool Email(string value);

        public static extern bool Url(string value);

        public static extern bool Alpha(string value);

        public static extern bool AlphaNum(string value);

        public static extern bool CreditCard(string value);

        public static extern bool CreditCard(string value, CreditCardType type);
    }

    [External]
    [Enum(Emit.StringNamePreserveCase)]
    public enum CreditCardType
    {
        Default,
        Visa,
        MasterCard,
        Discover,
        AmericanExpress,
        DinersClub
    }
}