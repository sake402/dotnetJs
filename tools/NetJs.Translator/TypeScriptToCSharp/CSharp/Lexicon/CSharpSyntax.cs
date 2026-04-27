using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public abstract class CSharpSyntax : ICSharpSyntax
    {
        public abstract void Write(ICSharpFormatter formatter, bool comment = true);
        public override string ToString()
        {
            StringBuilderFormatter formatter = new StringBuilderFormatter();
            Write(formatter, false);
            return formatter.ToString();
        }

        public virtual void PreConvert() { }
    }
}
