using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Window
{
    /// <summary>
    /// Touch event wrapper.
    /// </summary>
    [NetJs.External]
    public class TouchEvent : UIEvent
    {
        public extern TouchList touches { get; }
        public extern TouchList targetTouches { get; }
        public extern TouchList changedTouches { get; }

        public extern bool altKey { get; }
        public extern bool metaKey { get; }
        public extern bool ctrlKey { get; }
        public extern bool shiftKey { get; }

        public extern TouchEvent(string type, object? eventInitDict = null);
    }

    [NetJs.External]
    public class Touch
    {
        public extern long identifier { get; }
        public extern double clientX { get; }
        public extern double clientY { get; }
        public extern double screenX { get; }
        public extern double screenY { get; }
        public extern EventTarget? target { get; }
    }

    [NetJs.External]
    public class TouchList
    {
        public extern int length { get; }
        public extern Touch? item(int index);
    }
}