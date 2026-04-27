using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Interface : Class
    {
        public Interface(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class> baseClass = null) : base(closure, name, access, partial, baseClass)
        {
        }

        public override ClosureType ClosureType => ClosureType.Interface;
    }
}
