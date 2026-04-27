using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public interface IGeneric : ICSharpClosure
    {
        IEnumerable<Type> GenericTypes { get; set; }
    }
}
