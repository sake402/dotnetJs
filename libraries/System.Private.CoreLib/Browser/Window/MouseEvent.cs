using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Mouse event wrapper.
    /// </summary>
    [NetJs.External]
    public class MouseEvent : UIEvent
    {
        public extern int screenX { get; }
        public extern int screenY { get; }
        public extern int clientX { get; }
        public extern int clientY { get; }
        public extern int button { get; }
        public extern int buttons { get; }
        public extern bool altKey { get; }
        public extern bool ctrlKey { get; }
        public extern bool shiftKey { get; }
        public extern bool metaKey { get; }

        public extern MouseEvent(string type, object? eventInitDict = null);
    }
}