using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Base DOM Event.
    /// </summary>
    [NetJs.External]
    public class Event
    {
        public extern string type { get; }
        public extern EventTarget? target { get; }
        public extern EventTarget? currentTarget { get; }
        public extern bool bubbles { get; }
        public extern bool cancelable { get; }
        public extern bool defaultPrevented { get; }
        public extern long timeStamp { get; }

        public extern void stopPropagation();
        public extern void stopImmediatePropagation();
        public extern void preventDefault();
    }
}