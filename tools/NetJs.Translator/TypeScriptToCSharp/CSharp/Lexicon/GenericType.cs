using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class GenericType:Type
    {
        public GenericType(ICSharpClosure closure, string name, IEnumerable< Type> constraints = null):base(closure, name)
        {
            Constraints = new List<Type>();
            if (constraints != null)
            {
                Constraints.AddRange(constraints);
            }
        }

        public List<Type> Constraints { get; private set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            //formatter.WriteLine($"<Name>");
            //formatter.WriteLine($"where {Name} : {Constraints}");
        }
    }
}
