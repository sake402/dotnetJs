using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handles expression like &pointed to create a pointer
    sealed class PointerCreateSyntaxEmitter : SyntaxEmitter<PrefixUnaryExpressionSyntax>
    {
        public override bool TryEmit(PrefixUnaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.AddressOfExpression))
            {
                var operandType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Operand), visitor);
                var operandRefKind = operandType?.GetRefKind();
                //Ref and Pointer uses the same object for abstraction
                //If it is already a ref, no need to create another pointer
                if (operandRefKind != null && operandRefKind != RefKind.None)
                {
                    visitor.Visit(node.Operand);
                }
                else
                {
                    visitor.WriteCreateRef(node, node.Operand);
                }
                return true;
            }
            return false;
        }
    }
}
