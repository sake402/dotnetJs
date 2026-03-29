using System;

namespace NetJs
{
    /// <summary>
    /// Call a method using static conventions. Eg Array a; a.GetValue(...) => System.Array.GetValue.call(a, ...)
    /// This is an extensibility point for Js native type. Eg if a native js string doesnt have a method Foo and we have such definition in .Net. 
    /// The method shoulld be marked with this attribute so it get generated as static Foo() and called as System.String.Foo.call(string) from string.Foo()
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class StaticCallConventionAttribute : Attribute
    {
        public StaticCallConventionAttribute() { }
        public StaticCallConventionAttribute(bool enabled) { }
    }
}