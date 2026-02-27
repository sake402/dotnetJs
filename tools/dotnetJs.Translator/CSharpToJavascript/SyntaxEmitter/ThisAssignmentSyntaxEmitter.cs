using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handle this = value syntax
    sealed class ThisAssignmentSyntaxEmitter : SyntaxEmitter<AssignmentExpressionSyntax>
    {
        public override bool TryEmit(AssignmentExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            //The only time when c# allows this to be assigned is if it is a struct type.
            //We clone the rhs into this
            if (node.Left.IsKind(SyntaxKind.ThisExpression)/* is ThisExpressionSyntax*/&& node.OperatorToken.IsKind(SyntaxKind.EqualsToken))
            {
                visitor.Visit(node.Right);
                visitor.Writer.Write(node, ".Clone(this)");
                return true;
            }
            return false;
        }
    }
}
