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
        bool gotoVariableDeclarationActive;
        int gotoGeneratorActive;
        List<VariableDeclarationSyntax> gotoDeclarationDefined = new List<VariableDeclarationSyntax>();

        bool GotoHasDefinedVariable(VariableDeclarationSyntax variable) => gotoGeneratorActive > 0 && gotoDeclarationDefined.Contains(variable);
        bool BlockTryHandleJumpLabels(BlockSyntax node)
        {
            //Debug.Assert(CurrentClosure.Syntax == node);
            var labels = node.ChildNodes().Where(e => e.IsKind(SyntaxKind.LabeledStatement)).Cast<LabeledStatementSyntax>().ToList();
            foreach (var label in labels)
            {
                CurrentClosure.GotoJumpLabels.Add(label.Identifier.ValueText);
            }
            if (labels.Count > 0)
            {
                gotoVariableDeclarationActive = true;

                //var firstLabel = labels.First();
                //FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(firstLabel.Span);
                //int lineNumber = span.StartLinePosition.Line;

                //Since we use switch case for goto, define all declarations in the block initially so they are visible througout the block scope
                var declarations = (node.ChildNodes().Concat(labels.SelectMany<LabeledStatementSyntax, SyntaxNode>(l => l.ChildNodes())))
                    .Where(e => e.IsKind(SyntaxKind.LocalDeclarationStatement))
                    .Cast<LocalDeclarationStatementSyntax>()
                    .SelectMany(e => e.Declaration.Variables)
                    .DistinctBy(v => v.Identifier.ValueText);
                foreach (var d in declarations)
                {
                    CurrentTypeWriter.WriteLine(node, $"/*{((VariableDeclarationSyntax)d.Parent!).Type.ToString().Trim()}*/ let {d.Identifier.ValueText};", true);
                }
                //VisitChildren(declarations);
                gotoDeclarationDefined.AddRange(declarations.Select(d => (VariableDeclarationSyntax)d.Parent!));
                gotoVariableDeclarationActive = false;

                gotoGeneratorActive++;
                var manglingSeed = ++CurrentTypeWriter.CurrentClosure.NameManglingSeed;
                string jumpStart = $"$gotoJumpStart{manglingSeed}";
                string jumpState = $"$gotoJumpState{manglingSeed}";
                CurrentClosure.JumpStartLabelName = jumpStart;
                CurrentClosure.JumpStateMachineVariableName = jumpState;
                CurrentTypeWriter.WriteLine(node, $"let {jumpState} = 0;", true);
                CurrentTypeWriter.WriteLine(node, $"{jumpStart}: while(true)", true);
                CurrentTypeWriter.WriteLine(node, "{", true);
                CurrentTypeWriter.WriteLine(node, $"switch({jumpState})", true);
                CurrentTypeWriter.WriteLine(node, "{", true, forbidInsertion: true);
                CurrentTypeWriter.WriteLine(node, "case 0:", true);
                CurrentTypeWriter.WriteLine(node, "{", true);
                //write every statement with no label first
                List<SyntaxNode> writtenNodes = new List<SyntaxNode>();
                foreach (var mnode in node.ChildNodes())
                {
                    if (labels.Contains(mnode))
                    {
                        break;
                    }
                    Visit(mnode);
                    writtenNodes.Add(mnode);
                }
                //Writer.WriteLine(node, "break;", true);
                CurrentTypeWriter.WriteLine(node, "}", true); //end case 0
                List<SyntaxNode> labelledStatements = new List<SyntaxNode>();
                void EmitLabelledStatements(LabeledStatementSyntax label)
                {
                    var index = CurrentClosure.GotoJumpLabels.IndexOf(label.Identifier.ValueText) + 1;
                    CurrentTypeWriter.Write(node, $"case ", true);
                    CurrentTypeWriter.Write(node, index.ToString());
                    CurrentTypeWriter.WriteLine(node, $": /*{label.Identifier.ValueText}*/");
                    foreach (var node in labelledStatements)
                    {
                        Visit(node);
                    }
                    labelledStatements.Clear();
                }
                LabeledStatementSyntax? currentLabel = null;
                foreach (var mnode in node.ChildNodes())
                {
                    if (writtenNodes.Contains(mnode))
                        continue;
                    if (labels.Contains(mnode))
                    {
                        if (currentLabel != null)
                            EmitLabelledStatements(currentLabel);
                        currentLabel = (LabeledStatementSyntax)mnode;
                        labelledStatements.Add(currentLabel.Statement);
                        continue;
                    }
                    labelledStatements.Add(mnode);
                }
                if (currentLabel != null)
                    EmitLabelledStatements(currentLabel);
                CurrentTypeWriter.WriteLine(node, "}", true); //end switch
                CurrentTypeWriter.WriteLine(node, "break;", true);
                CurrentTypeWriter.WriteLine(node, "}", true); //end while
                gotoGeneratorActive--;
                //gotoDeclarationDefined.Remove(declarations.Select(d => (VariableDeclarationSyntax)d.Parent!));

                return true;
            }
            return false;
        }

        public override void VisitLabeledStatement(LabeledStatementSyntax node)
        {
            var index = CurrentClosure.GotoJumpLabels.IndexOf(node.Identifier.ValueText) + 1;
            CurrentTypeWriter.Write(node, $"case ", true);
            CurrentTypeWriter.Write(node, index.ToString());
            CurrentTypeWriter.WriteLine(node, $": /*{node.Identifier.ValueText}*/");
            Visit(node.Statement);
            //Writer.WriteLine(node, "break;", true);
            //base.VisitLabeledStatement(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            if (node.Expression is IdentifierNameSyntax id)
            {
                BlockSyntax block = node.FindClosestParent<BlockSyntax>(e =>
                {
                    var blockClosure = GetClosureOf(e);
                    if (blockClosure.GotoJumpLabels.Contains(id.Identifier.ValueText))
                        return true;
                    return false;
                }) ?? throw new InvalidOperationException("Goto must be within a block");
                var blockClosure = GetClosureOf(block);
                var index = blockClosure.GotoJumpLabels.IndexOf(id.Identifier.ValueText) + 1;
                CurrentTypeWriter.Write(node, $"{blockClosure.JumpStateMachineVariableName} = ", true);
                CurrentTypeWriter.Write(node, index.ToString());
                CurrentTypeWriter.WriteLine(node, $"; /*goto {id.Identifier.ValueText}*/");
                CurrentTypeWriter.WriteLine(node, $"continue {blockClosure.JumpStartLabelName};", true);
            }
            else if (node.IsKind(SyntaxKind.GotoCaseStatement) || node.IsKind(SyntaxKind.GotoDefaultStatement))
            {
                var _switch = node.FindClosestParent<SwitchStatementSyntax>(e =>
                {
                    var switchClosure = GetClosureOf(e);
                    if (switchClosure.JumpStartLabelName != null)
                        return true;
                    return false;
                }) ?? throw new InvalidOperationException("Goto case must be within a switch");
                var switchClosure = GetClosureOf(_switch);
                CurrentTypeWriter.Write(node, $"{switchClosure.JumpStateMachineVariableName} = ", true);
                if (node.IsKind(SyntaxKind.GotoCaseStatement))
                {
                    //Debug.Assert(node.Expression is LiteralExpressionSyntax);
                    Visit(node.Expression); //this should be a literal expression
                }
                else
                {
                    //default
                    //we want to make sure we assign a value to JumpStateMachineVariableName that will not match any case in the switch
                    //undefined or null will not work as we generate the switch using JumpStateMachineVariableName ?? b
                    //TODO: For now we use this magic string. Hopefully it wont conflict with any user defined case value
                    CurrentTypeWriter.Write(node, "\"$__ChangeMe__$\"");
                }
                CurrentTypeWriter.WriteLine(node, $"; /*goto case {node.Expression?.ToString() ?? "default"}*/");
                CurrentTypeWriter.WriteLine(node, $"continue {switchClosure.JumpStartLabelName};", true);
            }
            else
            {

            }
            //base.VisitGotoStatement(node);
        }
    }
}
