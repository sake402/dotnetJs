using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class FunctionDataType : Type
    {
        public FunctionDataType(ICSharpClosure closure, string name) : base(closure, name)
        {
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.Write($"Func<object>");
        }
    }
}
