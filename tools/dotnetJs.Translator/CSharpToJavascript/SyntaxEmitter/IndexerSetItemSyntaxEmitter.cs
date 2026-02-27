using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handle likes of CustomObject[key] = value, Dictionary[key] = value, Span[index] = value;
    sealed class IndexerSetItemSyntaxEmitter : SyntaxEmitter<AssignmentExpressionSyntax>
    {
        public override bool TryEmit(AssignmentExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            bool IsEqualToken()
            {
                return node.OperatorToken.IsKind(SyntaxKind.EqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.MinusEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.PercentEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.AmpersandEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.AsteriskEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.SlashEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.BarEqualsToken) ||
                node.OperatorToken.IsKind(SyntaxKind.CaretEqualsToken);
            }
            if (node.Left is ElementAccessExpressionSyntax elementAccess && IsEqualToken())
            {
                var rhsType = visitor.GetExpressionBoundTarget(node.Right).TypeSyntaxOrSymbol as ISymbol;
                if (rhsType == null)
                {
                    rhsType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Right), visitor/*, out _, out _*/);
                }
                var lhsType = visitor.GetExpressionBoundTarget(node.Left).TypeSyntaxOrSymbol as ISymbol;
                if (lhsType == null)
                {
                    lhsType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Left), visitor/*, out _, out _*/);
                }
                var assignmentType = rhsType?.GetTypeSymbol() ?? lhsType?.GetTypeSymbol();

                var source = elementAccess.Expression;
                var sourceDeclaration = visitor.GetExpressionReturnSymbol(elementAccess.Expression);
                var sourceType = visitor.Global.ResolveSymbol(sourceDeclaration, visitor/*, out _, out _*/)?.GetTypeSymbol();
                if (sourceType != null)
                {
                    var propertyIndexers = sourceType.GetMembers("set_Item", visitor.Global).Where(e => e is IMethodSymbol p && p.Parameters.Count() == elementAccess.ArgumentList.Arguments.Count + 1).Cast<IMethodSymbol>().ToList();
                    //var propertyIndexers = nt.GetMembers("set_Item", _global).Where(e => e is IPropertySymbol p && p.IsIndexer && p.Parameters.Count() == elementAccess.ArgumentList.Arguments.Count && p.SetMethod != null).Cast<IPropertySymbol>().ToList();
                    var bestIndexer = visitor.GetBestOverloadMethod(sourceType, propertyIndexers, null, elementAccess.ArgumentList.Arguments, node.Right, out _);
                    if (bestIndexer != null && bestIndexer.CanInvoke(visitor.Global))
                    {
                        var args = elementAccess.ArgumentList.Arguments;
                        visitor.WriteMethodInvocation(node, bestIndexer, null, elementAccess.ArgumentList.Arguments.Select(a => new CodeNode(a)), elementAccess.Expression, assignmentType, null, false, suffixArguments: (Action)(() =>
                        {
                            if (!node.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                            {
                                visitor.Visit(node.Left);
                                visitor.Writer.Write(node, " ");
                                visitor.Writer.Write(node, node.OperatorToken.ValueText.Substring(0, node.OperatorToken.ValueText.Length - 1));
                                visitor.Writer.Write(node, " ");
                            }
                            visitor.Visit(node.Right);
                        }));
                        return true;
                    }

                    //check if we have a get_Item that return a ref type in the source
                    //propertyIndexers = sourceType.GetMembers("get_Item", _global).Where(e => e is IMethodSymbol p && p.Parameters.Count() == elementAccess.ArgumentList.Arguments.Count).Cast<IMethodSymbol>().ToList();
                    //bestIndexer = GetBestOverloadMethod(sourceType, propertyIndexers, null, elementAccess.ArgumentList.Arguments, node.Right, out _);
                    //if (bestIndexer != null && bestIndexer.CanInvoke(_global))
                    //{
                    //    if (bestIndexer.ReturnsByRef)
                    //    {
                    //        Visit(node.Left);
                    //        Writer.Write(node, ".");
                    //        Writer.Write(node, Constants.RefValueName);
                    //        if (node.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                    //            Writer.Write(node, $" {node.OperatorToken.ValueText} ");
                    //        else
                    //        {
                    //            //a+=b becomes  a.$v = a.$v+b;
                    //            Writer.Write(node, $" = ");
                    //            Visit(node.Left);
                    //            Writer.Write(node, ".");
                    //            Writer.Write(node, Constants.RefValueName);
                    //            Writer.Write(node, " ");
                    //            Writer.Write(node, node.OperatorToken.ValueText.Substring(0, node.OperatorToken.ValueText.Length - 1));
                    //            Writer.Write(node, " ");
                    //        }
                    //        Visit(node.Right);
                    //        return;
                    //    }
                    //}
                }
            }
            return false;
        }
    }
}
