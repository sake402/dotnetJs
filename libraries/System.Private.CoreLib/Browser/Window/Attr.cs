using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Represents an attribute on an Element.
    /// </summary>
    [NetJs.External]
    public class Attr
    {
        public extern string name { get; }
        public extern string? value { get; set; }
        public extern bool specified { get; }
        public extern Element? ownerElement { get; }
    }
}