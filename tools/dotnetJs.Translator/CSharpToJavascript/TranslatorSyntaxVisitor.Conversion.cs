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
        bool TryCastUsingExternalInterface(CSharpSyntaxNode node, ITypeSymbol? lhsType, ITypeSymbol? rhsType, CSharpSyntaxNode rhsExpression)
        {
            //if we are trying to assign an Extern type like native js Array to a dotnet type like IEnumerable. Let the runtime cast system handle the conversion
            //if (lhsType is ITypeSymbol lt && rhsType is ITypeSymbol rt && rhsType.CanConvertTo(lhsType, _global, null, out _) < 0)
            //{
            //if we are trying to assign an Extern type like native js Array to a dotnet type like IEnumerable
            INamedTypeSymbol? castHandler = null;
            TypeArgumentListSyntax? castGenericArguments = null;
            if (lhsType != null &&
                rhsType != null &&
                lhsType.SpecialType != SpecialType.System_Object &&
                !lhsType.Equals(_global.Compilation.DynamicType, SymbolEqualityComparer.Default) &&
                !lhsType.Equals(rhsType, SymbolEqualityComparer.Default) &&
                _global.HasAttribute(rhsType, typeof(ExternalInterfaceImplementationAttribute).FullName!, this, true, out var constructorsArgs))
            {
                var castType = (INamedTypeSymbol)constructorsArgs![0]!;
                if (castType.IsUnboundGenericType)
                {
                    rhsType.IsArray(out var elementType);
                    if (elementType != null)
                    {
                        castType = castType.OriginalDefinition.Construct(elementType);
                    }
                }
                if (castType.CanConvertTo(lhsType, _global, null, out _) > 0)
                {
                    castHandler = castType;
                    //var systemObject = (ITypeSymbol)_global.GetTypeSymbol("System.Object", this, out _, out _);
                    //castHandler = castHandler.OriginalDefinition.Construct(Enumerable.Range(1, castHandler.Arity).Select(i => systemObject).ToArray());
                    //castGenericArguments = SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(Enumerable.Range(1, castHandler.Arity).Select(e => SyntaxFactory.ParseTypeName("object"))));
                }
            }
            if (castHandler != null)
            {
                WriteConstructorCall(node, castHandler, castHandler.GetMembers(".ctor").Cast<IMethodSymbol>().First(), castGenericArguments, [rhsExpression as ArgumentSyntax ?? SyntaxFactory.Argument((ExpressionSyntax)rhsExpression)]);
                return true;
            }
            return false;
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            var parentIsEnum = node.FindClosestParent<EnumDeclarationSyntax>();
            //Don't generate cast for enum value initialization
            var type = _global.GetTypeSymbol(node.Type, this/*, out _, out _*/);
            if (parentIsEnum != null || node.Expression.IsKind(SyntaxKind.NullLiteralExpression) || !_global.ShouldExportType(type, this))
            {
                Visit(node.Expression);
            }
            else
            {
                var fromType = _global.ResolveSymbol(GetExpressionReturnSymbol(node.Expression), this)?.GetTypeSymbol();
                var toType = _global.GetTypeSymbol(node.Type, this/*, out _, out _*/).GetTypeSymbol();
                if (fromType != null && toType != null /*&& _global.Compilation.HasImplicitConversion(fromType, toType)*/ && fromType.IsJsNativeIntegerNumeric() && toType.IsJsNativeIntegerNumeric())
                {
                    //this type can just be assigned, no cast/conversion neccessary in js
                    Visit(node.Expression);
                    return;
                }
                if (TryInvokeMethodOperator(node, ExplicitOperatorName, null, node.Type, [node.Expression]))
                    return;
                if (TryCastUsingExternalInterface(node, toType, fromType, node.Expression))
                    return;
                //if ((toType?.Equals(_global.Compilation.ObjectType, SymbolEqualityComparer.Default) ?? false) && fromType != null && !fromType.IsValueType)
                //{
                    //cast reference type to object can just be assigned, no cast/conversion neccessary in js
                if (fromType != null && toType != null && fromType.CanConvertTo(toType, _global, null, out _) > 0)
                {
                    Visit(node.Expression);
                    return;
                }
                EnsureImported(node.Type);
                Writer.Write(node, $"{_global.GlobalName}.{Constants.CastName}(");
                Visit(node.Expression);
                Writer.Write(node, ", ");
                Visit(node.Type);
                Writer.Write(node, ")");
            }
            //base.VisitCastExpression(node);
        }

    }
}
