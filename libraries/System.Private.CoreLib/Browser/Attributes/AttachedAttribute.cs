using System;

namespace dotnetJs
{
    /// <summary>
    /// Attach an attribute indirectly to a type/member defined, but we dont want to attach the attribute directly
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AttachedAttribute : Attribute
    {
        [CLSCompliant(false)]
        public AttachedAttribute(string symbolName, Type attributeType, params object[] parameters)
        {

        }

        [CLSCompliant(false)]
        public AttachedAttribute(Type symbolType, Type attributeType, params object[] parameters)
        {

        }
    }
}