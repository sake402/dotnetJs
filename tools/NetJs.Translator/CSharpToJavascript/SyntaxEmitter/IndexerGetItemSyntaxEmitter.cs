using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handle indexer get_Item eg foo.Item[1]
    sealed class IndexerGetItemSyntaxEmitter : SyntaxEmitter<ElementAccessExpressionSyntax>
    {
        public override bool TryEmit(ElementAccessExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            var targetType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Expression), visitor)?.GetTypeSymbol();
            if (targetType != null)
            {
                var propertyIndexers = targetType.GetMembers("get_Item", visitor.Global).Where(e => e is IMethodSymbol p && p.Parameters.Count() == node.ArgumentList.Arguments.Count).Cast<IMethodSymbol>().ToList();
                //var propertyIndexers = nt.GetMembers("get_Item", _global).Where(e => e is IPropertySymbol p && p.IsIndexer && p.Parameters.Count() == node.ArgumentList.Arguments.Count && p.GetMethod != null).Cast<IPropertySymbol>().ToList();
                var bestIndexer = visitor.GetBestOverloadMethod(targetType, propertyIndexers, null, node.ArgumentList.Arguments, null, out _);
                if (bestIndexer != null)
                {
                    bool isExtern = bestIndexer.IsExtern || visitor.Global.HasAttribute(bestIndexer, typeof(ExternalAttribute).FullName!, visitor, false, out _) ||
                         (bestIndexer.AssociatedSymbol?.IsExtern ?? false) || (bestIndexer.AssociatedSymbol != null && visitor.Global.HasAttribute(bestIndexer.AssociatedSymbol, typeof(ExternalAttribute).FullName!, visitor, false, out _));
                    bool hasTemplate = bestIndexer.GetTemplateAttribute(visitor.Global) != null;
                    if (!isExtern || hasTemplate)
                    {
                        visitor.WriteMethodInvocation(node, bestIndexer, null, node.ArgumentList.Arguments.Select(a => new CodeNode(a)), node.Expression, targetType, null, false);
                        if (bestIndexer.RefKind != RefKind.None)
                        {
                            if (node.Parent.IsKind(SyntaxKind.EqualsEqualsToken) ||
                                node.Parent.IsKind(SyntaxKind.NotEqualsExpression) ||
                                node.Parent.IsKind(SyntaxKind.PostIncrementExpression) ||
                                node.Parent.IsKind(SyntaxKind.PostDecrementExpression) ||
                                node.Parent.IsKind(SyntaxKind.SubtractExpression) ||
                                node.Parent.IsKind(SyntaxKind.AddExpression) ||
                                node.Parent.IsKind(SyntaxKind.MultiplyExpression) ||
                                node.Parent.IsKind(SyntaxKind.DivideExpression) ||
                                node.Parent.IsKind(SyntaxKind.IfStatement)||
                                node.Parent.IsKind(SyntaxKind.WhileStatement))
                            {
                                visitor.Writer.Write(node, $".");
                                visitor.Writer.Write(node, Constants.RefValueName);
                            }
                        }
                        return true;
                    }
                }

            }
            return false;
        }
    }
}
