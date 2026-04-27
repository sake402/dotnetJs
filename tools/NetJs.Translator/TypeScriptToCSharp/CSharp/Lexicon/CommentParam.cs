using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class CommentParam : CSharpSyntax
    {
        public CommentParam(string name, string message)
        {
            Name = name;
            Message = message.Trim();
        }

        public string Name { get; private set; }
        public string Message { get; private set; }
        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.WriteLine($"///<param name=\"{Name}\">");
            Message.Format(formatter);
            formatter.WriteLine($"///</param>");
        }
    }
}
