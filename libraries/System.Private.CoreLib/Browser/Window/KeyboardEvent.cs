using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Keyboard event wrapper.
    /// </summary>
    [NetJs.External]
    public class KeyboardEvent : UIEvent
    {
        public extern string key { get; }
        public extern string? code { get; }
        public extern bool altKey { get; }
        public extern bool ctrlKey { get; }
        public extern bool shiftKey { get; }
        public extern bool metaKey { get; }

        public extern KeyboardEvent(string type, object? eventInitDict = null);
    }
}