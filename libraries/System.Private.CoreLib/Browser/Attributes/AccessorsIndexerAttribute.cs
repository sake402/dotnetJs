using System;

namespace NetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AccessorsIndexerAttribute : Attribute
    {
    }
}