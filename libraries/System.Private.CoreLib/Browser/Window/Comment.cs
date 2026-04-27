using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Represents a comment node.
    /// </summary>
    [NetJs.External]
    public class Comment : Node
    {
        public extern string data { get; set; }
    }
}