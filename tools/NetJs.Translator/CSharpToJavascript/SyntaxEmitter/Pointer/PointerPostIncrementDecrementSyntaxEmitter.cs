using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Pointer
{
    //Handles expression like p++, p-- where p is a pointer
    sealed class PointerPostIncrementDecrementSyntaxEmitter : SyntaxEmitter<PostfixUnaryExpressionSyntax>
    {
        public override bool TryEmit(PostfixUnaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.PostIncrementExpression) || node.IsKind(SyntaxKind.PostDecrementExpression))
            {
                var operandType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Operand), visitor)!.GetTypeSymbol();
                if (operandType.IsPointer(out var pointerType))
                {
                    //if the result of the expression is passed to other variables
                    if (node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                        node.Parent.IsKind(SyntaxKind.EqualsExpression) ||
                        node.Parent.IsKind(SyntaxKind.Argument) ||
                        node.Parent.IsKind(SyntaxKind.PointerIndirectionExpression))
                    {
                        visitor.WrapStatementsInExpression(node, () =>
                        {
                            visitor.CurrentTypeWriter.Write(node, "var $oldp = ", true);
                            visitor.Visit(node.Operand);
                            visitor.CurrentTypeWriter.WriteLine(node, ";");
                            visitor.CurrentTypeWriter.Write(node, "", true);
                            visitor.WritePointerSelfAdvance(node, node.Operand, new CodeNode(() =>
                            {
                                visitor.CurrentTypeWriter.Write(node, node.IsKind(SyntaxKind.PostIncrementExpression) ? "1" : "-1");
                            }));
                            visitor.CurrentTypeWriter.WriteLine(node, ";");
                            visitor.CurrentTypeWriter.WriteLine(node, "return $oldp;", true);
                        });
                    }
                    else
                    {
                        visitor.WritePointerSelfAdvance(node, node.Operand, new CodeNode(() =>
                        {
                            visitor.CurrentTypeWriter.Write(node, node.IsKind(SyntaxKind.PostIncrementExpression) ? "1" : "-1");
                        }));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
