using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorCSharpMethodBlock : RazorCodeBlock
    {
        public RazorCSharpMethodBlock(string blockStart, string blockEnd, RazorXmlNode? parentNode) : base(blockStart, blockEnd, parentNode)
        {
        }
    }
}
