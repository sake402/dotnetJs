using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class GenericClass : ConcreteClass, IGeneric
    {
        public GenericClass(ICSharpClosure? closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class>? baseClass = null, bool internalType = false) : base(closure, name, access, partial, baseClass)
        {
            _internalType = internalType;
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            if (_internalType)
                return;
            //for classes with constraint that says any, create a class to inherit from this with all the parameters replaced as object
            if (GenericTypes.All(t=> { 
                if (t is GenericType gType)
                {
                    return gType.Constraints.Any(c => c.Name == "object");
                }
                return false;
            }))
            {
                var newClass = new ConcreteClass(Closure, base.Name, Access, Partial, new Class[] { this });
            }
            base.Write(formatter, comment);
        }

        public override void PreConvert()
        {
            if (GenericTypes.Count() > 0)
            {
                if (GenericTypes.All(t =>
                {
                    if (t is GenericType gType)
                    {
                        return gType.Constraints.Count() > 0;// All(c => c.DefaultValue != null);
                    }
                    return false;
                }))
                {
                    var name = Name.Split(new char[] { '<' })[0];
                    var newClass = new ConcreteClass(Closure, name, Access, Partial, new Class[] { new GenericClass(null, name, AccessSpecifier.Public) {
                            GenericTypes = GenericTypes.SelectMany(t=> (t as GenericType).Constraints).ToList(),
                        } })
                    {
                        Methods = Methods.Where(m => m.IsConstructor).Concat(new Method[] {
                            new Method(this, name, AccessSpecifier.Public, null)
                        }).ToList()
                    };
                }
            }
            base.PreConvert();
        }

        bool _internalType;
        public IEnumerable<Type> GenericTypes { get; set; } = new List<Type>();

        string? name;

        public override string Name
        {
            get
            {
                if (GenericTypes == null)
                    return base.Name;
                string genericTypes = string.Join(", ", GenericTypes.Select(cls =>
                {
                    var v = cls?.Name;
                    if (v == "void")
                    {
                        v = "undefined";
                    }
                    return v;
                }));
                if (GenericTypes.Count() > 0)
                {
                    genericTypes = "<" + genericTypes.Trim(new char[] { ',', ' ' }) + ">";
                }
                return name + genericTypes;
            }
            /*protected */set
            {
                if (!value.Trim().EndsWith(">"))
                {
                    name = value;
                }
            }
        }

        //public override string ToString()
        //{
        //    return Name;
        //}
        public override bool Equals(object obj)
        {
            GenericClass? gClass = obj as GenericClass;
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
