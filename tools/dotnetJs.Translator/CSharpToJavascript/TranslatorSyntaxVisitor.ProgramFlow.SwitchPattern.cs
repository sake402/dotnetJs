using dotnetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        void HandleDeclarationPatternInSwitchExpression(DeclarationPatternSyntax node)
        {
            SingleVariableDesignationSyntax? svd = null;
            if (node.Designation is SingleVariableDesignationSyntax isvd)
            {
                svd = isvd;
            }
            if (svd != null)
            {
                Writer.InsertInCurrentClosure(node, $"let {svd.Identifier.ValueText};", true);
                Writer.Write(node, "(");
                Writer.Write(node, svd.Identifier.ValueText);
                Writer.Write(node, $" = ");
                WritePatternExpressionFilter(node);
                Writer.Write(node, $", ");
            }
            Writer.Write(node, $"{_global.GlobalName}.{Constants.IsTypeName}(");
            if (svd != null)
            {
                Writer.Write(node, svd.Identifier.ValueText);
            }
            else
            {
                WritePatternExpressionFilter(node);
            }
            Writer.Write(node, $", ");
            Visit(node.Type);
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
                    CurrentClosure.DefineIdentifierType(svd.Identifier.ValueText, CodeSymbol.From(node.Type, SymbolKind.Local));
                    //Writer.Write(node, $", {svd.Identifier.ValueText} = {id}");
                }
            }
        }

        public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        {
            //var governor = node.FindClosest<SwitchExpressionSyntax>()!.GoverningExpression;
            if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern) || node.WhenClause != null)
            {
                Writer.Write(node, "if", true);
                if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern))
                {
                    Writer.Write(node, "(");
                }
                if (node.WhenClause != null)
                {
                    Writer.Write(node, "(");
                }
            }
            //if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern)) { 
            //    WritePatternExpressionFilter(node);
            //}
            Visit(node.Pattern);
            if (node.WhenClause != null)
            {
                if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern))
                {
                    Writer.Write(node, ")");
                }
                if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern) && node.WhenClause != null)
                {
                    Writer.Write(node, " && ");
                }
                Visit(node.WhenClause);
            }
            if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern) || node.WhenClause != null)
            {
                Writer.WriteLine(node, ")");
                Writer.WriteLine(node, "{", true);
            }
            WriteReturn(node, node.Expression);
            //Writer.Write(node, node.Expression.IsKind(SyntaxKind.ThrowExpression) ? "" : "return ", true);
            //Visit(node.Expression);
            //Writer.WriteLine(node, ";");
            if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern) || node.WhenClause != null)
            {
                Writer.WriteLine(node, "}", true);
            }
            //base.VisitSwitchExpressionArm(node);
        }

        bool NeedsCachePatternExpressionInTempVariable(ExpressionSyntax syntax)
        {
            if (syntax.IsKind(SyntaxKind.IdentifierName))
            {
                return false;
            }
            var inWhile = syntax.FindClosestParent<WhileStatementSyntax>();
            if (inWhile != null)
            {
                if (inWhile.Condition.DescendantNodes().Contains(syntax))
                    return false;
            }
            var inFor = syntax.FindClosestParent<ForStatementSyntax>();
            if (inFor != null)
            {
                if (inFor.Condition != null && inFor.Condition.DescendantNodes().Contains(syntax))
                    return false;
            }
            return true;
        }

        public override void VisitSwitchExpression(SwitchExpressionSyntax node)
        {
            WrapStatementsInExpression(node, () =>
            {
                bool needsVar = NeedsCachePatternExpressionInTempVariable(node.GoverningExpression);
                if (needsVar)
                {
                    var i = ++Writer.CurrentClosure.NameManglingSeed;
                    CurrentClosure.Tags.Add(SwitchExpressionVariableName, $"$switch{i}");
                    Writer.Write(node, $"let $switch{i} = ", true);
                    Visit(node.GoverningExpression);
                    Writer.WriteLine(node, ";");
                }
                foreach (var arm in node.Arms)
                {
                    Visit(arm);
                }
                if (needsVar)
                {
                    CurrentClosure.Tags.Remove(SwitchExpressionVariableName);
                }
            });
            //base.VisitSwitchExpression(node);
        }

    }
}
