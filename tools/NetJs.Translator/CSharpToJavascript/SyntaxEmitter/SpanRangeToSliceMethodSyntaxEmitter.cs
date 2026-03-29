using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handles an index operator on a ReadOnlySpan and rewrite it as span[range] => span.Slice(range.Start, range.Length)
    sealed class SpanRangeToSliceMethodSyntaxEmitter : SyntaxEmitter<ElementAccessExpressionSyntax>
    {
        public override bool TryEmit(ElementAccessExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            var type = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Expression), visitor)?.GetTypeSymbol();
            if (type != null)
            {
                if (node.ArgumentList.Arguments.Count == 1)
                {
                    var arg = node.ArgumentList.Arguments[0];
                    var argType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(arg), visitor)?.GetTypeSymbol();
                    if (argType != null)
                    {
                        var range = (ITypeSymbol)visitor.Global.GetTypeSymbol("System.Range", visitor);
                        if (argType.Equals(range, SymbolEqualityComparer.Default))
                        {
                            var readOnlySpan = (ITypeSymbol)visitor.Global.GetTypeSymbol("System.ReadOnlySpan<>", visitor);
                            var span = (ITypeSymbol)visitor.Global.GetTypeSymbol("System.Span<>", visitor);
                            var sint = (ITypeSymbol)visitor.Global.GetTypeSymbol("System.Int32", visitor);
                            ITypeSymbol? dSpan = null;
                            if (type.OriginalDefinition.Equals(readOnlySpan.OriginalDefinition, SymbolEqualityComparer.Default))
                            {
                                dSpan = readOnlySpan;
                            }
                            else if (type.OriginalDefinition.Equals(readOnlySpan.OriginalDefinition, SymbolEqualityComparer.Default))
                            {
                                dSpan = span;
                            }
                            if (dSpan != null)
                            {
                                var sliceMethod = (IMethodSymbol)dSpan.GetMembers("Slice").Single(e => e is IMethodSymbol m && m.Parameters.Count() == 2 && m.Parameters[0].Type.SpecialType == SpecialType.System_Int32 && m.Parameters[1].Type.SpecialType == SpecialType.System_Int32);

                                visitor.Writer.WriteLine(node, $"/*{node}*/ {visitor.Global.GlobalName}.{Constants.Expression}(function()");
                                visitor.Writer.WriteLine(node, "{", true);
                                visitor.Writer.Write(node, $"var $s = ", true);
                                visitor.Visit(node.Expression);
                                visitor.Writer.WriteLine(node, $";");

                                visitor.Writer.Write(node, $"var $i = ", true);
                                visitor.Visit(arg);
                                visitor.Writer.Write(node, ".");
                                visitor.WriteMemberName(node, range, "GetOffsetAndLength");
                                visitor.Writer.Write(node, $"($s.");
                                visitor.WriteMemberName(node, dSpan, "Length");
                                visitor.Writer.WriteLine(node, $");");

                                var source = SyntaxFactory.IdentifierName("$s");
                                var index = SyntaxFactory.IdentifierName("$i");
                                var start = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, index, SyntaxFactory.IdentifierName("Item1"));
                                var length = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, index, SyntaxFactory.IdentifierName("Item2"));
                                var disposeSource = visitor.CurrentClosure.DefineIdentifierType("$s", CodeSymbol.From(new GeneratedLocalSymbol(dSpan, "$s")));
                                var disposeIndex = visitor.CurrentClosure.DefineIdentifierType("$i", CodeSymbol.From(new GeneratedLocalSymbol(visitor.Global.Compilation.CreateTupleTypeSymbol([sint, sint]), "$i")));
                                visitor.Writer.Write(node, $"return ", true);
                                visitor.WriteMethodInvocation(node, sliceMethod, null, [start, length], source, dSpan, null, false);
                                visitor.Writer.WriteLine(node, $";");
                                disposeSource.Dispose();
                                disposeIndex.Dispose();
                                visitor.Writer.Write(node, "})", true);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
