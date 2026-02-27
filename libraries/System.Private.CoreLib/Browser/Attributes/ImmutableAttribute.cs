using System;

namespace dotnetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class ImmutableAttribute : Attribute
    {
    }
}