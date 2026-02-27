using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.RazorToCSharp
{
    public class RazorComponent
    {
        public RazorInherit? Inherit { get; set; }
        public RazorNamespace? Namespace { get; set; }
        public RazorLayout? Layout { get; set; }
        public List<RazorPage> Routes { get; } = new List<RazorPage>();
        public List<RazorUsing> Usings { get; } = new List<RazorUsing>();
        public List<RazorAttribute> Attributes { get; } = new List<RazorAttribute>();
        public List<RazorTemplateTypeName> TemplateTypes { get; } = new List<RazorTemplateTypeName>();
        public List<RazorInject> Injects { get; } = new List<RazorInject>();
        public List<RazorXmlNode> RootNodes { get; } = new List<RazorXmlNode>();

        public override string ToString()
        {
            return @$"
{Inherit}
{string.Join("\r\n", Usings.Select(u => u.ToString()))}
{string.Join("\r\n", TemplateTypes.Select(u => u.ToString()))}
{string.Join("\r\n", Injects.Select(u => u.ToString()))}
{string.Join("\r\n", RootNodes.Select(u => u.ToString()))}
";
        }
    }
}
