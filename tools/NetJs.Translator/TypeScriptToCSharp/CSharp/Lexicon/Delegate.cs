using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Delegate : Method
    {
        public Delegate(ICSharpClosure @object, string name, AccessSpecifier access, Type @return, bool @static = false, IEnumerable<MethodParameter> parameters = null) : base(@object, name, access, @return, @static, parameters)
        {
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.Write($"delegate {Return} {Name}");
            base.Write(formatter, comment);
        }
    }
}
