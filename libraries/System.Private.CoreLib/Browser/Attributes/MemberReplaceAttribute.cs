using System;

namespace NetJs
{
    [Flags]
    public enum MemberReplaceType
    {
        None,
        Body = 1 << 0,
        Attributes = 1 << 1,
        Modifiers = 1 << 2,
        All = 0x7FFFFFFF
    }
    /// <summary>
    /// Copies the content of a member to replace the one already defined in another member, both within the same type(partial)
    /// Useful for implementing Intrisic and MethodImplOptions.InternalCall that will end up being replaced
    /// </summary>
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class MemberReplaceAttribute : Attribute
    {
        public MemberReplaceAttribute() { }
        public MemberReplaceAttribute(string originalMemberName) { }
        public MemberReplaceAttribute(string originalMemberName, MemberReplaceType type) { }
    }

    [NonScriptable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class MemberParameterCountMayNotMatch : Attribute
    {
    }
    [NonScriptable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class MemberParameterTypesMayNotMatch : Attribute
    {
    }
}