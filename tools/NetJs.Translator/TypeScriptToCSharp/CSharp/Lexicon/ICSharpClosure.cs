using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public interface ICSharpClosure:ICSharpSyntax
    {
        string Name { get; }
        string FullName { get; }
        ClosureType ClosureType { get; }
        void Add(CSharpConstruct construct);
        void Remove(CSharpConstruct construct);
        Type? GetByName(string name);
        ICSharpClosure? Parent { get; }
        int AutoDefinedTypesCount { get; set; }
    }
}
