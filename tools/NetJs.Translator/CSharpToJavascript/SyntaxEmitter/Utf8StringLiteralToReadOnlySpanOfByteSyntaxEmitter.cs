using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Convert likes of "ABC"u8 to ReadOnlySpan<byte>
    sealed class Utf8StringLiteralToReadOnlySpanOfByteSyntaxEmitter : SyntaxEmitter<LiteralExpressionSyntax>
    {
        public override bool TryEmit(LiteralExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.Utf8StringLiteralExpression))
            {
                var readOnlySpan = (INamedTypeSymbol)visitor.Global.GetTypeSymbol("System.ReadOnlySpan<>", visitor);
                var ssbyte = (ITypeSymbol)visitor.Global.GetTypeSymbol("System.Byte", visitor);
                readOnlySpan = readOnlySpan.Construct(ssbyte);
                var bytes = node.Token.ValueText.ToArray().Select(e => e & 0xFF);
                var constructor = readOnlySpan.GetMembers(".ctor").Cast<IMethodSymbol>().Single(t => t.Parameters.Count() == 1 && t.Parameters[0].Type.IsArray(out _));
                visitor.WriteConstructorCall(node, readOnlySpan, constructor, null, [new CodeNode(() =>
                {
                    //visitor.WriteCreateArray(rhsExpression, SyntaxFactory.ParseTypeName("System.Byte"), () =>
                    //{
                    //    visitor.Writer.Write(rhsExpression,"" );
                    //}, null);
                    visitor.Writer.Write(node,"[");
                    int ix = 0;
                    foreach (var b in bytes)
                    {
                        if (ix > 0)
                            visitor.Writer.Write(node,", ");
                        visitor.Writer.Write(node,b.ToString());
                        ix++;
                    }
                    visitor.Writer.Write(node,"]");
                })]);
                return true;
            }
            return false;
        }
    }
}
