using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class CommentReturn : CSharpSyntax
    {
        public CommentReturn(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.WriteLine($"///<return>");
            Message.Format(formatter);
            formatter.WriteLine($"///</return>");
        }
    }
}
