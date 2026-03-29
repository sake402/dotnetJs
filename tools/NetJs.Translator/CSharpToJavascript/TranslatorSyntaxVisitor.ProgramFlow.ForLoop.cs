using NetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        void WriteForEach(CommonForEachStatementSyntax node, object variable)
        {
            OpenClosure(node);
            var enumerableTypeSymbol = (INamedTypeSymbol)_global.GetTypeSymbol("System.Collections.Generic.IEnumerable<>", this/*, out _, out _*/);
            var enumerableGetEnumerator = (IMethodSymbol)(enumerableTypeSymbol.GetMembers("GetEnumerator", _global).First());
            var enumerableGetEnumeratorMethodMetadata = _global.GetRequiredMetadata(enumerableGetEnumerator);
            string getEnumeratorInvocationName = enumerableGetEnumeratorMethodMetadata.InvocationName ?? enumerableGetEnumerator.Name;

            var enumerationRhsType = GetExpressionReturnSymbol(node.Expression);
            var enumerationTargetRhsTypeSymbol = _global.ResolveSymbol(enumerationRhsType, this/*, out _, out _*/)?.GetTypeSymbol();

            ITypeSymbol? enumerableItemSymbol = null;
            string? enumeratorMoveNextInvocationName = null;
            string? enumeratorCurrentInvocationName = null;

            if (enumerationTargetRhsTypeSymbol != null)
            {
                //enumerableTypeSymbol = enumerableTypeSymbol.Construct([enumerationTargetRhsTypeSymbol]);
                //If we can get the enumerator directly on the target, we dont need to call the interface method
                //This is espcially useful if the target doesn't actually implement the IEnumerable Interface
                var directGetEnumerator = enumerationTargetRhsTypeSymbol.TypeKind != TypeKind.Interface ?
                    (IMethodSymbol?)(enumerationTargetRhsTypeSymbol.GetMembers("GetEnumerator", _global).FirstOrDefault()) : null;
                if (directGetEnumerator != null)
                {
                    //var directGetENumeratorMetadata = _global.GetRequiredMetadata(directGetEnumerator);
                    //getEnumeratorInvocationName = directGetENumeratorMetadata.InvocationName ?? directGetEnumerator.Name;
                    getEnumeratorInvocationName = "GetEnumerator";
                    enumeratorMoveNextInvocationName = "MoveNext";
                    enumeratorCurrentInvocationName = "Current";
                }
                else
                {
                    if (enumerationTargetRhsTypeSymbol.IsArray(out var elementType))
                    {
                        enumerableItemSymbol = elementType;
                    }
                    else if (!enumerationTargetRhsTypeSymbol.IsEnumerable(out _) && !enumerationTargetRhsTypeSymbol.IsEnumerable())
                    {
                        var ienumerable = enumerationTargetRhsTypeSymbol.AllInterfaces.FirstOrDefault(o => o.IsEnumerable(out _)) ??
                            enumerationTargetRhsTypeSymbol.AllInterfaces.First(o => o.IsEnumerable());
                        enumerableItemSymbol = ienumerable.TypeArguments.FirstOrDefault() ?? _global.Compilation.ObjectType;
                    }
                    else if (enumerationTargetRhsTypeSymbol.IsEnumerable())
                    {
                        enumerableItemSymbol = _global.Compilation.ObjectType;
                    }
                    else
                    {
                        enumerableItemSymbol = ((INamedTypeSymbol)enumerationTargetRhsTypeSymbol).TypeArguments[0];
                    }
                    enumerableTypeSymbol = enumerableTypeSymbol.Construct([enumerableItemSymbol]);

                    var enumeratorSymbol = (INamedTypeSymbol)_global.GetTypeSymbol("System.Collections.Generic.IEnumerator<>", this/*, out _, out _*/)!.GetTypeSymbol();
                    if (enumerableItemSymbol != null)
                    {
                        enumeratorSymbol = enumeratorSymbol.Construct([enumerableItemSymbol]);
                    }
                    var enumeratorMoveNext = (IMethodSymbol)(enumeratorSymbol.GetMembers("MoveNext", _global).First());
                    var enumeratorMoveNextMethodMetadata = _global.GetRequiredMetadata(enumeratorMoveNext);
                    enumeratorMoveNextInvocationName = enumeratorMoveNextMethodMetadata.InvocationName ?? enumeratorMoveNext.Name;

                    var enumeratorCurrent = (IPropertySymbol)(enumeratorSymbol.GetMembers("Current", _global).First());
                    var enumeratorCurrentMethodMetadata = _global.GetRequiredMetadata(enumeratorCurrent);
                    enumeratorCurrentInvocationName = enumeratorCurrentMethodMetadata.InvocationName ?? enumeratorCurrent.Name;

                }
                var enumeratorVariableSymbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/) as ILocalSymbol;
                if (enumeratorVariableSymbol != null)
                {
                    if (variable is SyntaxToken identifierName)
                        CurrentClosure.DefineIdentifierType(identifierName.ValueText, CodeSymbol.From(enumeratorVariableSymbol));
                }
            }


            var enumarableName = $"$en{Writer.ClosureDepth}";
            Writer.Write(node, $"var {enumarableName} = ", true);
            WriteVariableAssignment(node, null, enumerableTypeSymbol, null, node.Expression, enumerationTargetRhsTypeSymbol);
            //Visit(node.Expression);
            Writer.WriteLine(node, $".{getEnumeratorInvocationName}();");
            Writer.WriteLine(node, $"while ({enumarableName}.{enumeratorMoveNextInvocationName}())", true);
            //if (!node.Statement.IsKind(SyntaxKind.Block))
            Writer.WriteLine(node, "{", true);
            if (variable is SyntaxToken identifierName2)
            {
                Writer.WriteLine(node, $"var {identifierName2.ValueText} = {enumarableName}.{enumeratorCurrentInvocationName};", true);
            }
            else if (variable is TupleExpressionSyntax tp)
            {
                var i = ++Writer.CurrentClosure.NameManglingSeed;
                Writer.WriteLine(node, $"var $t{i} = {enumarableName}.{enumeratorCurrentInvocationName};", true);
                int item = 0;
                foreach (var t in tp.Arguments)
                {
                    item++;
                    if (t.Expression.IsKind(SyntaxKind.DiscardPattern))
                        continue;
                    if (t.Expression is IdentifierNameSyntax id && id.Identifier.ValueText == "_")
                        continue;
                    Writer.Write(t.Expression, "", true);
                    Visit(t.Expression);
                    Writer.WriteLine(node, $" = $t{i}.Item{item};");
                }
            }
            Visit(node.Statement);
            //if (!node.Statement.IsKind(SyntaxKind.Block))
            Writer.WriteLine(node, "}", true);
            CloseClosure();
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            WriteForEach(node, node.Identifier);
            //base.VisitForEachStatement(node);
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            WriteForEach(node, node.Variable);
            //base.VisitForEachVariableStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            ExpressionSyntax? rewittenCondition = null;
            if (node.Condition != null)
            {
                bool conditionIsAlwaysFalse = _global.EvaluateConditionalExpressionAsConstant(node.Condition, this, out rewittenCondition) == false;
                if (conditionIsAlwaysFalse)
                {
                    Writer.WriteLine(node, $"//for {node.Condition.ToString().Replace("\r", "").Replace("\n", "")} {{ ... }}", true);
                    return;
                }
            }
            OpenClosure(node);
            Writer.Write(node, "for(", true);
            if (node.Declaration != null)
                Visit(node.Declaration);
            Writer.Write(node, "; ");
            if (node.Condition != null)
                Visit(rewittenCondition ?? node.Condition);
            Writer.Write(node, "; ");
            int i = 0;
            foreach (var inc in node.Incrementors)
            {
                if (i > 0)
                    Writer.Write(node, ", ");
                Visit(inc);
                i++;
            }
            Writer.WriteLine(node, ")");
            if (!node.Statement.IsKind(SyntaxKind.Block))
                Writer.WriteLine(node, "{", true);
            Visit(node.Statement);
            if (!node.Statement.IsKind(SyntaxKind.Block))
                Writer.WriteLine(node, "}", true);
            CloseClosure();
        }

    }
}
