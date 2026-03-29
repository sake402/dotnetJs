using NetJs.Translator.CSharpToJavascript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.ValueDeserializers;

namespace NetJs.Translator.CSharpToJavascript
{
    public partial class TranslatorSyntaxVisitor
    {
        bool IsFieldStructLayout(CSharpSyntaxNode? member, ISymbol? field, out int fieldOffset)
        {
            if (field == null && member == null)
                throw new InvalidOperationException("Expected one of member or field");
            field ??= _global.GetTypeSymbol(member!, this);
            if (field is IFieldSymbol fs && fs.IsConst)
            {
                fieldOffset = -1;
                return false;
            }
            if (_global.HasAttribute(field, typeof(FieldOffsetAttribute).FullName!, this, false, out var fieldOffsetAttribute))
            {
                var offsetArg = fieldOffsetAttribute![0];
                fieldOffset = (int)fieldOffsetAttribute[0];
            }
            else
            {
                LayoutKind layoutKind = field.ContainingType.IsValueType ? LayoutKind.Sequential : LayoutKind.Auto;
                if (_global.HasAttribute(field.ContainingType, typeof(StructLayoutAttribute).FullName, this, false, out var structLayoutAttribute))
                {
                    layoutKind = (LayoutKind)(int)structLayoutAttribute![0];
                }
                else
                {
                    fieldOffset = -1;
                    return false;
                }
                //var fields = field.ContainingType.GetMembers().Where(m =>
                //(m.Kind == SymbolKind.Field && ((IFieldSymbol)m).AssociatedSymbol == null) ||
                //(m.Kind == SymbolKind.Event) ||
                //(m.Kind == SymbolKind.Property && ((IPropertySymbol)m).IsAutoProperty()));
                switch (layoutKind)
                {
                    case LayoutKind.Auto:
                    //fields = fields.OrderBy(m => m.Name).ToList();
                    //break;
                    case LayoutKind.Sequential:
                        if (member != null)
                        {
                            var type = member.FindClosestParent<TypeDeclarationSyntax>() ?? throw new InvalidOperationException();
                            var members = type.Members.SelectMany(m =>
                            {
                                if (m is BaseFieldDeclarationSyntax vd)
                                {
                                    if (vd.Modifiers.IsConst())
                                        return [];
                                    return vd.Declaration.Variables.Cast<CSharpSyntaxNode>();
                                }
                                return [(CSharpSyntaxNode)m];
                            });
                            fieldOffset = Array.IndexOf(members.ToArray(), member);
                        }
                        else
                        {
                            var fields = field.ContainingType.GetMembers().Where(m =>
                                (m.Kind == SymbolKind.Field && ((IFieldSymbol)m).AssociatedSymbol == null && !((IFieldSymbol)m).IsConst) ||
                                (m.Kind == SymbolKind.Event) ||
                                (m.Kind == SymbolKind.Property && ((IPropertySymbol)m).IsAutoProperty()))
                                .OrderBy(f =>
                                {
                                    if (_global.HasAttribute(f, typeof(FieldOffsetAttribute).FullName!, this, false, out var fieldOffsetAttribute))
                                    {
                                        var offsetArg = fieldOffsetAttribute![0];
                                        return (int)fieldOffsetAttribute[0];
                                    }
                                    if (f.Kind == SymbolKind.Field)
                                        return int.MaxValue / 2;
                                    return int.MaxValue;
                                })
                                .ToArray();
                            fieldOffset = Array.IndexOf(fields, field);
                        }
                        if (fieldOffset == -1)
                            throw new InvalidOperationException();
                        break;
                    default:
                    case LayoutKind.Explicit:
                        fieldOffset = -1;
                        return false;
                        throw new InvalidOperationException("Must have FieldOffsetAttribute already");
                        break;
                }
                //fieldOffset = Array.IndexOf(fields.ToArray(), field);
                //foreach (var f in fields)
                //{
                //    if (SymbolEqualityComparer.Default.Equals(f, field))
                //        break;
                //    var fType = (f as IFieldSymbol)!.Type;
                //    fieldOffset += 1;//_global.GetTypeSizeInBytes(fType, this);
                //}
            }
            return true;
        }

        bool TryWriteFieldLayout(CSharpSyntaxNode member, ISymbol field, ITypeSymbol fieldType, string fieldName, string? modifier, string? comment)
        {
            int fieldOffset = 0;
            if (IsFieldStructLayout(member, field, out fieldOffset))
            {
                Writer.WriteLine(member, $"/*{comment}*/ {modifier} get {fieldName}() {{ return this.GetField({fieldOffset}); }}", true);
                Writer.WriteLine(member, $"/*{comment}*/ {modifier} set {fieldName}(value) {{ this.SetField({fieldOffset}, value); }}", true);
                return true;
            }
            return false;
        }

