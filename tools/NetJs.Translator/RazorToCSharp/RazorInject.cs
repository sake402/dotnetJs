using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.RazorToCSharp
{
    public class RazorInject
    {
        public RazorInject(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"@inject {Type} {Name}";
        }
    }
}
