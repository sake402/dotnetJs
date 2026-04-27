using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Storage (localStorage / sessionStorage) wrapper.
    /// </summary>
    [NetJs.External]
    public class Storage
    {
        public extern int length { get; }
        public extern string? key(int index);
        public extern string? getItem(string key);
        public extern void setItem(string key, string value);
        public extern void removeItem(string key);
        public extern void clear();
    }
}