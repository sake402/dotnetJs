using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.TypeScript
{
    public class TypeScriptParserSettings
    {
        public bool PublicAll { get; set; }
        public bool ResolveConflictingNames { get; set; }
        public string[] Using { get; set; }
        public Dictionary<string, string> RenameType { get; set; }
    }
}
