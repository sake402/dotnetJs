using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class EnumeratedClass : ConcreteClass
    {
        public EnumeratedClass(ICSharpClosure @namespace, string name, AccessSpecifier access = AccessSpecifier.Public, IEnumerable<Class> baseClass = null) : base(@namespace, name, access, false, baseClass)
        {
        }

        public List<Type> EnumeratedTypes { get; } = new List<Type>();
        public override void Add(CSharpConstruct construct)
        {
            if (construct is Type type)
            {
                if (!EnumeratedTypes.Contains(type))
                    EnumeratedTypes.Add(type);
            }
            base.Add(construct);
        }

        public override void PreConvert()
        {
            //add implicit converter
            foreach (var io in InnerObjects.ToList().Concat(EnumeratedTypes.ToList()))
            {
                new Method(this, "", AccessSpecifier.Public, new ImplicitType(null, Name), true, new List<MethodParameter>() {
                    new MethodParameter(null, io.UnderlyingType, "value")
                })
                {
                    Body = $"{{ return new {Name}(value); }}"
                };
            }
            new Method(this, Name, AccessSpecifier.Public, null, false, new List<MethodParameter>() {
                    new MethodParameter(null, new Type(this, "object"), "value")
                })
            {
                Body = $"{{ Value = value; }}"
            };
            Extends.Add(new Type(null, "Enumerated"));
        }

        //public override string Designator => "static class";
        //public List<LiteralClass> Literals { get; set; }

        //public override bool Equals(object obj)
        //{
        //    EnumeratedClass gClass = obj as EnumeratedClass;
        //    if (gClass != null && gClass != this)
        //    {
        //        if (gClass.Literals.Count == Literals.Count)
        //        {
        //            List<Type> g1 = new List<Type>();
        //            List<Type> g2 = new List<Type>();
        //            g1.AddRange(Literals);
        //            g2.AddRange(gClass.Literals);
        //            foreach (var t1 in g1.ToList())
        //            {
        //                foreach (var t2 in g2.ToList())
        //                {
        //                    if (t1.Equals(t2))
        //                    {
        //                        g1.Remove(t1);
        //                        g2.Remove(t2);
        //                    }
        //                }
        //            }
        //            return g1.Count == 0 && g2.Count == 0;
        //        }
        //    }
        //    return base.Equals(obj);
        //}
    }
}