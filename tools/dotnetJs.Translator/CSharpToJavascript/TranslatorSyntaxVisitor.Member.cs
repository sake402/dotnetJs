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
        bool MemberIsLiteralInitialization(EqualsValueClauseSyntax? initializer, ITypeSymbol memberType)
        {
            bool isLiteralInit = initializer != null &&
                   (_global.EvaluateConstant(initializer.Value, this).HasValue || (initializer.Value is LiteralExpressionSyntax || (initializer.Value is PrefixUnaryExpressionSyntax pu && pu.Operand is LiteralExpressionSyntax))) &&
                   memberType.IsJsPrimitive();
            return isLiteralInit;
        }

        public void WriteMemberName(CSharpSyntaxNode node, ITypeSymbol symbol, string memberName, CodeNode? _this = null)
        {
            var member = symbol.GetMembers(memberName, _global).Single();
            WriteMemberName(node, symbol, member, _this);
        }

        public void WriteMemberName(CSharpSyntaxNode node, ITypeSymbol symbol, ISymbol member, CodeNode? _this = null)
        {
            var template = member.GetTemplateAttribute(_global);
            if (template != null)
            {
                if (_this == null && !member.IsStatic)
                {
                    throw new InvalidOperationException("Cannot literarily write a templated member without providing this");
                }
                WriteMethodTemplate(node, _this, symbol, false, null, null, template, default);
            }
            var metadata = _global.GetRequiredMetadata(member);
            Writer.Write(node, metadata.InvocationName ?? symbol.Name);
            if (member.IsStaticCallConvention(_global))
            {
                if (_this == null)
                {
                    throw new InvalidOperationException("Cannot literarily write a member with static convention wthout providing the this");
                }
                Writer.Write(node, ".call(");
                VisitNode(_this);
                Writer.Write(node, ")");
            }
        }

        void WriteMemberAccess(CSharpSyntaxNode node, CodeNode? lhsExpression, ITypeSymbol? lhsSymbol, string? memberName, ISymbol? member)
        {
            if (lhsSymbol == null && lhsExpression == null)
                throw new InvalidOperationException("Must supply one of lhsSymbol or lhsExpression");
            if (memberName == null && member == null)
                throw new InvalidOperationException("Must supply one of memberName or member");
            if (lhsSymbol == null)
            {
                if (lhsExpression!.IsT0)
                {
                    CodeSymbol lhsType = GetExpressionReturnSymbol(lhsExpression!.AsT0);
                    lhsSymbol = _global.ResolveSymbol(lhsType, this/*, out _, out _*/)?.GetTypeSymbol() ??
                        throw new InvalidOperationException($"Cannot resolve expreession type of {lhsExpression}");
                }
                else
                {
                    throw new InvalidOperationException($"Cannot resolve expreession type of {lhsExpression}");
                }
            }
            member ??= lhsSymbol.GetMembers(memberName, _global).FirstOrDefault();
            memberName ??= member.Name;
            bool isStaticConvention = member?.IsStaticCallConvention(_global) ?? false;
            if (member is IFieldSymbol field &&
                field.IsConst &&
                field.ConstantValue != null &&
                (_global.OutputMode.HasFlag(OutputMode.InlineConstants) || _global.HasAttribute(member, typeof(InlineConstAttribute).FullName, this, false, out _) || _global.HasAttribute(member.ContainingType, typeof(InlineConstAttribute).FullName, this, false, out _)) &&
                !_global.HasAttribute(member, typeof(TemplateAttribute).FullName, this, false, out _))
            {
                TryWriteConstant(node, field.Type, null, new Optional<object?>(field.ConstantValue));
                //var systemString = _global.GetTypeSymbol("System.String", this, out _, out _);
                //if (field.Type.Equals(systemString, SymbolEqualityComparer.Default))
                //    Writer.Write(node, "\"");
                //Writer.Write(node, field.ConstantValue.ToString());
                //if (field.Type.Equals(systemString, SymbolEqualityComparer.Default))
                //    Writer.Write(node, "\"");
                return;
            }
            bool isAssignment = false;
            ExpressionSyntax? assignmentRhs = null;
            if (node.Parent is EqualsValueClauseSyntax eq)
            {
                if (eq.Value != node)
                {
                    isAssignment = true;
                    assignmentRhs = eq.Value;
                }
            }
            IMethodSymbol? method = member as IMethodSymbol;
            AttributeData? attribute = member?.GetTemplateAttribute(_global);
            if (attribute == null)
            {
                if (member is IPropertySymbol property)
                {
                    if (isAssignment)
                    {
                        method = property.SetMethod;
                        attribute = property.SetMethod?.GetTemplateAttribute(_global);
                    }
                    else
                    {
                        method = property.GetMethod;
                        attribute = property.GetMethod?.GetTemplateAttribute(_global);
                    }
                }
            }
            if (attribute != null)
            {
                WriteMethodTemplate(node, lhsExpression, lhsSymbol, false, method, null, attribute, default, assignmentRhs);
                return;
            }
            if (member != null)
            {
                var memberMetadata = _global.GetRequiredMetadata(member);
                if (member.IsStatic || isStaticConvention)
                {
                    if (lhsSymbol is ITypeParameterSymbol tp)
                    {
                        Writer.Write(node, tp.Name);
                        Writer.Write(node, ".");
                        Writer.Write(node, memberMetadata.OverloadName /*??memberMetadata.LocalName */?? member.Name);
                    }
                    else
                    {
                        Writer.Write(node, memberMetadata.InvocationName ?? member.Name);
                    }
                    if (isStaticConvention)
                    {
                        Writer.Write(node, ".call(");
                        if (lhsExpression != null)
                            VisitNode(lhsExpression);
                        else
                            Writer.Write(node, "this");
                        Writer.Write(node, ")");
                    }
                    return;
                }
                else
                {
                    memberName = memberMetadata.InvocationName ?? memberName;
                }
            }
            var initialCurrentNamespace = currentExpressionNamespace;
            bool lhsWritten = false;
            if (lhsExpression == null && member != null && !(member.IsStatic || isStaticConvention))
            {
                if (node.Parent is AssignmentExpressionSyntax assign && assign.Left == node && node.Parent?.Parent is InitializerExpressionSyntax)
                {
                }
                else if (node.Parent.IsKind(SyntaxKind.NameColon))
                {
                }
                else
                {
                    if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        if (((MemberAccessExpressionSyntax)node.Parent).Expression == node)
                        {
                            Writer.Write(node, "this");
                            lhsWritten = true;
                        }
                        else if (((MemberAccessExpressionSyntax)node.Parent).Name == node)
                        {
                        }
                    }
                    else
                    {
                        Writer.Write(node, "this");
                        lhsWritten = true;
                    }
                }
            }
            else
            {
                VisitNode(lhsExpression);
                lhsWritten = lhsExpression != null;
            }
            if (/*node.Expression.IsKind(SyntaxKind.IdentifierName) && */initialCurrentNamespace != currentExpressionNamespace)
            {
                //Visit(node.Name);
                Writer.Write(node, memberName);
                //if the above expression is captured into a namespace, don't write the dot
            }
            else
            {
                if (lhsWritten)
                {
                    Writer.Write(node, ".");
                }
                Writer.Write(node, memberName);
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.ToFullString().Contains("char.MinValue") || node.ToFullString().Contains("char.MaxValue"))
            {

            }
            string? memberName = null;
            if (node.Name is GenericNameSyntax gn)
            {
                memberName = gn.Identifier.Text;
            }
            else
            {
                memberName = node.Name.Identifier.ValueText;
            }
            WriteMemberAccess(node, node.Expression, null, memberName, null);
            return;

            ////var memberType = GetExpressionReturnType(node);
            //var _memberType = GetExpressionBoundMember(node);
            //if (_memberType.TypeSyntaxOrSymbol != null)
            //{
            //    var memberType = _global.ResolveTypeSymbol(_memberType, this/*, out var declaringType, out _*/);
            //    if (memberType != null &&
            //            (
            //                _global.HasAttribute(memberType, typeof(InlineConstAttribute).FullName, this, false, out _) ||
            //                (declaringType != null && _global.HasAttribute(declaringType, typeof(InlineConstAttribute).FullName, this, false, out _))
            //            ))
            //    {
            //        var member = declaringType ?? ((ITypeSymbol)memberType).GetMembers(memberName, _global).FirstOrDefault();
            //        if (member is IFieldSymbol field && field.IsConst && field.ConstantValue != null)
            //        {
            //            var systemString = _global.GetTypeSymbol("System.String", this/*, out _, out _*/);
            //            if (field.Type.Equals(systemString, SymbolEqualityComparer.Default))
            //                Writer.Write(node, "\"");
            //            Writer.Write(node, field.ConstantValue.ToString());
            //            if (field.Type.Equals(systemString, SymbolEqualityComparer.Default))
            //                Writer.Write(node, "\"");
            //            return;
            //        }
            //    }
            //}
            ////var type = GetExpressionReturnType(node.Expression);
            //////var symbol = GetTypeSymbol(type, out _);
            //////we want to fully qualify member access names
            ////if (!node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression)/* is not MemberAccessExpressionSyntax*/ && type.TypeSyntaxOrSymbol is INamedTypeSymbol symbol)
            ////{
            ////    var metadata = global.ReversedSymbols[symbol.OriginalDefinition];
            ////    Writer.Write(node, metadata.InvocationName ?? symbol.Name);
            ////}
            ////else
            ////{
            ////}
            //CodeType lhsType = GetExpressionReturnType(node.Expression);
            //ISymbol? lhsSymbol = null;
            //if (lhsType.TypeSyntaxOrSymbol != null)
            //{
            //    lhsSymbol = _global.ResolveTypeSymbol(lhsType, this, out _, out _);
            //    if (lhsSymbol is ITypeSymbol typeSymbol)
            //    {
            //        var accessedMember = typeSymbol.GetMembers(memberName, _global).FirstOrDefault();
            //        bool isAssignment = false;
            //        ExpressionSyntax? assignmentRhs = null;
            //        if (node.Parent is EqualsValueClauseSyntax eq)
            //        {
            //            if (eq.Value != node)
            //            {
            //                isAssignment = true;
            //                assignmentRhs = eq.Value;
            //            }
            //        }
            //        IMethodSymbol? method = accessedMember as IMethodSymbol;
            //        AttributeData? attribute = accessedMember?.GetTemplateAttribute(_global);
            //        if (attribute == null)
            //        {
            //            if (accessedMember is IPropertySymbol property)
            //            {
            //                if (isAssignment)
            //                {
            //                    method = property.SetMethod;
            //                    attribute = property.SetMethod?.GetTemplateAttribute(_global);
            //                }
            //                else
            //                {
            //                    method = property.GetMethod;
            //                    attribute = property.GetMethod?.GetTemplateAttribute(_global);
            //                }
            //            }
            //        }
            //        if (attribute != null)
            //        {
            //            WriteMethodTemplate(node, node.Expression, lhsSymbol, false, method, null, attribute, default, assignmentRhs);
            //            return;
            //        }
            //        if (accessedMember != null)
            //        {
            //            var memberMetadata = _global.GetRequiredMetadata(accessedMember);
            //            if (accessedMember.IsStatic)
            //            {
            //                if (lhsSymbol is ITypeParameterSymbol tp)
            //                {
            //                    Writer.Write(node, tp.Name);
            //                    Writer.Write(node, ".");
            //                    Writer.Write(node, memberMetadata.OverloadName /*??memberMetadata.LocalName */?? accessedMember.Name);
            //                }
            //                else
            //                {
            //                    Writer.Write(node, memberMetadata.InvocationName ?? accessedMember.Name);
            //                }
            //                return;
            //            }
            //            else
            //            {
            //                memberName = memberMetadata.InvocationName ?? memberName;
            //                //if (true)
            //                //{
            //                //    Writer.Write(node, "this.");
            //                //    Writer.Write(node, memberName);
            //                //}
            //            }
            //        }
            //    }
            //}
            //var initialCurrentNamespace = currentExpressionNamespace;
            //Visit(node.Expression);
            //if (/*node.Expression.IsKind(SyntaxKind.IdentifierName) && */initialCurrentNamespace != currentExpressionNamespace)
            //{
            //    Visit(node.Name);
            //    //if the above expression is captured into a namespace, don't write the dot
            //}
            //else
            //{
            //    Writer.Write(node, ".");
            //    Writer.Write(node, memberName);
            //}
            //base.VisitMemberAccessExpression(node);
        }
    }
}
