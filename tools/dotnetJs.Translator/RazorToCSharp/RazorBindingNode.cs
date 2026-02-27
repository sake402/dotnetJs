using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorBindingNode : RazorTextBaseNode
    {
        public RazorBindingNode(string expression, RazorXmlNode? parentNode) : base(parentNode)
        {
            Expression = expression;
        }

        public string Expression { get; }

        public override string ToString()
        {
            return $"{ToStringFormatTabs}@{Expression}";
        }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            return $"{GetCodeFormatTabs(tabDepth)}__frame{parameterDepth}.Content({Expression}{(parameterDepth > 0 ? $", key: __key{parameterDepth}" : "")}, sequenceNumber: {context.RazorSequenceNumber++});";
        }
    }
}
