using System;

namespace NetJs
{
    /// <summary>
    /// Use this to arrange how types are exported, if they depend on each other
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    [NonScriptable]
    public class OutputOrderAttribute : Attribute
    {
        public OutputOrderAttribute(int order) { }
    }
}