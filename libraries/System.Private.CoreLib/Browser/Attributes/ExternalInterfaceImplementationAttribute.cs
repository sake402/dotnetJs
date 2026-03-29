using System;

namespace NetJs
{
    /// <summary>
    /// Applies to class if its interfaces implementation is handled by another class
    /// The target type must have a single constructor with a single parameter of type of the attributed class
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public sealed class ExternalInterfaceImplementationAttribute : Attribute
    {
        public ExternalInterfaceImplementationAttribute(Type implementationType) { }

        public bool IsVirtual
        {
            get; set;
        }
    }
}