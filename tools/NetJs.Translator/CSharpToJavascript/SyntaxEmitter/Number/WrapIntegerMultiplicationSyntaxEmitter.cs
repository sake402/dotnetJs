using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Numbers
{
    sealed class WrapIntegerMultiplicationSyntaxEmitter : SyntaxEmitter<BinaryExpressionSyntax>
    {
        public override bool TryEmit(BinaryExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.MultiplyExpression))
            {
                var lhsType = visitor.Global.TryGetTypeSymbol(node.Left, visitor)?.GetTypeSymbol();
                var rhsType = visitor.Global.TryGetTypeSymbol(node.Left, visitor)?.GetTypeSymbol();
                if (lhsType != null && rhsType != null && lhsType.IsJsNativeIntegerNumeric() && rhsType.IsJsNativeIntegerNumeric())
                {
                    bool isSigned = lhsType.IsSignedNumericType() || rhsType.IsSignedNumericType();
                    visitor.CurrentTypeWriter.Write(node, visitor.Global.GlobalName);
                    visitor.CurrentTypeWriter.Write(node, ".$wrap(");
                    visitor.Visit(node.Left);
                    visitor.CurrentTypeWriter.Write(node, " * ");
                    visitor.Visit(node.Right);
                    visitor.CurrentTypeWriter.Write(node, ", ");
                    visitor.CurrentTypeWriter.Write(node, isSigned ? "1" : "0");
                    visitor.CurrentTypeWriter.Write(node, ")");
                    return true;
                }
            }
            return false;
        }
    }
}
