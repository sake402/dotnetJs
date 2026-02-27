using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public static class RazorUtility
    {
        public static string Escape(ReadOnlySpan<char> t)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                switch (c)
                {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }
            return b.ToString();
        }
    }
}
