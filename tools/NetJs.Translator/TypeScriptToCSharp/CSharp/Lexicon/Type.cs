using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Type : CSharpConstruct
    {
        public Type(ICSharpClosure? closure, string name, Type? alias = null) : base(closure, name)
        {
            Alias = alias;
        }
        public Type? Alias { get; set; }
        public virtual Type UnderlyingType { get { return this; } }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (Alias != null)
            {
                //Alias.Write(formatter);
            }
            else
            {
                base.Write(formatter, comment);
            }
        }
    }
}
