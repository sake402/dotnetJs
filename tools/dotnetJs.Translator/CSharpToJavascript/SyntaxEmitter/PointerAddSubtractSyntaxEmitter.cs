using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handles expression like p+2, p-2 where p is a pointer
    sealed class PointerAddSubtractSyntaxEmitter : SyntaxEmitter<BinaryExpressionSyntax>
    {
        public override bool TryEmit(BinaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.AddAssignmentExpression) || node.IsKind(SyntaxKind.SubtractAssignmentExpression))
            {
                var operandType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Left), visitor)!.GetTypeSymbol();
                if (operandType.IsPointer(out var pointerType))
                {
                    visitor.WritePointerAdvance(node, node.Left, node.Right);
                    return true;
                }
            }
            return false;
        }
    }
}
