using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class GenericAbstractClass : AbstractClass, IGeneric
    {
        public GenericAbstractClass(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class> baseClass = null) : base(closure, name, access, partial, baseClass)
        {
            GenericTypes = new List<Type>();
        }

        public IEnumerable<Type> GenericTypes { get; set; }

        string name;

        public override string Name
        {
            get
            {
                string genericTypes = string.Join(", ", GenericTypes.Select(cls => cls.Name));
                if (GenericTypes.Count() > 0)
                {
                    genericTypes = "<" + genericTypes + ">";
                }
                return name + genericTypes;
            }
            /*protected */set
            {
                name = value;
            }
        }

        public override bool Equals(object obj)
        {
            GenericClass gClass = obj as GenericClass;
            if (gClass != null)
            {
                if (Name.Equals(gClass.Name))
                {
                    if (gClass.GenericTypes.Count() == GenericTypes.Count())
                    {
                        foreach (var t1 in GenericTypes)
                        {
                            foreach (var t2 in gClass.GenericTypes)
                            {
                                if (!t1.Equals(t2))
                                    return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return base.Equals(obj);
        }
    }
}
