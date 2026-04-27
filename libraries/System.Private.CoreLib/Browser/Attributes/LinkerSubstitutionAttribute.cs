using System;

namespace NetJs
{
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LinkerSubstitutionAttribute : Attribute
    {
        public LinkerSubstitutionAttribute(string signature, string value) { }
    }
}