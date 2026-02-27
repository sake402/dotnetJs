//using CodeLineWriter = System.IO.StringWriter;

using LivingThing.Core.Frameworks.Common.OneOf;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public class CodeNode : OneOfBase<CSharpSyntaxNode, Action>
    {
        public CodeNode(CSharpSyntaxNode input) : base(input)
        {
        }

        public CodeNode(Action input) : base(input)
        {
        }

        //public MethodArgument(string input) : base(input)
        //{
        //}

        public static implicit operator CodeNode(CSharpSyntaxNode? _) => _ == null ? null! : new CodeNode(_);
        public static implicit operator CodeNode(Action? _) => _ == null ? null! : new CodeNode(_);
        //public static implicit operator MethodArgument(string? _) => _ == null ? null! : new MethodArgument(_);
    }
}