        void WriteField(BaseFieldDeclarationSyntax node)
        {
            //if (_global.HasAttribute(node, typeof(InlineConstAttribute).FullName!))
            //return;
            string? modifier = null;
            if (node.Modifiers.Any(e => e.ValueText == "static" || e.ValueText == "const"))
            {
                modifier += "static ";
            }
            foreach (var var in node.Declaration.Variables)
            {
                EnsureImported(node.Declaration.Type);
                IFieldSymbol? fieldSymbol = null;
                IEventSymbol? eventSymbol = null;
                var symbol = _global.GetTypeSymbol(var, this/*, out _, out _*/);
                fieldSymbol = symbol as IFieldSymbol;
                eventSymbol = symbol as IEventSymbol;
                if (fieldSymbol == null && eventSymbol == null)
                    throw new InvalidOperationException();
                ITypeSymbol type = fieldSymbol?.Type ?? eventSymbol?.Type!;
                if (_global.HasAttribute(symbol, typeof(InlineConstAttribute).FullName!, this, false, out _))
                    continue;
                if (_global.HasAttribute(symbol, typeof(TemplateAttribute).FullName!, this, false, out _))
                    continue;
                var fieldMetadata = _global.GetRequiredMetadata(symbol);
                var declaringSymbolMeta = _global.GetRequiredMetadata(symbol.ContainingSymbol);
                var fieldName = Utilities.ResolveIdentifierName(var.Identifier);
                //if (fieldSymbol != null)
                CurrentClosure.DefineIdentifierType(symbol.Name, CodeSymbol.From(symbol));
                //else
                //    CurrentClosure.DefineIdentifierType(fieldName, CodeType.From(node.Declaration.Type, SymbolKind.Field));

                var defaultValue = var.Initializer == null ? _global.GetDefaultValue(node.Declaration.Type, this) : null;

                bool useStaticPropertyFunction = false;
                //if ((node.Modifiers.IsStatic() || node.Modifiers.IsConst()) &&
                //    (defaultValue.EndsWith("()")/*eg T.default() ot Guid.default()*/ ||
                //    (var.Initializer?.Value != null && var.Initializer?.Value is not LiteralExpressionSyntax)))
                //{
                //    useStaticPropertyFunction = true;
                //    Writer.WriteLine(node, $"static $_{fieldMetadata.OverloadName ?? fieldName};", true);
                //}
                bool isLiteralInit = MemberIsLiteralInitialization(var.Initializer, type);
                bool isFieldLayout = _global.HasAttribute(symbol.ContainingType, typeof(StructLayoutAttribute).FullName!, this, false, out _);
                if (var.Initializer != null || type.SpecialType == SpecialType.System_ValueType)
                {
                    //If we initialize a field from a primary constructor parameter, this is already handled in the primary constructor generator (WritePrimaryConstructor)
                    //We should skip it here
                    if (MemberWasInitializedByPrimaryConstructor(var, var.Initializer))
                    {
                    }
                    else if (isLiteralInit && !isFieldLayout)
                    {
                        //use inline init for literal init value
                    }
                    else
                    {
                        bool isStaticInit = node.Modifiers.IsStatic() || node.Modifiers.IsConst();
                        CurrentClosure.RegisterTypeInitializer(() =>
                        {
                            //If we are in a static initilizer, it is safe to use this as it reference the class prototype itself
                            Writer.Write(node, $"{(isStaticInit ? "this."/*declaringSymbolMeta.InvocationName + "."*/ : "this.")}{fieldName}", true);
                            //Visit(var.Initializer);
                            if (var.Initializer != null)
                            {
                                Writer.Write(node, " = ");
                                if (!TryWriteConstant(node, type, var.Initializer.Value))
                                    WriteVariableAssignment(node, null, symbol, null, var.Initializer.Value, null);
                            }
                            if (type.SpecialType == SpecialType.System_ValueType)
                            {
                                Writer.Write(node, $"new {type.ComputeOutputTypeName(_global)}()");
                            }
                            Writer.WriteLine(node, ";");
                        }, isStaticInit);
                    }
                }
                if (isFieldLayout && TryWriteFieldLayout(var, fieldSymbol ?? (ISymbol)eventSymbol!, fieldSymbol?.Type ?? eventSymbol!.Type, fieldMetadata.OverloadName ?? fieldName, modifier, $"{node.Declaration.Type.ToFullString().Trim()}"))
                {
                }
                else
                {
                    Writer.Write(node, $"/*{node.Declaration.Type.ToFullString().Trim()}*/ {modifier}{(useStaticPropertyFunction ? "get " : "")}{fieldMetadata.OverloadName ?? fieldName}", true);
                    //if (useStaticPropertyFunction)
                    //{
                    //    Writer.Write(node, $"() {{ return {(node.Modifiers.IsConst() || node.Modifiers.IsStatic() ? $"{declaringSymbolMeta.InvocationName}." : "")}$_{fieldMetadata.OverloadName ?? fieldName} ??= ");
                    //}
                    if (isLiteralInit)
                    {
                        Writer.Write(node, " = ");
                        if (!TryWriteConstant(node, type, var.Initializer!.Value))
                            WriteVariableAssignment(node, null, symbol, null, var.Initializer!.Value, null);
                    }
                    else
                    {
                        if (defaultValue != null)
                        {
                            if (!useStaticPropertyFunction)
                                Writer.Write(node, " = ");
                            Writer.Write(node, defaultValue);
                        }
                    }
                    if (useStaticPropertyFunction)
                    {
                        Writer.WriteLine(node, $"; }}");
                    }
                    else
                    {
                        Writer.WriteLine(node, $";");
                    }
                }
            }
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            WriteField(node);
            //base.VisitFieldDeclaration(node);
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            WriteField(node);
            //base.VisitEventFieldDeclaration(node);
        }

    }
}
