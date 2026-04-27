using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class Property : Member
    {
        public Property(ICSharpClosure closure, string name, AccessSpecifier access, Type type, bool @static = false, bool @readonly = false, bool optional = false, AccessSpecifier get = AccessSpecifier.Public, AccessSpecifier set = AccessSpecifier.Public) : base(closure, name, access, @static, @readonly)
        {
            Type = type;
            IsOptional = optional;
            GetSpecifier = get;
            SetSpecifier = set;
        }

        //public bool ReadOnly { get; private set; }
        public bool IsOptional { get; private set; }
        public Type Type { get; private set; }
        public AccessSpecifier GetSpecifier { get; set; }
        public AccessSpecifier SetSpecifier { get; set; }
        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (comment)
            {
                if (Comment != null)
                {
                    Comment.Write(formatter);
                }
            }
            foreach (var attr in Attributes)
            {
                formatter.WriteLine($"[{attr}]");
            }
            formatter.WriteLine($"{((Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface && Access != AccessSpecifier.Private) ? Access.ToString().ToLower() + " " : "")}{(Closure.ClosureType != ClosureType.Interface && Closure.ClosureType != ClosureType.AnonymousInterface ? /*"extern "*/"virtual " : "")}{(Static ? /*"static "*/"" : "")}{(Abstract ? "abstract " : "")}{Type} {Name.FormatCSharpName()} {{ {(GetSpecifier != AccessSpecifier.Public ? GetSpecifier.ToString().ToLower() + " " : "")}get; {(SetSpecifier != AccessSpecifier.Public ? SetSpecifier.ToString().ToLower() + " " : "")}{(!Readonly ? "set; " : "")}}}");
        }

        public override bool Equals(object obj)
        {
            Property p = obj as Property;
            if (p != null && p != this)
            {
                Object closure1 = Closure as Object;
                Object closure2 = p.Closure as Object;
                if (closure1 != null && closure2 != null)
                    return (closure1.InheritFrom(closure2) || closure2.InheritFrom(closure1) || p.Closure.Equals(Closure)) && (p.Type?.Equals(Type) ?? false) && p.Name.Equals(Name);
            }
            return base.Equals(obj);
        }
    }
}
