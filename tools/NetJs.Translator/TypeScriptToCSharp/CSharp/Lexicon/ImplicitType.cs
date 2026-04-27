using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class ImplicitType : Type
    {
        public ImplicitType(ICSharpClosure closure, string name, Type alias = null) : base(closure, name, alias)
        {
        }

        public override string ToString()
        {
            return $"implicit operator {Name}";
        }
    }
}
