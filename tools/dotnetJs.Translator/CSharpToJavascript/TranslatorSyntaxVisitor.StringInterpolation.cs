using dotnetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        public override void VisitInterpolation(InterpolationSyntax node)
        {
            var handler = (ITypeSymbol)_global.GetTypeSymbol("System.Runtime.CompilerServices.DefaultInterpolatedStringHandler", this);
            Writer.Write(node, $"$handler.", true);
            var sint = (ITypeSymbol)_global.GetTypeSymbol("System.Int32", this);
            var sstring = (ITypeSymbol)_global.GetTypeSymbol("System.String", this);
            var appendFormattedObjectMethod = handler.GetMembers("AppendFormatted").Cast<IMethodSymbol>().Single(e => e.Parameters.Count() == 3 && e.Parameters[0].Type.Equals(_global.Compilation.ObjectType, SymbolEqualityComparer.Default) && e.Parameters[1].Type.Equals(sint, SymbolEqualityComparer.Default) && e.Parameters[2].Type.Equals(sstring, SymbolEqualityComparer.Default));
            WriteMemberName(node, handler, appendFormattedObjectMethod);
            Writer.Write(node, $"(");
            Visit(node.Expression);
            Writer.Write(node, $", ");
            if (node.AlignmentClause != null)
            {
                Visit(node.AlignmentClause);
            }
            else
                Writer.Write(node, "null");
            Writer.Write(node, $", ");
            if (node.FormatClause != null)
            {
                Writer.Write(node, $"\"");
                Writer.Write(node, node.FormatClause.FormatStringToken.ValueText);
                Writer.Write(node, $"\"");
            }
            else
                Writer.Write(node, "null");
            Writer.WriteLine(node, $");");
            //base.VisitInterpolation(node);
        }

        public override void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            var handler = (ITypeSymbol)_global.GetTypeSymbol("System.Runtime.CompilerServices.DefaultInterpolatedStringHandler", this);
            Writer.Write(node, $"$handler.", true);
            WriteMemberName(node, handler, "AppendLiteral");
            Writer.Write(node, $"(\"");
            Writer.Write(node, node.TextToken.ValueText.Escape());
            Writer.WriteLine(node, $"\");");
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            //Use native js interpolation for simple interpolation types
            if (node.Contents.All(e => e is InterpolatedStringTextSyntax || (e is InterpolationSyntax token && token.FormatClause == null && token.AlignmentClause == null)))
            {
                Writer.Write(node, "`");
                foreach (var token in node.Contents)
                {
                    if (token is InterpolatedStringTextSyntax str)
                    {
                        Writer.Write(node, str/*.ToFullString()*/.TextToken.ValueText);
                    }
                    else if (token is InterpolationSyntax format)
                    {
                        Writer.Write(node, "${");
                        var type = _global.ResolveSymbol(GetExpressionReturnSymbol(format.Expression), this)?.GetTypeSymbol();
                        string? formatSpecifier = null;
                        if (type != null && !type.IsType("System.String")&& !type.IsJsPrimitive())
                        {
                            var toString = type.GetMembers("ToString", _global).Where(e => e is IMethodSymbol m && m.Parameters.Count() == (formatSpecifier == null ? 0 : 1)).Cast<IMethodSymbol>().FirstOrDefault();
                            if (toString != null)
                            {
                                WriteMethodInvocation(node, toString, null, null, format.Expression, null, null, false);
                            }
                            else
                            {
                                Writer.Write(node, $"{_global.GlobalName}.{Constants.ToStringName}(");
                                Visit(format.Expression);
                                Writer.Write(node, $", \"\"");
                                Writer.Write(node, $")");
                            }
                        }
                        else
                        {
                            if (type?.IsJsPrimitive()??false)
                            {
                                Visit(format.Expression);
                            }
                            else if (type?.IsType("System.String") ?? false)
                            {
                                Visit(format.Expression);
                            }
                            else
                            {
                                Writer.Write(node, $"{_global.GlobalName}.{Constants.ToStringName}(");
                                Visit(format.Expression);
                                Writer.Write(node, $", \"\"");
                                Writer.Write(node, $")");
                            }
                        }
                        Writer.Write(node, "}");
                    }
                }
                Writer.Write(node, "`");
            }
            else
            {
                var handler = (ITypeSymbol)_global.GetTypeSymbol("System.Runtime.CompilerServices.DefaultInterpolatedStringHandler", this);
                var sint = (ITypeSymbol)_global.GetTypeSymbol("System.Int32", this);
                var constructor = handler.GetMembers(".ctor").Cast<IMethodSymbol>().Single(e => e.Parameters.Count() == 2 && e.Parameters[0].Type.Equals(sint, SymbolEqualityComparer.Default) && e.Parameters[1].Type.Equals(sint, SymbolEqualityComparer.Default));
                int literalLenght = node.Contents.Count(e => !e.IsKind(SyntaxKind.Interpolation));
                int formattedLenght = node.Contents.Count(e => e.IsKind(SyntaxKind.Interpolation));

                Writer.WriteLine(node, $"{_global.GlobalName}.{Constants.Expression}(function()");
                Writer.WriteLine(node, $"{{", true);
                Writer.Write(node, $"var $handler = ", true);
                WriteConstructorCall(node, handler, constructor, null, [SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(literalLenght)), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(formattedLenght))]);
                Writer.WriteLine(node, $";");
                foreach (var token in node.Contents)
                {
                    Visit(token);
                }
                Writer.WriteLine(node, "return $handler.ToStringAndClear();", true);
                Writer.Write(node, $"}})", true);
            }
            //base.VisitInterpolatedStringExpression(node);
        }
    }
}
