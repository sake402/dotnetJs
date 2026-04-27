using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptToCSharp.CSharp.Formatter;

namespace TypeScriptToCSharp.CSharp.Lexicon
{
    public class MethodDelegate : Method
    {
        public MethodDelegate(ICSharpClosure? closure, string name, Type returnType, IEnumerable<MethodParameter>? parameters = null) : base(closure, name, AccessSpecifier.Public, returnType, false, parameters)
        {
        }

        public override string Name
        {
            get
            {
                if (Parameters == null)
                    return base.Name;
                string @params = string.Join(", ", Parameters.Select(p => p.Type).ToList().ConvertAll(p => p.Name));
                if (Return == null || Return.Name == "void")
                {
                    if (Parameters.Count == 0)
                    {
                        return $"Action";
                    }
                    else
                    {
                        return $"Action<{@params}>";
                    }
                }
                else
                {
                    if (Parameters.Count == 0)
                    {
                        return $"Func<{Return}>";
                    }
                    else
                    {
                        return $"Func<{@params}, {Return}>";
                    }
                }
            }
        }

        public override void Write(ICSharpFormatter formatter, bool comment = true)
        {
            formatter.Write(FullName);
        }
    }
}
