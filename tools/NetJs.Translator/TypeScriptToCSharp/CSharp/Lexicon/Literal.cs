using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Literal : Property
    {
        public Literal(ICSharpClosure closure, string name, AccessSpecifier access, Type type, bool @static = false, bool @readonly = false, bool optional = false, AccessSpecifier get = AccessSpecifier.Public, AccessSpecifier set = AccessSpecifier.Public) : base(closure, name, access, type, @static, @readonly, optional, get, set)
        {
        }

        public override Type UnderlyingType => Type;

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.WriteLine($"public static {Type} {Name.Trim(new char[] { '"' }).FormatCSharpName()} = {Name};");
        }

        public override bool Equals(object obj)
        {
            Literal lc = obj as Literal;
            if (lc != null)
            {
                return Name.Equals(lc.Name) && Type.Equals(lc.Type);
            }
            return base.Equals(obj);
        }
    }
}
