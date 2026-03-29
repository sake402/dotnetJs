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
        bool HasYield(CSharpSyntaxNode node)
        {
            return node.DescendantNodes().Any(n => n.IsKind(SyntaxKind.YieldReturnStatement) || n.IsKind(SyntaxKind.YieldBreakStatement));
        }

        void TryWrapInYieldingGetEnumerable(CSharpSyntaxNode node, IEnumerable<TypeSyntax>? typeparameters, IEnumerable<SyntaxNode> body, bool isAsync = false)
        {
            if (HasYield(node))
            {
                bool isObjectEnumerable = false;
                bool isObjectEnumerator = false;
                if (typeparameters == null)
                {
                    var methodInfo = _global.GetTypeSymbol(node, this) as IMethodSymbol;
                    if (methodInfo != null)
                    {
                        var returnType = methodInfo.ReturnType;
                        if (returnType.SpecialType == SpecialType.System_Collections_IEnumerable)
                        {
                            isObjectEnumerable = true;
                        }
                        if (returnType.SpecialType == SpecialType.System_Collections_IEnumerator)
                        {
                            isObjectEnumerator = true;
                        }
                    }
                    if (!isObjectEnumerable && !isObjectEnumerator)
                        throw new InvalidOperationException("Type parameters are required for a yielding enumerable");
                }
                bool bodyIsBlock = body.Count() == 1 && body.Single().IsKind(SyntaxKind.Block);
                if (bodyIsBlock)
                    Writer.WriteLine(node, "{", true);
                var systemObject = (ITypeSymbol)_global.GetTypeSymbol("System.Object", this);
                var genericArgs = $"({string.Join(", ", isObjectEnumerable || isObjectEnumerator ? [systemObject.ComputeOutputTypeName(_global)] : typeparameters.Select(parameter =>
                {
                    var symbol = _global.GetTypeSymbol(parameter, this/*, out _, out _*/);
                    return symbol.ComputeOutputTypeName(_global);
                }))})";
                Writer.WriteLine(node, $"return new {_global.GlobalName}.System.YieldToIterator{genericArgs}({(isAsync ? "async " : "")}function*()", true);
                if (!bodyIsBlock)
                    Writer.WriteLine(node, "{", true);
                //if (bodyIsBlock)
                //{
                //    VisitChildren(body.Single().ChildNodes());
                //}
                //else
                //{
                VisitChildren(body);
                //}
                if (bodyIsBlock)
                    Writer.WriteLine(node, ")", true); //end return
                else
                    Writer.WriteLine(node, "})", true); //end return
                if (isObjectEnumerator)
                {
                    Writer.WriteLine(node, ".GetEnumerator()", true);
                }
                Writer.WriteLine(node, ";", true);
                if (bodyIsBlock)
                    Writer.WriteLine(node, "}", true);
                return;
            }
            VisitChildren(body);
        }

        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            if (node.IsKind(SyntaxKind.YieldBreakStatement))
            {
                Writer.WriteLine(node, $"return;", true);
            }
            else
            {
                Writer.Write(node, $"yield ", true);
                Visit(node.Expression);
                Writer.WriteLine(node, $";");
            }
            //base.VisitYieldStatement(node);
        }
    }
}
