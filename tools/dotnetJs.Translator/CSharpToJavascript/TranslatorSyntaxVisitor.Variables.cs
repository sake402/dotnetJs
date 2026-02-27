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
        public void WriteLazyVariable(CSharpSyntaxNode node, ExpressionSyntax expression)
        {
            WriteMethodInvocation(node, "System.Runtime.CompilerServices.RuntimeHelpers.LazyValue", arguments: [new CodeNode(() =>
            {
                Writer.Write(node, "() => ");
                Visit(expression);
                //Writer.Write(node, ";");
            })]);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            Writer.Write(node, "", true);
            base.VisitLocalDeclarationStatement(node);
            Writer.WriteLine(node, ";");
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            EnsureImported(node.Type);
            var variableType = _global.TryGetTypeSymbol(node.Type, this/*, out _, out _*/);
            IDisposable? disposeDelegateType = null;
            if (variableType is ITypeSymbol ts)
            {
                if (ts.IsDelegate(out var delegateReturnType, out var delegateParameterTypes))
                {
                    disposeDelegateType = CurrentClosure.DefineAnonymousMethodParameterTypes(delegateReturnType == null ? delegateParameterTypes! : [.. delegateParameterTypes!, delegateReturnType]);
                }
            }
            //In a goto state machine, we already define this variable in the initializer phase
            if (gotoVariableDeclarationActive)
            {
                Writer.Write(node, $"/*{node.Type.ToString().Trim()}*/ let ", true);
            }
            else if (!GotoHasDefinedVariable(node))
            {
                Writer.Write(node, $"/*{node.Type.ToString().Trim()}*/ let ");
            }
            VisitChildren(node.Variables, ", ");
            disposeDelegateType?.Dispose();
            //base.VisitVariableDeclaration(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (gotoVariableDeclarationActive)
            {
                Writer.Write(node, node.Identifier.Text);
                return;
            }
            var parent = (VariableDeclarationSyntax)node.Parent!;
            Writer.Write(node, node.Identifier.Text);
            var localSymbol = _global.TryGetTypeSymbol(node, this/*, out _, out _*/);
            if (localSymbol != null)
            {
                CurrentClosure.DefineIdentifierType(node.Identifier.ValueText, CodeSymbol.From(localSymbol));
            }
            else if (!parent.Type.IsVar)
            {
                //Register type lookups for this vairable in the current closure
                CurrentClosure.DefineIdentifierType(node.Identifier.ValueText, CodeSymbol.From(parent.Type, SymbolKind.Local));
            }
            else if (node.Initializer != null)
            {
                var returnType = GetExpressionReturnSymbol(node.Initializer.Value);
                //Register type lookups for this vairable in the current closure
                CurrentClosure.DefineIdentifierType(node.Identifier.ValueText, returnType);
            }
            else if (memberAccesChainCurrentType.TypeSyntaxOrSymbol != null)
            {
                CurrentClosure.DefineIdentifierType(node.Identifier.ValueText, memberAccesChainCurrentType);
            }
            base.VisitVariableDeclarator(node);
        }

        public override void VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            if (node.Designation.IsKind(SyntaxKind.DiscardDesignation))
            {
                Writer.Write(node, $"{_global.GlobalName}.{Constants.DiscardRefName}");
            }
            else
            {
                Writer.Write(node, $"/*{node.Type}*/ let ");
                Visit(node.Designation);
            }
            //base.VisitDeclarationExpression(node);
        }

        public override void VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            //Writer.InsertInCurrentClosure($"let {node.Identifier.ValueText};", true);
            Writer.Write(node, Utilities.ResolveIdentifierName(node.Identifier));
            //base.VisitSingleVariableDesignation(node);
        }

        public override void VisitDeclarationPattern(DeclarationPatternSyntax node)
        {
            var switchExpression = node.FindClosestParent<SwitchExpressionSyntax>();
            if (switchExpression != null)
            {
                HandleDeclarationPatternInSwitchExpression(node);
            }
            else
            {
                var switchStatement = node.FindClosestParent<SwitchStatementSyntax>();
                if (switchStatement != null)
                {
                    HandleDeclarationPatternInSwitchStatement(node);
                }
                else
                {
                    base.VisitDeclarationPattern(node);
                }
            }
        }

    }
}
