using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorTemplateTypeName
    {
        public RazorTemplateTypeName(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public override string ToString()
        {
            return $"@typeparam {Type}";
        }
    }
}
