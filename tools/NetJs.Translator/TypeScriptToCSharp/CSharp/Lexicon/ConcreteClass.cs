using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class ConcreteClass : Class
    {
        public ConcreteClass(ICSharpClosure closure, string name, AccessSpecifier access = AccessSpecifier.Public, bool partial = true, IEnumerable<Class> baseClass = null) : base(closure, name, access, partial, baseClass)
        {
            //Attributes.Add("External");
        }

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
