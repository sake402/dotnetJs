using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class GenericUnionClass : GenericClass
    {
        public GenericUnionClass(Namespace @namespace, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class> baseClass = null, bool internalType = false) : base(@namespace, name, access, partial, baseClass, internalType)
        {
        }

        public override bool Equals(object obj)
        {
            GenericClass gClass = obj as GenericClass;
            if (gClass != null && gClass != this && gClass.GenericTypes != null && GenericTypes != null)
            {
                if (gClass.GenericTypes.Count() == GenericTypes.Count())
                {
                    List<Type> g1 = new List<Type>();
                    List<Type> g2 = new List<Type>();
                    g1.AddRange(GenericTypes);
                    g2.AddRange(gClass.GenericTypes);
                    foreach (var t1 in g1.ToList())
                    {
                        foreach (var t2 in g2.ToList())
                        {
                            if (t1.Equals(t2))
                            {
                                g1.Remove(t1);
                                g2.Remove(t2);
                            }
                        }
                    }
                    return g1.Count == 0 && g2.Count == 0;
                }
            }
            return base.Equals(obj);
        }
    }
}