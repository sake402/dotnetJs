using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Variable:Member
    {
        public Variable(ICSharpClosure closure, Type type, string name, AccessSpecifier access = AccessSpecifier.Private, bool @static = false, bool @readonly = false, string initialValue = null):base(closure, name, access, @static, @readonly)
        {
            Type = type;
            InitialValue = initialValue;
        }

        public Type Type { get; private set; }
        public string InitialValue { get; protected set; }

        public override string ToString()
        {
            return $"{Type} {Name.FormatCSharpName()}";
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (comment)
            {
                Comment?.Write(formatter);
            }
            foreach (var attr in Attributes)
            {
                formatter.WriteLine($"[{attr}]");
            }
            formatter.Write($"{Access.ToString().ToLower()}{(Static ? " static" : "")}{(Readonly ? " readonly" : "")} {Type} {Name}{(InitialValue != null ? $" = {InitialValue}" : "")}");
            if (Closure.ClosureType != ClosureType.Argument)
            {
                formatter.WriteLine(";");
            }
        }

        public override bool Equals(object obj)
        {
            Variable vr = obj as Variable;
            if (vr != null)
            {
                return vr.Name.Equals(Name) && Type.Equals(vr.Type);
            }
            return base.Equals(obj);
        }

    }
}
