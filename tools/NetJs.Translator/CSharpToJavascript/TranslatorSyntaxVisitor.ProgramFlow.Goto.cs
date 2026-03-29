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
        bool gotoGeneratorActive;
        List<VariableDeclarationSyntax> gotoDeclarationDefined = new List<VariableDeclarationSyntax>();

        bool GotoHasDefinedVariable(VariableDeclarationSyntax variable) => gotoGeneratorActive && gotoDeclarationDefined.Contains(variable);
        bool BlockTryHandleJumpLabels(BlockSyntax node)
        {
            //Debug.Assert(CurrentClosure.Syntax == node);
            var labels = node.ChildNodes().Where(e => e.IsKind(SyntaxKind.LabeledStatement)).Cast<LabeledStatementSyntax>().ToList();
            foreach (var label in labels)
            {
                CurrentClosure.JumpLabels.Add(label.Identifier.ValueText);
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
                    Writer.WriteLine(node, $"/*{((VariableDeclarationSyntax)d.Parent!).Type.ToString().Trim()}*/ let {d.Identifier.ValueText};", true);
                }
                //VisitChildren(declarations);
                gotoDeclarationDefined.AddRange(declarations.Select(d => (VariableDeclarationSyntax)d.Parent!));
                gotoVariableDeclarationActive = false;

                gotoGeneratorActive = true;
                var manglingSeed = ++Writer.CurrentClosure.NameManglingSeed;
                string jumpStart = $"$gotoJumpStart{manglingSeed}";
                string jumpState = $"$gotoJumpState{manglingSeed}";
                CurrentClosure.JumpStartLabelName = jumpStart;
                CurrentClosure.JumpStateMachineVariableName = jumpState;
                Writer.WriteLine(node, $"let {jumpState} = 0;", true);
                Writer.WriteLine(node, $"{jumpStart}: while(true)", true);
                Writer.WriteLine(node, "{", true);
                Writer.WriteLine(node, $"switch({jumpState})", true);
                Writer.WriteLine(node, "{", true, forbidInsertion:true);
                Writer.WriteLine(node, "case 0:", true);
                Writer.WriteLine(node, "{", true);
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
                Writer.WriteLine(node, "}", true); //end case 0
                List<SyntaxNode> labelledStatements = new List<SyntaxNode>();
                void EmitLabelledStatements(LabeledStatementSyntax label)
                {
                    var index = CurrentClosure.JumpLabels.IndexOf(label.Identifier.ValueText) + 1;
                    Writer.Write(node, $"case ", true);
                    Writer.Write(node, index.ToString());
                    Writer.WriteLine(node, $": /*{label.Identifier.ValueText}*/");
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
                Writer.WriteLine(node, "}", true); //end switch
                Writer.WriteLine(node, "break;", true);
                Writer.WriteLine(node, "}", true); //end while
                gotoGeneratorActive = false;
                return true;
            }
            return false;
        }

        public override void VisitLabeledStatement(LabeledStatementSyntax node)
        {
            var index = CurrentClosure.JumpLabels.IndexOf(node.Identifier.ValueText) + 1;
            Writer.Write(node, $"case ", true);
            Writer.Write(node, index.ToString());
            Writer.WriteLine(node, $": /*{node.Identifier.ValueText}*/");
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
                    if (blockClosure.JumpLabels.Contains(id.Identifier.ValueText))
                        return true;
                    return false;
                }) ?? throw new InvalidOperationException("Goto must be within a block");
                var blockClosure = GetClosureOf(block);
                var index = blockClosure.JumpLabels.IndexOf(id.Identifier.ValueText) + 1;
                Writer.Write(node, $"{blockClosure.JumpStateMachineVariableName} = ", true);
                Writer.Write(node, index.ToString());
                Writer.WriteLine(node, $"; /*goto {id.Identifier.ValueText}*/");
                Writer.WriteLine(node, $"continue {blockClosure.JumpStartLabelName};", true);
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
                Writer.Write(node, $"{switchClosure.JumpStateMachineVariableName} = ", true);
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
                    Writer.Write(node, "\"$__ChangeMe__$\"");
                }
                Writer.WriteLine(node, $"; /*goto case {node.Expression?.ToString() ?? "default"}*/");
                Writer.WriteLine(node, $"continue {switchClosure.JumpStartLabelName};", true);
            }
            else
            {

            }
            //base.VisitGotoStatement(node);
        }
    }
}
