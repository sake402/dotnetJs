using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    sealed class ImplicitConversionSyntaxEmitter : SyntaxEmitter<CSharpSyntaxNode>
    {
        Stack<CSharpSyntaxNode> _processing = new Stack<CSharpSyntaxNode>();
        public override bool TryEmit(CSharpSyntaxNode node, TranslatorSyntaxVisitor visitor)
        {
            if (_processing.TryPeek(out var top) && top == node)
                return false;
            foreach (var sm in visitor.SemanticModels)
            {
                if (node.SyntaxTree == sm.SyntaxTree)
                {
                    var conversion = sm.GetConversion(node);
                    if (conversion.Exists &&
                        conversion.IsImplicit &&
                        conversion.IsUserDefined &&
                        conversion.MethodSymbol != null &&
                        visitor.Global.ShouldExportType(conversion.MethodSymbol.ContainingType, visitor))
                    {
                        _processing.Push(node);
                        try
                        {
                            visitor.WriteMethodInvocation(node, conversion.MethodSymbol, null, [node], null, null, null, false);
                        }
                        finally
                        {
                            _processing.Pop();
                        }
                        //visitor.TryInvokeMethodOperator(node, "op_Implicit", (ITypeSymbol?)lhsType, null, [rhsAsExpression]));
                        return true;
                    }
                    else if (conversion.Exists &&
                        conversion.IsImplicit &&
                        conversion.IsSpan)
                    {
                        var operation = sm.GetOperation(node)?.Parent as IConversionOperation;
                        if (operation != null)
                        {
                            _processing.Push(node);
                            try
                            {
                                var sourceType = operation.Operand.Type!;
                                var spanType = operation.Type!;
                                //var spanType = (INamedTypeSymbol)visitor.Global.GetTypeSymbol("System.Span<>", visitor);
                                //var lhsType = visitor.Global.GetTypeSymbol(node, visitor).GetTypeSymbol();
                                var implicitConverter = spanType.GetMembers("op_Implicit", visitor.Global)
                                    .Cast<IMethodSymbol>()
                                    .FirstOrDefault(e => e.Parameters.Length == 1 && sourceType.CanConvertTo(e.Parameters[0].Type, visitor.Global, null, out _) > 0 && e.ReturnType.Equals(spanType, SymbolEqualityComparer.Default))
                                    ??
                                    sourceType.GetMembers("op_Implicit", visitor.Global)
                                    .Cast<IMethodSymbol>()
                                    .First(e => e.Parameters.Length == 1 && sourceType.CanConvertTo(e.Parameters[0].Type, visitor.Global, null, out _) > 0 && spanType.Equals(e.ReturnType, SymbolEqualityComparer.Default))
                                    ;
                                visitor.WriteMethodInvocation(node, implicitConverter, null, [node], null, null, null, false);
                            }
                            finally
                            {
                                _processing.Pop();
                            }
                            //visitor.TryInvokeMethodOperator(node, "op_Implicit", (ITypeSymbol?)lhsType, null, [rhsAsExpression]));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    //sealed class RefAssignmentSyntaxEmitter : SyntaxEmitter<ExpressionSyntax>
    //{
    //    Stack<ExpressionSyntax> _processing = new Stack<ExpressionSyntax>();
    //    public override bool TryEmit(ExpressionSyntax node, TranslatorSyntaxVisitor visitor)
    //    {
    //        if (_processing.TryPeek(out var top) && top == node)
    //            return false;
    //        foreach (var sm in visitor.SemanticModels)
    //        {
    //            if (node.SyntaxTree == sm.SyntaxTree)
    //            {
    //                var conversion = sm.Get(node);
    //                if (conversion.Exists && conversion.IsImplicit && conversion.IsUserDefined && conversion.MethodSymbol != null)
    //                {
    //                    _processing.Push(node);
    //                    try
    //                    {
    //                        visitor.WriteMethodInvocation(node, conversion.MethodSymbol, null, [node], null, null, null, false);
    //                    }
    //                    finally
    //                    {
    //                        _processing.Pop();
    //                    }
    //                    //visitor.TryInvokeMethodOperator(node, "op_Implicit", (ITypeSymbol?)lhsType, null, [rhsAsExpression]));
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }
    //}
}
