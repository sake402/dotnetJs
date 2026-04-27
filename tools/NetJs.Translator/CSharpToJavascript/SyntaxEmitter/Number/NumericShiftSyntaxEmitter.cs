using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Numbers
{
    //Js Number can handle 53 bits maximum. When we right/left shift though, the value is first converted to 32 bit
    //We use a native BigInt to do the job
    sealed class NumericShiftSyntaxEmitter : SyntaxEmitter<BinaryExpressionSyntax>
    {
        public override bool TryEmit(BinaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.RightShiftExpression) ||
                node.IsKind(SyntaxKind.LeftShiftExpression) ||
                node.IsKind(SyntaxKind.UnsignedRightShiftAssignmentExpression))
            {
                var lhsType = visitor.Global.GetTypeSymbol(node.Left, visitor).GetTypeSymbol();
                var rhsType = visitor.Global.GetTypeSymbol(node.Right, visitor).GetTypeSymbol();
                if (lhsType.IsNumericType() && rhsType.IsNumericType())
                {
                    if (lhsType.IsLongNumericType())
                    {
                        //var isUnsigned = lhsType.IsUnsignedNumericType();
                        //var op = node.OperatorToken.ValueText;
                        //if (isUnsigned && op == ">>")
                        //{
                        //    op = ">>>";
                        //}
                        visitor.CurrentTypeWriter.Write(node, visitor.Global.GlobalName);
                        visitor.CurrentTypeWriter.Write(node, ".");
                        visitor.CurrentTypeWriter.Write(node, Constants.NumericShift);
                        visitor.CurrentTypeWriter.Write(node, "(");
                        visitor.Visit(node.Left);
                        visitor.CurrentTypeWriter.Write(node, ", \"");
                        visitor.CurrentTypeWriter.Write(node, node.OperatorToken.ValueText);
                        visitor.CurrentTypeWriter.Write(node, "\", ");
                        visitor.Visit(node.Right);
                        visitor.CurrentTypeWriter.Write(node, ")");
                        return true;
                    }
                    else if (lhsType.IsUnsignedNumericType() && node.IsKind(SyntaxKind.RightShiftExpression)) //uint >> 5 should always result in a 32 bit uint
                    {
                        visitor.Visit(node.Left);
                        visitor.CurrentTypeWriter.Write(node, " >>> ");
                        visitor.Visit(node.Right);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
