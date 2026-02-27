using dotnetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        public void WritePointerAdvance(CSharpSyntaxNode node, ExpressionSyntax pointer, CodeNode advance)
        {
            Visit(pointer);
            Writer.Write(node, ".");
            var refOrPointer = (ITypeSymbol)_global.GetTypeSymbol("System.RefOrPointer<>", this);
            var add = (IMethodSymbol)refOrPointer.GetMembers("Add").Single();
            WriteMemberName(node, refOrPointer, add);
            Writer.Write(node, $"(");
            VisitNode(advance);
            Writer.Write(node, $")");
        }

        public void WritePointerSelfAdvance(CSharpSyntaxNode node, ExpressionSyntax pointer, CodeNode advance)
        {
            //Writer.Write(node, "", true);
            Visit(pointer);
            Writer.Write(node, " = ");
            WritePointerAdvance(node, pointer, advance);
        }

        public override void VisitPointerType(PointerTypeSyntax node)
        {
            Writer.Write(node, $"{_global.GlobalName}.{Constants.TypePointer}(");
            Visit(node.ElementType);
            Writer.Write(node, ")");
            //base.VisitPointerType(node);
        }
    }
}
