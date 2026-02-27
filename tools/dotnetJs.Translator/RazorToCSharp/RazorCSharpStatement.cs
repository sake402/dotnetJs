using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorCSharpStatement : RazorXmlNode
    {
        public RazorCSharpStatement(string code, RazorXmlNode? parentNode) : base(parentNode)
        {
            Code = code;
        }

        public string Code { get; }

        public override string GenerateCode(int tabDepth, int parameterDepth, ComponentCodeGenerationContext context)
        {
            return $"{GetCodeFormatTabs(tabDepth)}{Code}";
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
