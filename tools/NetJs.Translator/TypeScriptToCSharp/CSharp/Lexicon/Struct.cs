using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Struct : Object
    {
        public Struct(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Private, bool partial = true) : base(closure, name, access, partial)
        {
        }

        public override ClosureType ClosureType => ClosureType.Struct;

        public override bool InheritFrom(Object @object)
        {
            return false;
        }
    }
}
