using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// History API wrapper.
    /// </summary>
    [NetJs.External]
    public class History
    {
        public extern int length { get; }
        public extern object? state { get; }

        public extern void back();
        public extern void forward();
        public extern void go(int delta);
        public extern void pushState(object? state, string? title, string? url = null);
        public extern void replaceState(object? state, string? title, string? url = null);
    }
}