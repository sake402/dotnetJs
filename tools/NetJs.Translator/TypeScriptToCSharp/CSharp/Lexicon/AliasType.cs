using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class AliasType : Class
    {
        static ICSharpClosure RootClosure(ICSharpClosure closure)
        {
            while (closure.Parent != null)
                return RootClosure(closure.Parent);
            return closure;
        }
        public AliasType(ICSharpClosure closure, string name, Type alias, IEnumerable<GenericType> genericTypes) : base(closure, name)
        {
            Alias = alias;
            GenericTypes = genericTypes;
        }

        IEnumerable<GenericType> GenericTypes { get; }
        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (Alias.Name != Name)
            {
                string g = "";
                if (GenericTypes.Count() > 0)
                {
                    g = "<" + string.Join(", ", GenericTypes.Select(gg => gg.ToString())) + ">";
                }
                formatter.WriteLine($"public class {Name}{g} : TypeAlias");
                formatter.WriteLine($"{{");
                formatter.WriteLine($"public {Name}({Alias.Name} value) {{ Value = value; }}");
                formatter.WriteLine($"public static implicit operator {Name}{g}({Alias.Name} value){{ return new {Name}{g}(value); }}");
                formatter.WriteLine($"}}");
            }
            //base.Write(formatter, comment);
        }
    }
}
