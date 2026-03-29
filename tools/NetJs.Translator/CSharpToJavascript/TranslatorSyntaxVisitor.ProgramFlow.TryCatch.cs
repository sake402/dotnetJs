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
        public override void VisitTryStatement(TryStatementSyntax node)
        {
            Writer.WriteLine(node, "try", true);
            Visit(node.Block);
            var catches = node.ChildNodes().Where(e => e.IsKind(SyntaxKind.CatchClause)/* is CatchClauseSyntax*/).Cast<CatchClauseSyntax>();
            if (catches.Count() > 1)
            {
                Writer.WriteLine(node, "catch($e)", true);
                Writer.WriteLine(node, "{", true);
                foreach (var _catch in catches.Where(e => e.Declaration != null))
                {
                    Writer.Write(node, $"if($e instanceof ", true);
                    Visit(_catch.Declaration!.Type);
                    //Writer.Write(node, _catch.Declaration!.Type.ToFullString());
                    if (!string.IsNullOrEmpty(_catch.Declaration!.Identifier.ValueText))
                    {
                        Writer.Write(node, $", {_catch.Declaration!.Identifier.ValueText} = $e");
                    }
                    Writer.WriteLine(node, $")");
                    Visit(_catch.Block);
                }
                foreach (var _catch in catches.Where(e => e.Declaration == null))
                {
                    Visit(_catch.Block);
                }
                Writer.WriteLine(node, "}", true);
                VisitChildren(node.ChildNodes().Except([node.Block, .. catches]));
            }
            else
            {
                VisitChildren(node.ChildNodes().Except([node.Block]));
            }
            //base.VisitTryStatement(node);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Writer.Write(node, "catch(", true);
            if (!string.IsNullOrEmpty(node.Declaration?.Identifier.ValueText))
            {
                var localField = _global.TryGetTypeSymbol(node.Declaration, this/*, out _, out _*/);
                if (localField != null)
                    CurrentClosure.DefineIdentifierType(node.Declaration.Identifier.ValueText, CodeSymbol.From(localField));
                else
                    CurrentClosure.DefineIdentifierType(node.Declaration.Identifier.ValueText, CodeSymbol.From(node.Declaration.Type, SymbolKind.Local));
                Writer.Write(node, node.Declaration.Identifier.ValueText);
            }
            else
                Writer.Write(node, "$e");
            //Visit(node.Declaration);
            Writer.WriteLine(node, ")");
            Visit(node.Block);
            //base.VisitCatchClause(node);
        }

        public override void VisitFinallyClause(FinallyClauseSyntax node)
        {
            Writer.WriteLine(node, "finally", true);
            Writer.WriteLine(node, "{", true);
            Visit(node.Block);
            Writer.WriteLine(node, "}", true);
            //base.VisitFinallyClause(node);
        }

    }
}
