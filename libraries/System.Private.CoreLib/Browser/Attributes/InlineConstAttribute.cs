using System;

namespace NetJs
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