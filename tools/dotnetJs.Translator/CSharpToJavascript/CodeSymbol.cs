using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public struct CodeSymbol
    {
        public CodeSymbol()
        {
        }

        public object? TypeSyntaxOrSymbol { get; private set; }
        //public object? ConvertedType { get; private set; }
        public SymbolKind Kind { get; set; } = SymbolKind.ErrorType;
        public object? Tag { get; set; }
        public static CodeSymbol From(BaseTypeDeclarationSyntax? ts) => new CodeSymbol() { TypeSyntaxOrSymbol = ts };
        public static CodeSymbol From(TypeSyntax? ts, SymbolKind kind) => new CodeSymbol() { TypeSyntaxOrSymbol = ts, Kind = kind };
        public static CodeSymbol From(TypeParameterSyntax? ts, SymbolKind kind) => new CodeSymbol() { TypeSyntaxOrSymbol = ts, Kind = kind };
        public static CodeSymbol From(ISymbol? ts) => new CodeSymbol() { TypeSyntaxOrSymbol = ts };
        //public static CodeType From(IParameterSymbol? ts) => new CodeType() { TypeSyntaxOrSymbol = ts };
        public static CodeSymbol From(MemberSymbolOverload overloads) => new CodeSymbol() { TypeSyntaxOrSymbol = overloads };

        public override string? ToString()
        {
            if (TypeSyntaxOrSymbol != null)
                return (Kind != SymbolKind.ErrorType ? Kind.ToString() + ": " : "") + TypeSyntaxOrSymbol?.ToString();
            return base.ToString();
        }
    }
}