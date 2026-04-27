using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class MethodParameter : Variable
    {
        public MethodParameter(ICSharpClosure closure, Type type, string name, string initialValue = null, bool isOptional = false, bool defaultParameter = false, bool isParams = false) : base(closure, type, name, AccessSpecifier.Private, false, false, initialValue)
        {
            Optional = isOptional;
            IsParams = isParams;
        }

        public bool IsParams { get; private set; }
        //public bool IsLastParameter { get; set; }
        public bool Optional { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (!IsParams)
            {
                if (Optional)
                {
                    //InitialValue = Type.DefaultValue;
                }
                base.Write(formatter, comment);
                //if (!IsLastParameter)
                //{
                //    formatter.Write(", ");
                //}
            }
            else
            {
                formatter.Write($"params {Type} {Name}");
            }
        }
    }
}
