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
        string? currentExpressionNamespace;
        void WriteIdentifier(CSharpSyntaxNode node, SyntaxToken identifier)
        {
            ISymbol? symbolType;
            //ISymbol? declaringSymbol = null;
            //SymbolKind declaringKind = SymbolKind.ErrorType;
            if (currentExpressionNamespace?.Length > 0)
            {
                symbolType = _global.TryGetTypeSymbol(currentExpressionNamespace + "." + identifier.ValueText, this/*, out declaringSymbol, out declaringKind*/);
            }
            else
            {
                symbolType = _global.TryGetTypeSymbol(node, this);
            }
            if (symbolType is INamespaceSymbol ns)
            {
                if (currentExpressionNamespace?.Length > 0)
                    currentExpressionNamespace += ".";
                currentExpressionNamespace += ns.Name;
            }
            else if (symbolType is IFieldSymbol field && field.IsConst && (_global.OutputMode.HasFlag(OutputMode.InlineConstants) || _global.HasAttribute(field, typeof(InlineConstAttribute).FullName, this, false, out _)))
            {
                if (!TryWriteConstant(node, field.Type, node)) //this shouldn't fail, but if it does we need to know
                {
                    throw new ApplicationException($"Cannot write constant expression for {field}");
                }
            }
            else
            {
                currentExpressionNamespace = null;
                bool SymbolIsTypeMember(out bool isPrimaryConstructorParameter)
                {
                    isPrimaryConstructorParameter = false;
                    if (symbolType == null)
                        return false;
                    if (symbolType.ContainingSymbol?.Kind == SymbolKind.NamedType)
                    {
                        if (symbolType.Kind == SymbolKind.Method)
                            return true;
                        if (symbolType.Kind == SymbolKind.Property)
                            return true;
                        if (symbolType.Kind == SymbolKind.Field)
                            return true;
                    }
                    //primary constructor parameter are rewritten as field members
                    if (symbolType.Kind == SymbolKind.Parameter && symbolType.ContainingSymbol is IMethodSymbol method && method.IsPrimaryConstructor(_global))
                    {
                        isPrimaryConstructorParameter = true;
                        return true;
                    }
                    return false;
                }
                if (SymbolIsTypeMember(out var isPrimaryConstructorParameter))
                {
                    //AttributeData? attribute = symbolType!.GetTemplateAttribute(_global);
                    //IMethodSymbol? method = null;
                    //if (attribute == null)
                    //{
                    //    if (symbolType is IPropertySymbol property)
                    //    {
                    //        if (isAssignment)
                    //        {
                    //            method = property.SetMethod;
                    //            attribute = property.SetMethod?.GetTemplateAttribute(_global);
                    //        }
                    //        else
                    //        {
                    //            method = property.GetMethod;
                    //            attribute = property.GetMethod?.GetTemplateAttribute(_global);
                    //        }
                    //    }
                    //}
                    //if (attribute != null)
                    //{
                    //    WriteMethodTemplate(node, null, null, false, method, null, attribute, default, assignmentRhs);
                    //    return;
                    //}
                    //bool isStaticConvention = symbolType?.IsStaticCallConvention(_global) ?? false;
                    //if (node.Parent is AssignmentExpressionSyntax assign && assign.Left == node && node.Parent?.Parent is InitializerExpressionSyntax)
                    //{
                    //    //do not write this inside initializer expression
                    //}
                    ////TODO: While in dotnet, there is no "this" instance in a static method/property.
                    ////For Js however if we call a static method like System.AppDomain.method(), the object System.AppDomain becomes "this" in the context of the method
                    ////Perhaps in the future we can explore using the available this within the method to access other static members like this.staticField instead of rewriting to $.System.AppDomain.staticField
                    ////We can shrink the code further with this approach
                    //else if (!(symbolType!.IsStatic || isStaticConvention))
                    //    Writer.Write(node, "this.");
                    //var symbolMetadata = isPrimaryConstructorParameter ? null : _global.GetRequiredMetadata(symbolType!);
                    //Writer.Write(node, symbolMetadata?.InvocationName ?? symbolType!.Name);
                    //if (isStaticConvention)
                    //{
                    //    Writer.Write(node, ".call(this)");
                    //}
                    //else
                    //{
                    WriteMemberAccess(node, null, symbolType!.ContainingType, symbolType.Name, symbolType);
                    //}
                }
                else if (symbolType != null && symbolType.Kind != SymbolKind.TypeParameter && (symbolType is INamedTypeSymbol || symbolType is INamespaceSymbol))
                {
                    var symbolMetadata = _global.GetRequiredMetadata(symbolType);
                    Writer.Write(node, symbolMetadata.InvocationName ?? symbolType.Name);
                }
                else
                {
                    Writer.Write(node, Utilities.ResolveIdentifierName(identifier));
                }
                var refKind = symbolType?.GetRefKind();
                if (refKind != null && refKind != RefKind.None)
                {
                    bool NeedsDereferenceAccess()
                    {
                        return node.Parent is BinaryExpressionSyntax;
                    }
                    if (NeedsDereferenceAccess())
                    {
                        Writer.Write(node, $".{Constants.RefValueName}");
                    }
                }
                //dereference ref or out keyword ulness we are passing it as parameter
                //if (node.Parent is not ArgumentSyntax)
                //{
                //    var cd = GetIdentifierTypeInScope(node.Identifier.ValueText);
                //    if (cd.TypeSyntaxOrSymbol is IParameterSymbol parameter && (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out))
                //    {
                //        Writer.Write(node, ".");
                //        Writer.Write(node, Constants.RefValueName);
                //    }
                //}
                //base.VisitIdentifierName(node);
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            WriteIdentifier(node, node.Identifier);
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            WriteIdentifier(node, node.Identifier);
            ////var genericType = _global.TryGetTypeSymbol(node, this/*, out _, out _*/) ?? _global.GetTypeSymbol($"{node.Identifier.ValueText}<{string.Join(",", Enumerable.Range(1, node.Arity).Select(c => ""))}>", this/*, out _, out _*/);
            ////var metadata = _global.GetRequiredMetadata(genericType);
            ////Writer.Write(node, metadata?.OverloadName ?? node.Identifier.ValueText);
            ////Writer.Write(node, metadata?.InvocationName ?? node.Identifier.ValueText);
            ////if (metadata?.InvocationName == null)
            ////{
            //Writer.Write(node, "(");
            //int i = 0;
            //foreach (var n in node.TypeArgumentList.Arguments)
            //{
            //    if (i > 0)
            //        Writer.Write(node, ", ");
            //    Visit(n);
            //    i++;
            //}
            //Writer.Write(node, ")");
            ////}
            ////Writer.Write($"{ResolveFullTypeName(node.Identifier)}");
            ////foreach (var n in node.TypeArgumentList.Arguments)
            ////{
            ////    Visit(n);
            ////}
            ////Writer.Write($">");
            ////base.VisitGenericName(node);
        }

        public override void VisitQualifiedName(QualifiedNameSyntax node)
        {
            if (node.ToFullString().Contains("char.MinValue") || node.ToFullString().Contains("char.MaxValue"))
            {

            }
            var type = Global.TryGetTypeSymbol(node, this);
            if (type?.Kind == SymbolKind.Field)
            {
                string? memberName = null;
                //if (node.Right is GenericNameSyntax gn)
                //{
                //    memberName = gn.Identifier.Text;
                //}
                //else
                //{
                memberName = node.Right.Identifier.ValueText;
                //}
                WriteMemberAccess(node, node.Left, null, memberName, null);
            }
            else
            {
                string? initialNs = currentExpressionNamespace;
                Visit(node.Left);
                //dont write dot if the identifierName is collected/not written yet
                if (initialNs == currentExpressionNamespace)
                    Writer.Write(node, node.DotToken.ValueText);
                Visit(node.Right);
            }
            //base.VisitQualifiedName(node);
        }

    }
}
