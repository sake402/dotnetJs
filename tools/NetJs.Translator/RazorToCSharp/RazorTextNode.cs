using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.RazorToCSharp
{
    public class RazorTextNode : RazorTextBaseNode
    {
        public RazorTextNode(string text, RazorXmlNode? parentNode) : base(parentNode)
        {
            Text = text;
        }

        public string Text { get; }

        public override string ToString()
        {
            return $"{ToStringFormatTabs}{Text}";
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            return $"{GetCodeFormatTabs(tabDepth)}__frame{parameterDepth}.Text(\"{RazorUtility.Escape(Text.AsSpan())}\"{(parameterDepth > 0 ? $", key: __key{parameterDepth}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
        }
    }
}
