using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class ArrayDataType : Type
    {
        public ArrayDataType(ICSharpClosure? closure, Type type) : base(closure, type.Name + "[]")
        {
            Type = type;
        }

        public Type Type { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.Write($"{Name}");
        }
    }
}
