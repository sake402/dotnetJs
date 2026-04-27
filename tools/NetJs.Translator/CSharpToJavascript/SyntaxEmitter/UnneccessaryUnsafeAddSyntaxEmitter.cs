using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    /// <summary>
    /// And expression like int a = Unsafe.Add(ref first, index); will typically produce let a = $.$spc.System.Runtime.CompilerServices.Unsafe.Add$1(T)(first, index).$v;
    /// Rewrite as let a = first.Get(index), this will be way faster as itdoesnt create the temp reference returned by Unsafe.Add
    /// </summary>
    internal class UnneccessaryUnsafeAddSyntaxEmitter : SyntaxEmitter<ExpressionSyntax>
    {
        public override bool TryEmit(ExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) /*|| node.IsKind(SyntaxKind.EqualsExpression)*/)
            {
                var left = (node as AssignmentExpressionSyntax)?.Left;
                var right = (node as AssignmentExpressionSyntax)?.Right;
                if (left != null && right != null && right.IsKind(SyntaxKind.InvocationExpression) && right.ToString().StartsWith("Unsafe.Add("))
                {
                     var lhsType = visitor.Global.GetTypeSymbol(left, visitor);
                    var leftRefKind = lhsType.GetRefKind() ?? RefKind.None;
                    if (leftRefKind == RefKind.None &&
                        right is InvocationExpressionSyntax inv &&
                        inv.ArgumentList.Arguments.Count == 2)
                    {
                        visitor.Visit(left);
                        visitor.CurrentTypeWriter.Write(node, " = ");
                        visitor.Visit(inv.ArgumentList.Arguments[0]);
                        visitor.CurrentTypeWriter.Write(node, ".GetAt(");
                        visitor.Visit(inv.ArgumentList.Arguments[1]);
                        visitor.CurrentTypeWriter.Write(node, ")");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}