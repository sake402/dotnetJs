using System;

namespace NetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BoxAttribute : Attribute
    {
        /// <summary>
        /// Controls boxing of method parameter.
        /// </summary>
        /// <param name="allow">False skips generating unboxing.</param>
        public BoxAttribute(bool allow) { }
    }

}