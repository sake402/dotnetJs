using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Formatter
{
    public class StringBuilderFormatter : ICSharpFormatter
    {
        public StringBuilderFormatter()
        {
            Builder = new StringBuilder();
            tab = 0;
        }

        StringBuilder Builder { get; set; }
        int tab;
        bool lined = false;

        public void Write(string line)
        {
            if (lined)
            {
                lined = false;
                int dtab = tab;
                while (dtab-- != 0)
                {
                    Builder.Append("\t");
                }
            }
            Builder.Append(line);
        }

        public void WriteLine(string line)
        {
            if (line == "}")
            {
                if (tab > 0)
                    tab--;
            }
            int dtab = tab;
            while (dtab-- != 0)
            {
                Builder.Append("\t");
            }
            Builder.Append(line);
            Builder.AppendLine();
            lined = true;
            if (line.EndsWith("{") && !line.Trim().StartsWith("///"))
            {
                tab++;
            }
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}
