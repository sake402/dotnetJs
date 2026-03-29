using System;

namespace NetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ConstructorAttribute : Attribute
    {
        public ConstructorAttribute(string value)
        {
        }
    }
}