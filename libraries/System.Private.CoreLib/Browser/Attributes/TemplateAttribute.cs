using System;

namespace NetJs
{
    /// <summary>
    /// TemplateAttribute is instruction to replace method calling (in expression) by required code
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = true)]
    public sealed class TemplateAttribute : Attribute
    {
        internal TemplateAttribute()
        {
        }
        
        public TemplateAttribute(string format)
        {
        }

        public TemplateAttribute(string format, string condition)
        {
        }

        //public TemplateAttribute(string format, string nonExpandedFormat)
        //{
        //}

        //public string? Fn { get; set; }
    }
}