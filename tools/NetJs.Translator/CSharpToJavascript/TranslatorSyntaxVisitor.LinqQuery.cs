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
        public override void VisitQueryExpression(QueryExpressionSyntax node)
        {
            Writer.WriteLine(node, "dotnetJs.Expression(function()");
            Writer.WriteLine(node, "{", true);
            Writer.WriteLine(node, "let $ret = [];", true);
            Visit(node.FromClause);
            VisitChildren(node.ChildNodes().Except([node.FromClause]));
            Writer.WriteLine(node, "return $ret;", true);
            Writer.Write(node, "}.bind(this))", true);
            //base.VisitQueryExpression(node);
        }

        public override void VisitFromClause(FromClauseSyntax node)
        {
            var enumarableName = $"$en{Writer.ClosureDepth}";
            Writer.Write(node, $"var {enumarableName} = ", true);
            Visit(node.Expression);
            Writer.WriteLine(node, ";");
            Writer.WriteLine(node, $"while ({enumarableName}.MoveNext())", true);
            var children = node.ChildNodes().Except([node.Expression]);
            //bool explicitBlock = false;
            //if (children.Count() == 1 && children.Single() is not BlockSyntax)
            //{
            //    explicitBlock = true;
            //}
            //if (explicitBlock)
            //    Writer.WriteLine("{", true);
            VisitChildren(children);
            //if (explicitBlock)
            //    Writer.WriteLine("}", true);
            //base.VisitFromClause(node);
        }
        
        public override void VisitWhereClause(WhereClauseSyntax node)
        {
            Writer.Write(node, "if (", true);
            Visit(node.Condition);
            Writer.WriteLine(node, ")");
            //base.VisitWhereClause(node);
        }

        public override void VisitSelectClause(SelectClauseSyntax node)
        {
            Writer.Write(node, $"$ret.Push(", true);
            Visit(node.Expression);
            Writer.WriteLine(node, ");");
            //base.VisitSelectClause(node);
        }

        public override void VisitQueryBody(QueryBodySyntax node)
        {
            var enumarableName = $"$en{Writer.ClosureDepth}";
            var query = (QueryExpressionSyntax)node.Parent!;
            Writer.WriteLine(node, "{", true);
            Writer.WriteLine(node, $"var {query.FromClause.Identifier.ValueText/*.ToFullString().Trim()*/} = {enumarableName}.Current;", true);
            VisitChildren(node.ChildNodes());
            Writer.WriteLine(node, "}", true);
            //base.VisitQueryBody(node);
        }

    }
}
