using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Using : CSharpSyntax
    {
        public Using(string @namespace)
        {
            NameSpace = @namespace;
        }

        string NameSpace { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.WriteLine($"using {NameSpace};");
        }
    }
}
