
namespace System.Private.CoreLib.Browser
{
    public interface INativeValueBox<T> : IComparable, IComparable<T>, IEquatable<T>
    {
        T Value { get; }
        //int IComparable.CompareTo(object? obj)
        //{
        //    if (obj is T b)
        //    {
        //        return Script.Equals(b, Value) ? 0 : !Script.Equals(Value, default(T)) && Script.Equals(b, default(T)) ? 1 : -1;
        //    }
        //    return 1;
        //}

        //int IComparable<T>.CompareTo(T? b)
        //{
        //    return Script.Equals(b, Value) ? 0 : !Script.Equals(Value, default(T)) && Script.Equals(b, default(T)) ? 1 : -1;
        //}

        //bool IEquatable<T>.Equals(T? other)
        //{
        //    return Script.Equals(Value, other);
        //}
    }
}
