//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace dotnetJs.Translator.CSharpToJavascript
//{
//    public partial class TranslatorSyntaxVisitor
//    {
//        bool HasAnyAttribute(MemberDeclarationSyntax node, params string[] attributeNames)
//        {
//            var symbols = attributeNames.Select(s => _global.GetTypeSymbol(s, this, out _, out _)).ToList();
//            if (node.AttributeLists.SelectMany(a => a.Attributes).Any(a =>
//            {
//                var aName = a.Name.GetText().ToString();
//                if (!aName.EndsWith("Attribute"))
//                    aName += "Attribute";
//                var aSymbol = _global.GetTypeSymbol(aName, this, out _, out _);
//                return symbols.Contains(aSymbol);
//            }))
//                return true;
//            //if (inherits && node.BaseList!= null)
//            //{
//            //    return node.BaseList.Types.Any(b=>)
//            //}
//            return false;
//        }

//        bool HasAnyAttribute(ISymbol node, bool inherits, params string[] attributeNames)
//        {
//            var symbols = attributeNames.Select(s => _global.GetTypeSymbol(s, this, out _, out _)).ToList();
//            if (node.GetAttributes().Select(a => a.AttributeClass).Where(e => e != null).Any(a =>
//            {
//                var aName = a!.ToString()!;
//                if (!aName.EndsWith("Attribute"))
//                    aName += "Attribute";
//                var aSymbol = _global.GetTypeSymbol(aName, this, out _, out _);
//                return symbols.Contains(aSymbol);
//            }))
//                return true;
//            if (inherits && node is ITypeSymbol ns && ns.BaseType != null)
//            {
//                return HasAnyAttribute(ns.BaseType, inherits, attributeNames);
//            }
//            return false;
//        }

//        bool HasAttribute(MemberDeclarationSyntax node, bool inherits, string attributeName)
//        {
//            attributeName = attributeName.Substring(0, attributeName.Length - 9);
//            if (node.AttributeLists.SelectMany(a => a.Attributes).Any(a => attributeName == a.Name.GetText().ToString()))
//                return true;
//            return false;
//        }

//        bool HasAttribute(ISymbol symbol, string attributeName, bool inherits, out object[]? constructorArgs)
//        {
//            constructorArgs = null;
//            object[]? cArgs = null;
//            var attrSymbol = _global.GetTypeSymbol(attributeName, this, out _, out _);
//            if (symbol.GetAttributes().Select(a => (a, a.AttributeClass)).Where(e => e.AttributeClass != null).Any(a =>
//            {
//                var aName = a.AttributeClass!.ToString()!;
//                if (!aName.EndsWith("Attribute"))
//                    aName += "Attribute";
//                var aSymbol = _global.GetTypeSymbol(aName, this, out _, out _);
//                if (attrSymbol.Equals(aSymbol, SymbolEqualityComparer.Default))
//                {
//                    cArgs = a.a.ConstructorArguments.Select(c => c.Value!).ToArray();
//                    return true;
//                }
//                return false;
//            }))
//            {
//                constructorArgs = cArgs;
//                return true;
//            }
//            if (inherits && symbol is ITypeSymbol ns && ns.BaseType != null)
//            {
//                return HasAttribute(ns.BaseType, attributeName, inherits, out constructorArgs);
//            }
//            return false;
//        }

//        bool ShouldExportType(MemberDeclarationSyntax node)
//        {
//            return !HasAnyAttribute(node, typeof(ExternalAttribute).FullName,  typeof(ExternalInterfaceAttribute).FullName, typeof(NonScriptableAttribute).FullName,  typeof(ObjectLiteralAttribute).FullName);
//        }


//        bool ShouldExportType(ISymbol symbol)
//        {
//            if (symbol.IsExtern)
//                return false;
//            return !HasAnyAttribute(symbol, false, typeof(ExternalAttribute).FullName,  typeof(ExternalInterfaceAttribute).FullName, typeof(NonScriptableAttribute).FullName,  typeof(ObjectLiteralAttribute).FullName);
//        }
//    }
//}
