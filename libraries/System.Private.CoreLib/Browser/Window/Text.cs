using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Represents textual content in the DOM.
    /// </summary>
    [NetJs.External]
    public class Text : Node
    {
        public extern string data { get; set; }
        public extern int length { get; }
        public extern Text splitText(int offset);
    }
}