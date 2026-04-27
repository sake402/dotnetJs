using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class LookupObjectType : Type
    {
        public LookupObjectType(ICSharpClosure? closure, string name) : base(null, name)
        {
            FromClosure = closure;
        }

        public Type? ReferencedObject { get; set; }
        public ICSharpClosure? FromClosure { get; set; }

        public override string Name
        {
            get
            {
                return ReferencedObject?.Name ?? base.Name;
            }
            /*protected */set
            {
                base.Name = value;
            }
        }

        public override Type UnderlyingType { get { return ReferencedObject??this; } }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (ReferencedObject != null)
            {
                ReferencedObject.Write(formatter, comment);
            }
            else
                base.Write(formatter, comment);
        }

        public override string FullName => ReferencedObject?.FullName ?? base.FullName;

        public override string ToString()
        {
            if (ReferencedObject != null)
            {
                return ReferencedObject.ToString();
            }
            else
                return base.ToString();
        }
    }
}
