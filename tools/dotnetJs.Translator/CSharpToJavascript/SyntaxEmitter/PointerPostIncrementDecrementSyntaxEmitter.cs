using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
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
                    if (node.Parent is AssignmentExpressionSyntax || node.Parent.IsKind(SyntaxKind.EqualsExpression) || node.Parent.IsKind(SyntaxKind.Argument))
                    {
                        visitor.WrapStatementsInExpression(node, () =>
                        {
                            visitor.Writer.Write(node, "var $oldp = ", true);
                            visitor.Visit(node.Operand);
                            visitor.Writer.WriteLine(node, ";");
                            visitor.Writer.Write(node, "", true);
                            visitor.WritePointerSelfAdvance(node, node.Operand, new CodeNode(() =>
                            {
                                visitor.Writer.Write(node, node.IsKind(SyntaxKind.PostIncrementExpression) ? "1" : "-1");
                            }));
                            visitor.Writer.WriteLine(node, ";");
                            visitor.Writer.WriteLine(node, "return $oldp;", true);
                        });
                    }
                    else
                    {
                        visitor.WritePointerSelfAdvance(node, node.Operand, new CodeNode(() =>
                        {
                            visitor.Writer.Write(node, node.IsKind(SyntaxKind.PostIncrementExpression) ? "1" : "-1");
                        }));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
