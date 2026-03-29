using NetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        static string[] MathBinaryOperators = new string[]
        {
            "+", "-", "*", "/", "%", "==", "!="//, ">>", "<<"
        };

        bool TryWriteMathBinaryExpression(CSharpSyntaxNode node, string _operator, ExpressionSyntax left, ExpressionSyntax right)
        {
            if (MathBinaryOperators.Contains(_operator))
            {
                var leftType = _global.ResolveSymbol(GetExpressionReturnSymbol(left), this)?.GetTypeSymbol();
                var rightType = _global.ResolveSymbol(GetExpressionReturnSymbol(right), this)?.GetTypeSymbol();
                if (leftType != null && rightType != null && leftType.IsNumericType() && rightType.IsNumericType())
                {
                    if (TryInvokeMethodOperator(node, _operator, leftType, left, [left, right]))
                        return true;
                    if ((leftType.Equals(rightType, SymbolEqualityComparer.Default)) || //if both math lhs and rhs are the same, nothing special to do
                        (leftType.IsJsNativeNumeric() && rightType.IsJsNativeNumeric()) // if we handle both types in native javascript, nothing special to do
                        )
                    {
                        Visit(left);
                        Writer.Write(node, " ");
                        Writer.Write(node, _operator);
                        Writer.Write(node, " ");
                        Visit(right);
                        return true;
                    }
                    //lhs and rhs are not the same type, see if we need to cast before invoking the operator
                    //find the one that has highest precision
                    int lhsRank = leftType.GetNumericPrecisionRank();
                    int rhsRank = rightType.GetNumericPrecisionRank();
                    ITypeSymbol higherPrecision = rhsRank > lhsRank ? rightType : leftType;
                    ITypeSymbol lowerPrecision = rhsRank > lhsRank ? leftType : rightType;
                    //Generate a cast for the one from lower precision to high precision
                    CastExpressionSyntax Cast(ExpressionSyntax expression, ITypeSymbol type)
                    {
                        var typeSyntax = SyntaxFactory.ParseTypeName(type.Name);
                        var casted = SyntaxFactory.CastExpression(typeSyntax, expression);
                        return casted;
                    }
                    ExpressionSyntax newLeft = left, newRight = right;
                    bool leftCasted = false;
                    bool rightCasted = false;
                    if (lhsRank < rhsRank)
                    {
                        if (TryInvokeMethodOperator(node, ImplicitOperatorName, higherPrecision, null, [left]))
                        {
                            Writer.Write(node, " ");
                            Writer.Write(node, _operator);
                            Writer.Write(node, " ");
                            Visit(right);
                            return true;
                        }
                        leftCasted = true;
                        newLeft = Cast(left, higherPrecision);
                    }
                    else
                    {
                        if (TryInvokeMethodOperator(node, ImplicitOperatorName, higherPrecision, null, [right], () =>
                        {
                            Visit(left);
                            Writer.Write(node, " ");
                            Writer.Write(node, _operator);
                            Writer.Write(node, " ");
                        }))
                        {
                            return true;
                        }
                        rightCasted = true;
                        newRight = Cast(right, higherPrecision);
                    }
                    //Create a new binary expression from the left right
                    var kind = _operator switch
                    {
                        "+" => SyntaxKind.AddExpression,
                        "-" => SyntaxKind.SubtractExpression,
                        "*" => SyntaxKind.MultiplyExpression,
                        "/" => SyntaxKind.DivideExpression,
                        "%" => SyntaxKind.ModuloExpression,
                        "==" => SyntaxKind.EqualsExpression,
                        "!=" => SyntaxKind.NotEqualsExpression,
                        ">>" => SyntaxKind.RightShiftExpression,
                        "<<" => SyntaxKind.LeftShiftExpression,
                        _ => SyntaxKind.None
                    };
                    var newNode = SyntaxFactory.BinaryExpression(kind, newLeft, newRight);
                    IDisposable? leftDispose = null;
                    IDisposable? rightDispose = null;
                    if (leftCasted && newNode.Left is CastExpressionSyntax castL)
                        leftDispose = AssociateSyntaxFactoryNode(left, castL.Expression);
                    else
                        leftDispose = AssociateSyntaxFactoryNode(left, newNode.Left);
                    if (rightCasted && newNode.Right is CastExpressionSyntax castR)
                        rightDispose = AssociateSyntaxFactoryNode(right, castR.Expression);
                    else
                        rightDispose = AssociateSyntaxFactoryNode(right, newNode.Right);
                    Visit(newNode);
                    leftDispose?.Dispose();
                    rightDispose?.Dispose();
                    return true;
                }
            }
            return false;
        }
    }
}
