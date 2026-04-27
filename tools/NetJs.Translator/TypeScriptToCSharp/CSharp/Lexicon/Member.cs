using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public abstract class Member : Type
    {
        public Member(ICSharpClosure? closure, string name, AccessSpecifier access, bool @static = false, bool @readonly = false, Comment? comment = null):base(closure, name)
        {
            Access = access;
            Static = @static;
            Readonly = @readonly;
            Comment = comment;
            Attributes = new List<string>();
            if (name != Name)
            {
                //Attributes.Add($"Name(\"{name}\")");
            }
        }

        public List<string> Attributes { get; set; }
        public bool Static { get; set; }
        public bool Readonly { get; set; }
        public bool Abstract { get; set; }
        public AccessSpecifier Access { get; private set; }
        public Comment? Comment { get; set; }
    }
}
