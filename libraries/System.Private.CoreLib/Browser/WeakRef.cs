namespace System
{
    [dotnetJs.External]
    [dotnetJs.IgnoreGeneric]
    [dotnetJs.Name("WeakRef")]
    public class WeakRef<T>
    {
        public extern WeakRef(T value);
        public T Value
        {
            [dotnetJs.Template("{this}.deref()")]
            get;
        }
    }
}
