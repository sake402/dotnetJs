using System;

namespace NetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class ImmutableAttribute : Attribute
    {
    }
}