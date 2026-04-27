using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class AnonymousInterface : Interface
    {
        public AnonymousInterface(ICSharpClosure closure, string name) : base(closure, name, AccessSpecifier.Public)
        {
            this.name = name;
        }

        public Type Return
        {
            get
            {
                if (Methods.Count > 0)
                    return Methods[0].Return;
                return Properties[0].Type;
            }
        }

        public override string Designator => "interface";

        public override ClosureType ClosureType => ClosureType.AnonymousInterface;

        string name;
        public override string Name { get => !string.IsNullOrEmpty(name) ? name : "Anonymous_" + GetHashCode(); set => name = value; }

        //public override void Add(CSharpConstruct construct)
        //{
        //    if (construct is Object @object)
        //    {
        //        InnerObjects.Add(@object);
        //    }
        //    else if (construct is Method method)
        //    {
        //        Methods.Add(method);
        //    }
        //    else if (construct is Property property)
        //    {
        //        Properties.Add(property);
        //    }
        //}

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
