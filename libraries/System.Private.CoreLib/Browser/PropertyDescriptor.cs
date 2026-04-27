namespace System
{
    [NetJs.ObjectLiteral]
    [NetJs.Convention(NetJs.Notation.CamelCase)]
    public class PropertyDescriptor
    {
        public bool? Configurable { get; set; }
        public bool? Enumerable { get; set; }
        public object? Value { get; set; }
        public bool? Writable { get; set; }
        public Func<object>? Get { get; set; }
        public Action<object>? Set { get; set; }
    }
}
