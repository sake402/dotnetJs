namespace System
{
    [NetJs.External]
    [NetJs.IgnoreGeneric]
    [NetJs.Name("WeakRef")]
    public class WeakRef<T>
    {
        public extern WeakRef(T value);
        public T Value
        {
            [NetJs.Template("{this}.deref()")]
            get;
        }
    }
}
