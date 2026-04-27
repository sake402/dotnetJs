using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Indexer
{
    //Handle likes of CustomObject[key] = value, CustomObject?[key] = value, Dictionary[key] = value, Span[index] = value, 
    sealed class IndexerSetItemSyntaxEmitter : SyntaxEmitter<CSharpSyntaxNode>
    {
        public override bool TryEmit(CSharpSyntaxNode node, TranslatorSyntaxVisitor visitor)
        {
            if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                AssignmentExpressionSyntax assignment = (AssignmentExpressionSyntax)node;
                bool IsEqualToken()
                {
                    return assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.MinusEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.PercentEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.AmpersandEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.AsteriskEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.SlashEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.BarEqualsToken) ||
                    assignment.OperatorToken.IsKind(SyntaxKind.CaretEqualsToken);
                }
                if ((assignment.Left.IsKind(SyntaxKind.ElementAccessExpression) || assignment.Left.IsKind(SyntaxKind.ElementBindingExpression)) && IsEqualToken())
                {
                    var expression = (assignment.Left as ElementAccessExpressionSyntax)?.Expression;
                    ConditionalAccessExpressionSyntax? conditionalExpression = null;
                    if (expression == null)
                    {
                        conditionalExpression = node.FindClosestParent<ConditionalAccessExpressionSyntax>(isCandidate: e => e.WhenNotNull == node);
                        //conditionalExpression = node.FindClosestParent<ConditionalAccessExpressionSyntax>(isCandidate: e => e.WhenNotNull == node || (e.WhenNotNull is AssignmentExpressionSyntax ass && ass.Left == node));
                        if (conditionalExpression != null)
                            expression = conditionalExpression.Expression;
                    }
                    if (expression != null)
                    {
                        var arguments = (assignment.Left as ElementAccessExpressionSyntax)?.ArgumentList.Arguments ?? (assignment.Left as ElementBindingExpressionSyntax)!.ArgumentList.Arguments;

                        var rhsType = visitor.GetExpressionBoundTarget(assignment.Right).TypeSyntaxOrSymbol as ISymbol;
                        if (rhsType == null)
                        {
                            rhsType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(assignment.Right), visitor/*, out _, out _*/);
                        }
                        var lhsType = visitor.GetExpressionBoundTarget(assignment.Left).TypeSyntaxOrSymbol as ISymbol;
                        if (lhsType == null)
                        {
                            lhsType = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(assignment.Left), visitor/*, out _, out _*/);
                        }
                        var assignmentType = rhsType?.GetTypeSymbol() ?? lhsType?.GetTypeSymbol();

                        //var sourceDeclaration = visitor.GetExpressionReturnSymbol(expression);
                        //var sourceType = visitor.Global.ResolveSymbol(sourceDeclaration, visitor/*, out _, out _*/)?.GetTypeSymbol();
                        //if (sourceType != null)
                        {
                            CodeNode cExpression = expression;
                            if (conditionalExpression != null && visitor.ConditionalAccessUseIfNotNull(conditionalExpression, out _))
                            {
                                cExpression = new CodeNode(() =>
                                {
                                    visitor.CurrentTypeWriter.Write(node, Constants.IfNotNullParameterName);
                                });
                            }
                            var bestIndexer = visitor.GetSetIndexer(assignment.Left is ElementAccessExpressionSyntax ? (ElementAccessExpressionSyntax)assignment.Left : (ElementBindingExpressionSyntax)assignment.Left, assignment.Right);
                            if (bestIndexer != null && bestIndexer.IsInvokable(visitor.Global))
                            {
                                var valueParameter = bestIndexer.Parameters.Last();
                                var box = true;
                                if (valueParameter != null && visitor.Global.HasAttribute(valueParameter, typeof(BoxAttribute).FullName, visitor, false, out var arg))
                                {
                                    box = (bool)arg[0];
                                }
                                visitor.WriteMethodInvocation(node, bestIndexer, null, arguments.Select(a => new CodeNode(a)), cExpression, assignmentType, null, false, suffixArguments: (Action)(() =>
                                {
                                    if (!assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                                    {
                                        visitor.Visit(assignment.Left);
                                        visitor.CurrentTypeWriter.Write(node, " ");
                                        visitor.CurrentTypeWriter.Write(node, assignment.OperatorToken.ValueText.Substring(0, assignment.OperatorToken.ValueText.Length - 1));
                                        visitor.CurrentTypeWriter.Write(node, " ");
                                    }
                                    visitor.WriteVariableAssignment(node, null, lhsType, null, new CodeNode(assignment.Right), rhsType, enableBoxing: box);
                                    //visitor.Visit(node.Right);
                                }));
                                return true;
                            }

                            //var propertyIndexers = sourceType.GetMembers("set_Item", visitor.Global).Where(e => e is IMethodSymbol p && p.Parameters.Count() == arguments.Count + 1).Cast<IMethodSymbol>().ToList();
                            ////var propertyIndexers = nt.GetMembers("set_Item", _global).Where(e => e is IPropertySymbol p && p.IsIndexer && p.Parameters.Count() == elementAccess.ArgumentList.Arguments.Count && p.SetMethod != null).Cast<IPropertySymbol>().ToList();
                            //var bestIndexer = visitor.GetBestOverloadMethod(sourceType, propertyIndexers, null, arguments, assignment.Right, out _);
                            //if (bestIndexer != null && bestIndexer.IsInvokable(visitor.Global))
                            //{
                            //    var valueParameter = bestIndexer.Parameters.Last();
                            //    var box = true;
                            //    if (valueParameter != null && visitor.Global.HasAttribute(valueParameter, typeof(BoxAttribute).FullName, visitor, false, out var arg))
                            //    {
                            //        box = (bool)arg[0];
                            //    }
                            //    visitor.WriteMethodInvocation(node, bestIndexer, null, arguments.Select(a => new CodeNode(a)), cExpression, assignmentType, null, false, suffixArguments: (Action)(() =>
                            //    {
                            //        if (!assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                            //        {
                            //            visitor.Visit(assignment.Left);
                            //            visitor.CurrentTypeWriter.Write(node, " ");
                            //            visitor.CurrentTypeWriter.Write(node, assignment.OperatorToken.ValueText.Substring(0, assignment.OperatorToken.ValueText.Length - 1));
                            //            visitor.CurrentTypeWriter.Write(node, " ");
                            //        }
                            //        visitor.WriteVariableAssignment(node, null, lhsType, null, new CodeNode(assignment.Right), rhsType, enableBoxing: box);
                            //        //visitor.Visit(node.Right);
                            //    }));
                            //    return true;
                            //}
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
            }
            return false;
        }
    }
}
