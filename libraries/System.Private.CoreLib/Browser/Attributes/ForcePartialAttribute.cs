using System;

namespace NetJs
{
    /// <summary>
    /// An existing class was not defined as partial, but we want to force a partial declaration onto it so we can extend such class
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class ForcePartialAttribute : Attribute
    {
        public ForcePartialAttribute(Type existingType) { }
    }
}