using System;
using System.Runtime.CompilerServices;

namespace Window
{
    /// <summary>
    /// Base DOM Node.
    /// </summary>
    [NetJs.External]
    public class Node
    {
        public extern string? nodeName { get; }
        public extern string? nodeValue { get; set; }
        public extern ushort nodeType { get; }
        public extern Node? parentNode { get; }
        public extern NodeList childNodes { get; }
        public extern Document? ownerDocument { get; }

        public extern Node appendChild(Node node);
        public extern Node insertBefore(Node newNode, Node? referenceNode);
        public extern Node removeChild(Node node);
        public extern Node replaceChild(Node newChild, Node oldChild);
        public extern Node cloneNode(bool deep = false);

        public extern bool contains(Node? other);
    }
}