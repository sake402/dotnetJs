using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// CustomEvent wrapper.
    /// </summary>
    [NetJs.External]
    public class CustomEvent : Event
    {
        public extern object? detail { get; }
        public extern CustomEvent(string type, object? eventInitDict = null);
    }
}