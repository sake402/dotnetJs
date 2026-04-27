using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class AbstractClass : Class
    {
        public AbstractClass(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Private, bool partial = true, IEnumerable<Class> baseClass = null) : base(closure, name, access, partial, baseClass)
        {
        }

        public override string Designator => $"abstract{(Partial ? " partial" : "")} class";

        public override string Definition => $"{(Access != AccessSpecifier.Private ? Access.ToString().ToLower() + " " : "")}{Designator} {Name}";

        public override void PreConvert()
        {
            //add a default constructor
            new Method(this, Name.Split(new char[] { '<' })[0], AccessSpecifier.Public, null);
            //var baseClass = Extends;
            //var constructors = baseClass.SelectMany(b => (b as Class)?.Methods?.Where(m => m.IsConstructor) ?? new Method[] { });
            //Methods.AddRange(constructors);
            base.PreConvert();
        }
    }
}
