using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript
{
    public struct MemberSymbolOverload()
    {
        public IEnumerable<ISymbol> Overloads { get; set; } = Enumerable.Empty<ISymbol>();

        public IMethodSymbol? ResolveMethod(TranslatorSyntaxVisitor visitor, TypeArgumentListSyntax? explicitGenericArgs, ArgumentListSyntax? parameterArgs, out MethodOverloadResult overloadResult)
        {
            var lhsSymbol = Overloads.Where(e => e is IMethodSymbol).First().ContainingSymbol;
            var method = visitor.GetBestOverloadMethod((ITypeSymbol)lhsSymbol, Overloads.Where(e => e is IMethodSymbol).Cast<IMethodSymbol>(), explicitGenericArgs, parameterArgs?.Arguments, null, out overloadResult);
            return method;
        }

        public ISymbol? ResolveMember(TranslatorSyntaxVisitor visitor)
        {
            return Overloads.Where(e => e is not IMethodSymbol).First();
        }
    }
}