using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Focus event wrapper.
    /// </summary>
    [NetJs.External]
    public class FocusEvent : UIEvent
    {
        public extern EventTarget? relatedTarget { get; }

        public extern FocusEvent(string type, object? eventInitDict = null);
    }
}