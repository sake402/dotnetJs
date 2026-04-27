using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Represents the global window object and Browser API surface.
    /// </summary>
    [NetJs.External]
    public class Window
    {
        public extern Document? document { get; }
        public extern Console? console { get; }
        public extern Navigator? navigator { get; }
        public extern Location? location { get; }
        public extern History? history { get; }

        public extern double innerWidth { get; }
        public extern double innerHeight { get; }

        public extern int setTimeout(Action handler, int timeout);
        public extern int setInterval(Action handler, int timeout);
        public extern void clearTimeout(int id);
        public extern void clearInterval(int id);

        public extern void alert(string message);
        public extern bool confirm(string message);
        public extern string? prompt(string message, string? defaultValue = null);

        public extern void addEventListener(string type, object listener, object? options = null);
        public extern void removeEventListener(string type, object listener, object? options = null);
        public extern bool dispatchEvent(Event evt);
    }
}