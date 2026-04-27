using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public interface ICSharpSyntax
    {
        void PreConvert();
        void Write(ICSharpFormatter formatter, bool comment = true);        
    }
}
