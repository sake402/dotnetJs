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
        ExpressionSyntax GetPatternExpression(CSharpSyntaxNode node)
        {
            var listPattern = node.FindClosestParent<ListPatternSyntax>();
            var containingIsPatternExpression = node.FindClosestParent<IsPatternExpressionSyntax>();
            var containingSwitchExpression = containingIsPatternExpression == null ? node.FindClosestParent<SwitchExpressionSyntax>() : null;
            var containingSwitchStatement = containingIsPatternExpression == null && containingSwitchExpression == null ? node.FindClosestParent<SwitchStatementSyntax>() : null;
            var switchClosure = CurrentClosure.FindHierachy<SwitchStatementSyntax>() ?? CurrentClosure.FindHierachy<SwitchExpressionSyntax>() ?? CurrentClosure;
            var swVariableName = switchClosure.Tags.GetValueOrDefault(SwitchExpressionVariableName);
            var isVariableName = switchClosure.Tags.GetValueOrDefault(IsPatternExpressionVariableName);
            if (containingSwitchExpression != null)
            {
                return containingSwitchExpression.GoverningExpression;
            }
            else if (containingSwitchStatement != null)
            {
                return containingSwitchStatement.Expression;
            }
            else if (containingIsPatternExpression != null)
            {
                return containingIsPatternExpression.Expression;
            }
            throw new InvalidOperationException();
        }

        void WritePatternExpressionFilter(CSharpSyntaxNode node)
        {
            var listPattern = node.FindClosestParent<ListPatternSyntax>();
            var containingIsPatternExpression = node.FindClosestParent<IsPatternExpressionSyntax>();
            var containingSwitchExpression = containingIsPatternExpression == null ? node.FindClosestParent<SwitchExpressionSyntax>() : null;
            var containingSwitchStatement = containingIsPatternExpression == null && containingSwitchExpression == null ? node.FindClosestParent<SwitchStatementSyntax>() : null;
            var switchClosure = CurrentClosure.FindHierachy<SwitchStatementSyntax>() ?? CurrentClosure.FindHierachy<SwitchExpressionSyntax>() ?? CurrentClosure;
            var swVariableName = switchClosure.Tags.GetValueOrDefault(SwitchExpressionVariableName);
            var isVariableName = switchClosure.Tags.GetValueOrDefault(IsPatternExpressionVariableName);
            if (containingSwitchExpression != null)
            {
                if (swVariableName != null)
                {
                    Writer.Write(node, swVariableName);
                }
                else
                {
                    Visit(containingSwitchExpression.GoverningExpression);
                }
            }
            else if (containingSwitchStatement != null)
            {
                if (swVariableName != null)
                {
                    Writer.Write(node, swVariableName);
                }
                else
                {
                    Visit(containingSwitchStatement.Expression);
                }
            }
            else if (containingIsPatternExpression != null)
            {
                if (isVariableName != null)
                {
                    Writer.Write(node, isVariableName);
                }
                else
                {
                    Visit(containingIsPatternExpression.Expression);
                }
            }
            //Inside a list pattern?
            if (listPattern != null && currentListPatternContext != null)
            {
                Writer.Write(node, "[");
                Writer.Write(node, currentListPatternContext!.CurrentIndex.ToString());
                Writer.Write(node, "]");
            }
        }

        bool patternExpressionWrittenAlready;
        public override void VisitConstantPattern(ConstantPatternSyntax node)
        {
            if (!patternExpressionWrittenAlready && !node.Parent.IsKind(SyntaxKind.PropertyPatternClause) && !node.Parent.IsKind(SyntaxKind.Subpattern))
                WritePatternExpressionFilter(node);
            Writer.Write(node, node.Parent.IsKind(SyntaxKind.NotPattern) ? " !== " : " == ");
            Visit(node.Expression);
        }

        public override void VisitUnaryPattern(UnaryPatternSyntax node)
        {
            //WritePatternExpressionFilter(node);
            Visit(node.Pattern);
        }

        public override void VisitRelationalPattern(RelationalPatternSyntax node)
        {
            if (!patternExpressionWrittenAlready)
                WritePatternExpressionFilter(node);
            Writer.Write(node, " ");
            Writer.Write(node, node.OperatorToken.ValueText);
            Writer.Write(node, " ");
            Visit(node.Expression);
            //base.VisitRelationalPattern(node);
        }

        public override void VisitBinaryPattern(BinaryPatternSyntax node)
        {
            Writer.Write(node, "(");
            //WritePatternExpressionFilter(node);
            Visit(node.Left);
            Writer.Write(node, ")");
            switch (node.OperatorToken.ValueText)
            {
                case "or":
                    Writer.Write(node, " || ");
                    break;
                case "and":
                    Writer.Write(node, " && ");
                    break;
                default:
                    Writer.Write(node, $" {node.OperatorToken.ValueText} ");
                    break;
            }
            Writer.Write(node, "(");
            //WritePatternExpressionFilter(node);
            Visit(node.Right);
            Writer.Write(node, ")");
            //base.VisitBinaryPattern(node);
        }

        public override void VisitDiscardPattern(DiscardPatternSyntax node)
        {
            base.VisitDiscardPattern(node);
        }

        public override void VisitTypePattern(TypePatternSyntax node)
        {
            var switchStatement = node.FindClosestParent<SwitchStatementSyntax>();
            //var switchExpression = node.FindClosest<SwitchExpressionSyntax>();

            //if (switchStatement != null && IsTypeSwitchStatement(switchStatement))
            //{
            //    Visit(node.Type);
            //}
            //else
            //{
            Writer.Write(node, $"{_global.GlobalName}.{Constants.IsTypeName}(");
            WritePatternExpressionFilter(node);
            Writer.Write(node, ", ");
            Visit(node.Type);
            Writer.Write(node, ")");
            //}
        }

        public override void VisitSubpattern(SubpatternSyntax node)
        {
            base.VisitSubpattern(node);
        }

        public override void VisitPropertyPatternClause(PropertyPatternClauseSyntax node)
        {
            var containingIsPatternExpression = node.FindClosestParent<IsPatternExpressionSyntax>() ?? throw new InvalidOperationException();
            int ix = 0;
            foreach (var sub in node.Subpatterns)
            {
                if (ix > 0)
                    Writer.Write(node, " && ");
                //Writer.Write(node, "(");
                if (sub.ExpressionColon?.Expression is IdentifierNameSyntax id)
                {
                    var lhsType = _global.ResolveSymbol(GetExpressionReturnSymbol(containingIsPatternExpression.Expression), this)?.GetTypeSymbol();
                    WriteMemberAccess(id, new CodeNode(() => WritePatternExpressionFilter(node)), lhsType, id.Identifier.ValueText, null);
                }
                else
                {
                    WritePatternExpressionFilter(node);
                    if (sub.ExpressionColon != null)
                    {
                        Writer.Write(node, ".");
                        Visit(sub.ExpressionColon.Expression);
                    }
                    else
                    {

                    }
                }
                patternExpressionWrittenAlready = true;
                Visit(sub.Pattern);
                patternExpressionWrittenAlready = false;
                //Writer.Write(node, ")");
                ix++;
            }
            if (node.Subpatterns.Count == 0)
            {
                Writer.Write(node, "true");
            }
            //base.VisitPropertyPatternClause(node);
        }

        public override void VisitPositionalPatternClause(PositionalPatternClauseSyntax node)
        {
            var containingIsPatternExpression = node.FindClosestParent<IsPatternExpressionSyntax>();
            int ix = 0;
            foreach (var sub in node.Subpatterns)
            {
                if (sub.IsKind(SyntaxKind.DiscardPattern))
                    continue;
                if (ix > 0)
                    Writer.Write(node, " && ");
                Writer.Write(node, "(");
                WritePatternExpressionFilter(node);
                Writer.Write(node, ".");
                Writer.Write(node, "Item");
                Writer.Write(node, (ix + 1).ToString());
                Visit(sub.Pattern);
                Writer.Write(node, ")");
                ix++;
            }
            //base.VisitPositionalPatternClause(node);
        }

        class ListPatternBuidingContext
        {
            public int Items;
            public int PatternIndex;
            public int SpreadStartIndex = -1;
            public int SpreadRemainingElements = -1;
            public int CurrentIndex
            {
                get
                {
                    if (SpreadStartIndex < 0)
                    {
                        return PatternIndex;
                    }
                    return -(Items - PatternIndex);
                }
            }

        }
        ListPatternBuidingContext? currentListPatternContext;
        public override void VisitListPattern(ListPatternSyntax node)
        {
            var patternExpression = GetPatternExpression(node);
            var patteryType = _global.ResolveSymbol(GetExpressionReturnSymbol(patternExpression), this)?.GetTypeSymbol();
            var lenghtPropertyName = "Length";
            var lenghtProperty = patteryType?.GetMembers().FirstOrDefault(m => m.Name == "Length" || m.Name == "Count");
            if (lenghtProperty != null)
            {
                lenghtPropertyName = lenghtProperty.Name;
            }
            string lengthComparisonOperator = " == ";
            int countOffset = 0;
            if (node.Patterns.Any(p => p.IsKind(SyntaxKind.SlicePattern)))
            {
                lengthComparisonOperator = " >= ";
                countOffset = -1;
            }
            Writer.Write(node, "(");
            bool isStaticConvention = lenghtProperty?.IsStaticCallConvention(_global) ?? false;
            bool hasTemplate = lenghtProperty?.GetTemplateAttribute(_global) != null;
            if (lenghtProperty != null)
            {
                if (isStaticConvention || hasTemplate)
                {
                    WriteMemberName(node, patteryType!, lenghtProperty, new CodeNode(() => WritePatternExpressionFilter(node)));
                }
                else
                {
                    WritePatternExpressionFilter(node);
                    Writer.Write(node, ".");
                    WriteMemberName(node, patteryType!, lenghtProperty);
                }
            }
            else
            {
                WritePatternExpressionFilter(node);
                Writer.Write(node, ".length");
            }
            Writer.Write(node, lengthComparisonOperator);
            Writer.Write(node, (node.Patterns.Count + countOffset).ToString());
            ListPatternBuidingContext context = new();
            context.Items = node.Patterns.Count;
            currentListPatternContext = context;
            foreach (var pattern in node.Patterns)
            {
                if (pattern.IsKind(SyntaxKind.SlicePattern))
                {
                    context.SpreadStartIndex = context.PatternIndex;
                    context.SpreadRemainingElements = node.Patterns.Count - context.PatternIndex;
                }
                else if (pattern.IsKind(SyntaxKind.DiscardPattern))
                {
                }
                else
                {
                    Writer.Write(node, " && ");
                    Visit(pattern);
                }
                context.PatternIndex++;
            }
            Writer.Write(node, ")");
            currentListPatternContext = null;
            //base.VisitListPattern(node);
        }

        public override void VisitRecursivePattern(RecursivePatternSyntax node)
        {
            int ix = 0;
            bool hasOpeningBracket = false;
            if (node.Designation is SingleVariableDesignationSyntax sv)
            {
                Writer.Write(node, "(");
                hasOpeningBracket = true;

                Writer.InsertInCurrentClosure(node, $"let {sv.Identifier.ValueText};", true);
                Writer.Write(node, $"{sv.Identifier.ValueText} = ");
                WritePatternExpressionFilter(node);
                Writer.Write(node, ", ");
            }
            if (node.Type != null)
            {
                if (ix > 0)
                    Writer.Write(node, " && ");
                Writer.Write(node, $"{_global.GlobalName}.{Constants.IsTypeName}(");
                WritePatternExpressionFilter(node);
                Writer.Write(node, $", ");
                Visit(node.Type);
                Writer.Write(node, $")");
                ix++;
            }
            if (node.PropertyPatternClause != null)
            {
                if (ix > 0)
                    Writer.Write(node, " && ");
                Visit(node.PropertyPatternClause);
                ix++;
            }
            if (node.PositionalPatternClause != null)
            {
                if (ix > 0)
                    Writer.Write(node, " && ");
                Visit(node.PositionalPatternClause);
                ix++;
            }
            if (ix == 0)
            {
                Writer.Write(node, "true");
            }
            //VisitChildren(node.ChildNodes().Where(e => !e.IsKind(SyntaxKind.SingleVariableDesignation)));
            //base.VisitRecursivePattern(node);
            if (hasOpeningBracket)
                Writer.Write(node, ")");
        }

        const string IsPatternExpressionVariableName = "__isPatternExpressionVariableName__";
        public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            if (node.ToString().Contains("status is OperationStatus.Done"))
            {

            }
            var declarationPattern = node.Pattern as DeclarationPatternSyntax;
            if ((node.Pattern is UnaryPatternSyntax un && un.Pattern is DeclarationPatternSyntax dp2))
            {
                declarationPattern = dp2;
            }
            if (declarationPattern != null)
            {
                //IdentifierNameSyntax? id = null;
                SingleVariableDesignationSyntax? svd = null;
                if (declarationPattern.Designation is SingleVariableDesignationSyntax isvd)
                {
                    //id = iid;
                    svd = isvd;
                }
                if (svd != null)
                {
                    Writer.InsertInCurrentClosure(node, $"let {svd.Identifier.ValueText};", true);
                    Writer.Write(node, "(");
                    Writer.Write(node, svd.Identifier.ValueText);
                    Writer.Write(node, $" = ");
                    Visit(node.Expression);
                    Writer.Write(node, $", ");
                }
                Writer.Write(node, $"{(node.Pattern.IsKind(SyntaxKind.NotPattern) ? "!" : "")}{_global.GlobalName}.{Constants.IsTypeName}(");
                if (svd != null)
                {
                    Writer.Write(node, svd.Identifier.ValueText);
                }
                else
                {
                    Visit(node.Expression);
                }
                Writer.Write(node, $", ");
                Visit(declarationPattern.Type);
                Writer.Write(node, $")");
                if (svd != null)
                {
                    Writer.Write(node, ")");
                }
                if (svd != null)
                {
                    var localSymbol = _global.TryGetTypeSymbol(svd, this/*, out _, out _*/);
                    if (localSymbol != null)
                    {
                        CurrentClosure.DefineIdentifierType(svd.Identifier.ValueText, CodeSymbol.From(localSymbol));
                    }
                    else
                    {
                        CurrentClosure.DefineIdentifierType(svd.Identifier.ValueText, CodeSymbol.From(declarationPattern.Type, SymbolKind.Local));
                        //Writer.Write(node, $", {svd.Identifier.ValueText} = {id}");
                    }
                }
                //Visit(d.Pattern);
            }
            //else if (node.Pattern.IsKind(SyntaxKind.ConstantPattern))
            //{
            //    Visit(node.Expression);
            //    Writer.Write(node, " === ");
            //    Visit(node.Pattern);
            //}
            //else if (node.Pattern.IsKind(SyntaxKind.NotPattern))
            //{
            //    Visit(node.Expression);
            //    Writer.Write(node, " !== ");
            //    Visit(node.Pattern);
            //}
            //else if (node.Pattern is BinaryPatternSyntax bp)
            //{
            //    Visit(node.Expression);
            //    Visit(bp.Left);
            //    Writer.Write(node, bp.IsKind(SyntaxKind.AndPattern) ? " && " : " || ");
            //    Visit(node.Expression);
            //    Visit(bp.Right);
            //}
            else
            {
                bool needsVar = NeedsCachePatternExpressionInTempVariable(node.Expression);
                if (needsVar)
                {
                    //We used lazy variable evaluation because:

                    //If we have a statement like:
                    //if (provider.GetType() == typeof(CultureInfo) && ((CultureInfo)provider)._dateTimeInfo is { } info)

                    //This will typically produce:
                    //let $is1 = ($.$cast(provider, $.System.Globalization.CultureInfo))._dateTimeInfo;
                    //if ($.System.Type.op_Equality($.System.Object.GetType.call(provider), $.$typeof($.System.Globalization.CultureInfo)) && $is1 != null && ((info = $is1,..

                    //But this will fail as (CultureInfo)provider)._dateTimeInfo get evaluated before the type check provider.GetType() == typeof(CultureInfo)
                    //We therefore make the temp variable $is a lazy evaluation
                    var i = ++Writer.CurrentClosure.NameManglingSeed;
                    CurrentClosure.Tags.Add(IsPatternExpressionVariableName, $"$is{i}.{Constants.LazyVariableValueName}");
                    Writer.InsertInCurrentClosure(node, () =>
                    {
                        Writer.Write(node, $"let $is{i} = ");
                        WriteLazyVariable(node, node.Expression);
                        //WriteMethodInvocation(node, "System.Runtime.CompilerServices.RuntimeHelpers.LazyValue", arguments: [new CodeNode(() => {
                        //    Writer.Write(node, "() => ");
                        //    Visit(node.Expression);
                        //    //Writer.Write(node, ";");
                        //})]);
                    }, true);
                    //Visit(node.Expression);
                }
                bool patterIsNull = node.Pattern.IsKind(SyntaxKind.ConstantPattern) && (((ConstantPatternSyntax)node.Pattern).Expression.IsKind(SyntaxKind.NullLiteralExpression));
                if (!patterIsNull)
                {
                    WritePatternExpressionFilter(node);
                    Writer.Write(node, " != null && (");
                }
                Visit(node.Pattern);
                if (!patterIsNull)
                {
                    Writer.Write(node, ")");
                }
                if (needsVar)
                {
                    CurrentClosure.Tags.Remove(IsPatternExpressionVariableName);
                }
            }
            //base.VisitIsPatternExpression(node);
        }
    }
}
