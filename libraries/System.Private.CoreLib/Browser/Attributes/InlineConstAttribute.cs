using System;

namespace dotnetJs
{
    /// <summary>
    ///
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public sealed class InlineConstAttribute : Attribute
    {
    }
}