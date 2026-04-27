using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Comment : CSharpSyntax
    {
        public Comment(CommentSummary summary = null, CommentReturn @return = null, IEnumerable<CommentParam> @params = null)
        {
            Summary = summary;
            Return = @return;
            Params = new List<CommentParam>();
            if (@params != null)
            {
                Params.AddRange(@params);
            }
        }

        public CommentSummary Summary { get; set; }
        public List<CommentParam> Params { get; set; }
        public CommentReturn Return { get; set; }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (Summary != null || Return != null || (Params != null && Params.Count > 0))
            {
                formatter.WriteLine($"");
                if (Summary != null)
                    Summary.Write(formatter);
                foreach(var m in Params)
                {
                    m.Write(formatter);
                }
                if (Return != null)
                    Return.Write(formatter);
            }
        }
    }
}
