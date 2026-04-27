using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Browser console APIs.
    /// </summary>
    [NetJs.External]
    public class Console
    {
        public extern void log(params object?[] args);
        public extern void info(params object?[] args);
        public extern void warn(params object?[] args);
        public extern void error(params object?[] args);
        public extern void debug(params object?[] args);
        public extern void assert(bool condition, params object?[] args);
        public extern void clear();
        public extern void dir(object? obj);
        public extern void trace();
        public extern void time(string label);
        public extern void timeEnd(string label);
    }
}