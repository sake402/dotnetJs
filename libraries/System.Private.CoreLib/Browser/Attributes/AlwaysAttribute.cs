using System;

namespace NetJs
{
    /// <summary>
    /// Defina a method, property or field that always returns this value. Allows compiler to optimize/remove branches.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    [NonScriptable]
    public class AlwaysAttribute : Attribute
    {
        public AlwaysAttribute(object value) { }
    }
}