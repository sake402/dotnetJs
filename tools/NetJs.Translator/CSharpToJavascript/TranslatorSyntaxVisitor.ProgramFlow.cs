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
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            bool conditionIsAlwaysFalse = _global.EvaluateConditionalExpressionAsConstant(node.Condition, this, out var rewittenCondition) == false;
            if (conditionIsAlwaysFalse)
            {
                Writer.WriteLine(node, $"//{node.Condition.ToString().Replace("\r", "").Replace("\n", "")} {{ ... }}", true);
                return;
            }
            Writer.Write(node, "while(", true);
            Visit(rewittenCondition);
            Writer.WriteLine(node, ")");
            if (!node.Statement.IsKind(SyntaxKind.Block))
                Writer.WriteLine(node, "{", true);
            Visit(node.Statement);
            if (!node.Statement.IsKind(SyntaxKind.Block))
                Writer.WriteLine(node, "}", true);
            //base.VisitWhileStatement(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            bool conditionIsAlwaysFalse = _global.EvaluateConditionalExpressionAsConstant(node.Condition, this, out var rewittenCondition) == false;
            CodeLineWriter? doLine = null;
            if (!conditionIsAlwaysFalse)
            {
                doLine = Writer.WriteLine(node, "do", true);
                if (!node.Statement.IsKind(SyntaxKind.Block))
                    Writer.WriteLine(node, "{", true);
            }
            else
            {
                Writer.WriteLine(node, $"//do {{ ", true);
            }
            Visit(node.Statement);
            if (!conditionIsAlwaysFalse)
            {
                if (!node.Statement.IsKind(SyntaxKind.Block))
                    Writer.WriteLine(node, "}", true);
                var whileLine = Writer.Write(node, "while(", true);
                //mak us insert before do, any request from while
                whileLine.RedirectInsertBefore = doLine;
                Visit(rewittenCondition);
                Writer.WriteLine(node, ");");
            }
            else
            {
                Writer.WriteLine(node, $"// }} while({node.Condition.ToString().Replace("\r", "").Replace("\n", "")})", true);
            }
            //base.VisitDoStatement(node);
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            Writer.WriteLine(node, "break;", true);
            base.VisitBreakStatement(node);
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            Writer.WriteLine(node, "continue;", true);
            base.VisitContinueStatement(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            bool conditionIsAlwaysFalse = _global.EvaluateConditionalExpressionAsConstant(node.Condition, this, out var rewittenCondition) == false;
            CodeLineWriter? ifLine = null;
            if (!conditionIsAlwaysFalse)
            {
                ifLine = Writer.Write(node, $"if (", !node.Parent.IsKind(SyntaxKind.ElseClause)/* is not ElseClauseSyntax*/);
                Visit(rewittenCondition);
                Writer.WriteLine(node, $")");
                if (!node.Statement.IsKind(SyntaxKind.Block))
                    Writer.WriteLine(node, "{", true);
                Visit(node.Statement);
                if (!node.Statement.IsKind(SyntaxKind.Block))
                    Writer.WriteLine(node, "}", true);
            }
            else
            {
                Writer.WriteLine(node, $"//if ({node.Condition.ToString().Replace("\r", "").Replace("\n", "")}) {{ ... }}", !node.Parent.IsKind(SyntaxKind.ElseClause));
            }
            if (node.Else != null)
            {
                bool needsClosingBrace = false;
                if (node.Else.Statement.IsKind(SyntaxKind.IfStatement)/* is IfStatementSyntax*/)
                {
                    if (!conditionIsAlwaysFalse)
                    {
                        bool elseIfConditionIsAlwaysFalse = _global.EvaluateConditionalExpressionAsConstant(((IfStatementSyntax)node.Else.Statement).Condition, this, out var rewittenElseCondition) == false;
                        if (elseIfConditionIsAlwaysFalse)
                            Writer.Write(node, "/*else*/ ", true);
                        else
                        {
                            var elseLine = Writer.Write(node, "else ", true);
                            elseLine.RedirectInsertBefore = ifLine;
                        }
                    }
                    else
                    {
                        Writer.Write(node, "/*else*/ ", true);
                    }
                }
                else
                {
                    if (!conditionIsAlwaysFalse)
                    {
                        var elseLine = Writer.WriteLine(node, "else ", true);
                        elseLine.RedirectInsertBefore = ifLine;
                        if (!node.Else.Statement.IsKind(SyntaxKind.Block))
                        {
                            Writer.WriteLine(node, "{", true);
                            needsClosingBrace = true;
                        }
                    }
                    else
                    {
                        Writer.WriteLine(node, "//else ", true);
                    }
                }
                Visit(node.Else);
                if (needsClosingBrace)
                {
                    Writer.WriteLine(node, "}", true);
                }
            }
            //base.VisitIfStatement(node);
        }

        ISymbol? InferReturnType(CSharpSyntaxNode node)
        {
            ISymbol? returnType = null;
            var method = node.FindClosestParent<MethodDeclarationSyntax>();
            if (method != null)
            {
                var methodSymbol = (IMethodSymbol)_global.GetTypeSymbol(method, this/*, out _, out _*/);
                returnType = methodSymbol;
            }
            else
            {
                var property = node.FindClosestParent<PropertyDeclarationSyntax>();
                if (property != null)
                {
                    IPropertySymbol? propertySymbol = (IPropertySymbol)_global.GetTypeSymbol(property, this/*, out _, out _*/);
                    returnType = propertySymbol;
                }
                else
                {
                    var indexer = node.FindClosestParent<IndexerDeclarationSyntax>();
                    if (indexer != null)
                    {
                        IPropertySymbol? propertySymbol = (IPropertySymbol)_global.GetTypeSymbol(indexer, this/*, out _, out _*/);
                        returnType = propertySymbol;
                    }
                }
            }
            return returnType;
        }

        void WriteReturn(CSharpSyntaxNode node, ExpressionSyntax? expression)
        {
            Writer.Write(node, expression == null || !expression.IsKind(SyntaxKind.ThrowExpression) ? "return " : "", true);
            if (expression != null)
            {
                //We try to get the required return type from this expression to make sure we return the right type
                var rhsType = _global.ResolveSymbol(GetExpressionReturnSymbol(expression), this/*, out _, out _*/);
                var returnType = InferReturnType(node) ?? rhsType;
                WriteVariableAssignment(node, null, returnType, null, expression, rhsType);
            }
            Writer.WriteLine(node, ";");
        }

        public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            if (node.Expression.IsKind(SyntaxKind.ThrowExpression)/* is ThrowExpressionSyntax*/)
                Writer.Write(node, "", true);
            else
            {
                WriteReturn(node, node.Expression);
                return;
            }
            base.VisitArrowExpressionClause(node);
            Writer.WriteLine(node, ";");
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            WriteReturn(node, node.Expression);
            //base.VisitReturnStatement(node);
        }
    }
}
