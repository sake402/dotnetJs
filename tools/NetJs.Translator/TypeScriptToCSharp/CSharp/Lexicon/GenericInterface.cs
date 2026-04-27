using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class GenericInterface : Interface, IGeneric
    {
        public GenericInterface(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class> baseClass = null) : base(closure, name, access, partial, baseClass)
        {
        }

        public IEnumerable<Type> GenericTypes { get; set; } = new List<Type>();

        string name;

        public override string Name
        {
            get
            {
                if (GenericTypes == null)
                    return base.Name;
                string t = string.Join(", ", GenericTypes.Select(tt =>
                {
                    var v = tt.Name;
                    if (v == "void")
                    {
                        v = "undefined";
                    }
                    return v;
                }));
                if (!string.IsNullOrEmpty(t))
                {
                    t = "<" + t + ">";
                }
                return name + t;
            }
            /*protected */set
            {
                name = value;
            }
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            string genericTypes = string.Join(", ", GenericTypes.Select(cls => cls.Name));
            if (GenericTypes.Count() > 0)
            {
                genericTypes = "<" + genericTypes + ">";
            }

            base.Write(formatter);
        }
    }
}
