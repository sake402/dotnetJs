using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
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
                visitor.Writer.Write(node, ".");
                visitor.Writer.Write(node, Constants.RefValueName);
                return true;
            }
            return false;
        }
    }
}
