namespace dotnetJs.Translator.RazorToCSharp
{
    public abstract class RazorXmlHasChildrenNode : RazorXmlNode
    {
        protected RazorXmlHasChildrenNode(RazorXmlNode? parent) : base(parent)
        {

        }
        public List<RazorXmlNode> Children { get; } = new List<RazorXmlNode>();

        public override string ToString()
        {
            return $"{string.Join("\r\n", Children.Select(c => c.ToString()))}";
        }
    }
}
