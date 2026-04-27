using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Formatter
{
    public interface ICSharpFormatter
    {
        void Write(string line);
        void WriteLine(string line);
    }
}
