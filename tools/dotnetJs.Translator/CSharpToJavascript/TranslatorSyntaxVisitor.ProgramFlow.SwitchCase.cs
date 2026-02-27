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
        void HandleDeclarationPatternInSwitchStatement(DeclarationPatternSyntax node)
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

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            var switchStatement = node.FindClosestParent<SwitchStatementSyntax>() ?? throw new InvalidOperationException("Case should be inside a switch");
            if (IsSimpleSwitchCase(switchStatement))
            {
                Writer.Write(node, "case ", true);
                Visit(node.Value);
                Writer.WriteLine(node, ":");
            }
            else
            {
                Writer.Write(node, $"{_global.GlobalName}.{Constants.IsTypeName}(");
                WritePatternExpressionFilter(node);
                Writer.Write(node, $", ");
                Visit(node.Value);
                Writer.Write(node, $")");
            }
            //base.VisitCaseSwitchLabel(node);
        }

        public override void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
        {
            var switchStatement = node.FindClosestParent<SwitchStatementSyntax>();
            if (switchStatement != null && !IsSimpleSwitchCase(switchStatement))
            {
                if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern))
                {
                    WritePatternExpressionFilter(node);
                    Writer.Write(node, " != null && ");
                    Visit(node.Pattern);
                }
                if (node.WhenClause != null)
                {
                    if (!node.Pattern.IsKind(SyntaxKind.DiscardPattern))
                    {
                        Writer.Write(node, " && ");
                    }
                    Visit(node.WhenClause);
                }
            }
            else
            {
                Writer.Write(node, "case ", true);
                Visit(node.Pattern);
                Writer.WriteLine(node, ":");
            }
        }

        bool HasGotoCase(SwitchStatementSyntax node)
        {
            return node.DescendantNodes().Any(c => c.IsKind(SyntaxKind.GotoCaseStatement) || c.IsKind(SyntaxKind.GotoDefaultStatement));
        }

        bool IsSimpleSwitchCase(SwitchStatementSyntax node)
        {
            return node.Sections.SelectMany(c => c.Labels).All(c => c.IsKind(SyntaxKind.CaseSwitchLabel) || c.IsKind(SyntaxKind.DefaultSwitchLabel));
        }

        //bool IsTypeSwitchStatement(SwitchStatementSyntax node)
        //{
        //    bool isTypeSwitch = node.ChildNodes()
        //                   .Any(c => c.IsKind(SyntaxKind.CasePatternSwitchLabel) || (c is SwitchSectionSyntax cc && cc.Labels.Any(l => l.IsKind(SyntaxKind.CasePatternSwitchLabel))));
        //    return isTypeSwitch;
        //}

        const string SwitchExpressionVariableName = "__switchExpressionVariableName__";
        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (node.ToString().Contains("Rune.DecodeFromUtf16(chars, out Rune firstScalarValue, out int charsConsumedThisIteration)"))
            {

            }
            bool isSimpleSwitchCase = IsSimpleSwitchCase(node);
            var hasGotoCase = HasGotoCase(node);
            //if any of the case is a CasePatternSwitchLabelSyntax, use.GetType()
            //bool isTypeSwitch = IsTypeSwitchStatement(node);
            OpenClosure(node);
            var switchClosure = CurrentClosure;
            if (hasGotoCase)
            {
                var manglingSeed = ++Writer.CurrentClosure.NameManglingSeed;
                string jumpStart = $"$switchJumpStart{manglingSeed}";
                string jumpState = $"$switchJumpState{manglingSeed}";
                CurrentClosure.JumpStartLabelName = jumpStart;
                CurrentClosure.JumpStateMachineVariableName = jumpState;
                Writer.WriteLine(node, $"let {jumpState} = null;", true);
                Writer.WriteLine(node, $"{jumpStart}: while(true)", true);
                Writer.WriteLine(node, "{", true);
            }
            else if (!isSimpleSwitchCase)
            {
                Writer.WriteLine(node, $"while(true)", true);
                Writer.WriteLine(node, "{", true);
            }
            var i = ++Writer.CurrentClosure.NameManglingSeed;
            switchClosure.Tags.Add(SwitchExpressionVariableName, $"$switch{i}");
            if (!isSimpleSwitchCase)
            {
                Writer.WriteLine(node, $"//switch ({node.Expression.ToString().Escape()})", true);
            }
            Writer.Write(node, $"let $switch{i} = ", true);
            Visit(node.Expression);
            Writer.WriteLine(node, $";");
            if (isSimpleSwitchCase)
            {
                Writer.Write(node, "switch(", true);
                if (hasGotoCase)
                {
                    Writer.Write(node, $"{CurrentClosure.JumpStateMachineVariableName} ?? ");
                }
                //if (isTypeSwitch)
                //{
                //    Writer.Write(node, $"{_global.GlobalName}.System.Object.GetType.call(");
                //}
                Writer.Write(node, $"$switch{i}");
                //if (isTypeSwitch)
                //{
                //    Writer.Write(node, $").{Constants.TypePrototypeName}");
                //}
                Writer.WriteLine(node, ")");
                Writer.WriteLine(node, "{", true, forbidInsertion: true);
            }
            else
            {

            }
            VisitChildren(node.Sections);
            CloseClosure();
            //base.VisitSwitchStatement(node);
            if (isSimpleSwitchCase)
            {
                Writer.WriteLine(node, "}", true);
            }
            if (hasGotoCase || !isSimpleSwitchCase)
            {
                Writer.WriteLine(node, "break;", true); //end while
                Writer.WriteLine(node, "}", true);
            }
            switchClosure.Tags.Remove(SwitchExpressionVariableName);
        }

        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            var switchStatement = node.FindClosestParent<SwitchStatementSyntax>() ?? throw new InvalidOperationException("Case should be inside a switch");
            bool isSimpleSwitch = IsSimpleSwitchCase(switchStatement);
            bool sectionIsDefault = node.Labels.All(l => l.IsKind(SyntaxKind.DefaultSwitchLabel));
            //bool hasGotoCase = HasGotoCase(switchStatement);
            if (!isSimpleSwitch)
            {
                if (!sectionIsDefault)
                {
                    //make sure all case are in a closure lest we have varible conflict
                    if (node.Labels.Any(l => l.IsKind(SyntaxKind.CasePatternSwitchLabel)))
                    {
                        OpenClosure(node);
                        Writer.WriteLine(node, "{", true);
                    }
                    foreach (var label in node.Labels)
                    {
                        Writer.WriteLine(node, $"//{label.ToString().Escape()}", true);
                    }
                    Writer.Write(node, "if (", true);
                }
                //if (hasGotoCase)
                //{
                //    Writer.Write(node, $"{CurrentClosure.JumpStateMachineVariableName} == ");
                //    WritePatternExpressionFilter(node);
                //    Writer.Write(node, " || ");
                //}
            }
            int ix = 0;
            foreach (var label in node.Labels)
            {
                if (!isSimpleSwitch && ix > 0)
                {
                    Writer.Write(node, " || ");
                }
                Visit(label);
                ix++;
            }
            if (!isSimpleSwitch)
            {
                if (!sectionIsDefault)
                {
                    Writer.WriteLine(node, ")");
                }
            }
            var swClosure = CurrentClosure;
            bool childIsBlock = Utilities.ChildIsBlock(node);
            if (!isSimpleSwitch)
            {
                if (sectionIsDefault)
                {
                    Writer.WriteLine(node, $"//default", true);
                }
            }
            if (!childIsBlock)
            {
                OpenClosure(node);
                Writer.WriteLine(node, "{", true);
            }
            //foreach (var label in node.Labels)
            //{
            //    if (label is CasePatternSwitchLabelSyntax cp && cp.Pattern is DeclarationPatternSyntax dps && dps.Designation is SingleVariableDesignationSyntax svd)
            //    {
            //        var swExpressionVaribleName = swClosure.Tags[SwitchExpressionVariableName];
            //        Writer.WriteLine(node, $"let {svd.Identifier.ValueText} = {swExpressionVaribleName};", true);
            //        //Visit(dps.Designation);
            //        //Writer.Write(node, " = ");
            //        //Visit(dps.Type);
            //    }
            //}
            VisitChildren(node.ChildNodes().Except(node.Labels));
            //base.VisitSwitchSection(node);
            if (!childIsBlock)
            {
                Writer.WriteLine(node, "}", true);
                CloseClosure();
            }
            if (!isSimpleSwitch)
            {
                if (!sectionIsDefault)
                {
                    if (node.Labels.Any(l => l.IsKind(SyntaxKind.CasePatternSwitchLabel)))
                    {
                        CloseClosure();
                        Writer.WriteLine(node, "}", true);
                    }
                }
            }
        }

        public override void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            var switchStatement = node.FindClosestParent<SwitchStatementSyntax>() ?? throw new InvalidOperationException("Case should be inside a switch");
            if (IsSimpleSwitchCase(switchStatement))
            {
                Writer.WriteLine(node, "default:", true);
            }
            //base.VisitDefaultSwitchLabel(node);
        }
    }
}
