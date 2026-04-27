using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Location (window.location) wrapper.
    /// </summary>
    [NetJs.External]
    public class Location
    {
        public extern string href { get; set; }
        public extern string protocol { get; set; }
        public extern string host { get; set; }
        public extern string hostname { get; set; }
        public extern string pathname { get; set; }
        public extern string search { get; set; }
        public extern string hash { get; set; }
        public extern string? origin { get; }

        public extern void assign(string url);
        public extern void replace(string url);
        public extern void reload(bool forceGet = false);
        public extern string toString();
    }
}