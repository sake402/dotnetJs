using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class CommentSummary : CSharpSyntax
    {
        public CommentSummary(string message)
        {
            Message = message;
        }

        public string Message { get;  private set; }
        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.WriteLine($"///<summary>");
            Message.Format(formatter);
            formatter.WriteLine($"///</summary>");
        }
    }
}
