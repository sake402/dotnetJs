using NetJs;

namespace System.Reflection
{
    /// <summary>
    /// For types that needs to reference itself(defined via typeproxy)
    /// This is simply a JS proxy handler that forwards request to the proxy to the System.Type/Prototype itself
    /// </summary>
    [Boot]
    [Reflectable(false)]
    class TypeProxyHandler
    {
        public TypeProxyHandler(string fullName)
        {
            FullName = fullName;
        }

        string FullName { get; }
        /// <summary>
        /// The finally created type we will proxy to
        /// </summary>
        internal Type? TargetType { get; set; }
        internal TypePrototype? Prototype { get; set; }
        [Name("get")]
        public object? Get(object target, string property, object receiver)
        {
            if (TargetType != null && Prototype != null)
            {
                var v1 = Prototype[property];
                var v2 = TargetType[property];
                if (Script.IsDefined(v1) && Script.IsDefined(v2) & v1 != v2)
                {
                    throw new AmbiguousMatchException($"Due to a limitation on the type system, Type \"{FullName}\"(being dependent on itself and implemented via a TypeProxy), cannot have a member whose name clashes with a System.Type member. Name \"{property}\" caused a clash.");
                }
                return v1 ?? v2;
            }
            //We will need these properties early before the proxy is bound to its type
            if (property == nameof(FullName))
                return FullName;
            else if (property == "$type")
                return this;
            else if (property == "IsGenericTypeDefinition")
                return FullName.NativeEndsWith(">");
            return null;
        }
        [Name("set")]
        public bool Set(object target, string property, object value)
        {
            //Update the target of the proxy
            if (property == nameof(TargetType))
            {
                TargetType = value.As<Type>();
                return true;
            }
            else if (property == nameof(Prototype))
            {
                Prototype = value.As<TypePrototype>();
                return true;
            }
            return false;
        }
    }
}
