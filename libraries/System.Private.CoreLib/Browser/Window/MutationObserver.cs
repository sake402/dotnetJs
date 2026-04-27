using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// MutationObserver wrapper.
    /// </summary>
    [NetJs.External]
    public class MutationObserver
    {
        public extern MutationObserver(Action<MutationRecord[]> callback);
        public extern void observe(Node target, object options);
        public extern void disconnect();
        public extern MutationRecord[] takeRecords();
    }

    [NetJs.External]
    public class MutationRecord
    {
        public extern string type { get; }
        public extern Node? target { get; }
        public extern NodeList? addedNodes { get; }
        public extern NodeList? removedNodes { get; }
        public extern Node? previousSibling { get; }
        public extern Node? nextSibling { get; }
        public extern string? attributeName { get; }
        public extern string? attributeNamespace { get; }
        public extern string? oldValue { get; }
    }
}