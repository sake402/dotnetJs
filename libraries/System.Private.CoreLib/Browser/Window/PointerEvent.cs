using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Pointer event wrapper.
    /// </summary>
    [NetJs.External]
    public class PointerEvent : MouseEvent
    {
        public extern long pointerId { get; }
        public extern string? pointerType { get; }
        public extern bool isPrimary { get; }

        public extern PointerEvent(string type, object? eventInitDict = null);
    }
}