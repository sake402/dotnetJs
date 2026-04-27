using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Pointer
{
    //Handles expression like *pointer where pointer ia a pointer type
    sealed class PointerDereferenceSyntaxEmitter : SyntaxEmitter<PrefixUnaryExpressionSyntax>
    {
        public override bool TryEmit(PrefixUnaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.PointerIndirectionExpression))
            {
                //pointer dereference
                visitor.Visit(node.Operand);
                visitor.TryDereference(node);
                return true;
            }
            return false;
        }
    }
}
