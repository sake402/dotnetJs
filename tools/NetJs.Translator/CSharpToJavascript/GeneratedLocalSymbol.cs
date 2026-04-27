using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;

namespace NetJs.Translator.CSharpToJavascript
{
#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
    class GeneratedLocalSymbol : ILocalSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public GeneratedLocalSymbol(ITypeSymbol type, string name)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Type = type;
            Name = name;
        }
        public ITypeSymbol Type { get;}
        public NullableAnnotation NullableAnnotation { get; }
        public bool IsConst { get; }
        public bool IsRef { get; }
        public RefKind RefKind { get; }
        public ScopedKind ScopedKind { get; }
        public bool HasConstantValue { get; }
        public object? ConstantValue { get; }
        public bool IsFunctionValue { get; }
        public bool IsFixed { get; }
        public bool IsForEach { get; }
        public bool IsUsing { get; }
        public SymbolKind Kind => SymbolKind.Local;
        public string Language { get; }
        public string Name { get; }
        public string MetadataName { get; }
        public int MetadataToken { get; }
        public ISymbol ContainingSymbol { get; }
        public IAssemblySymbol ContainingAssembly { get; }
        public IModuleSymbol ContainingModule { get; }
        public INamedTypeSymbol ContainingType { get; }
        public INamespaceSymbol ContainingNamespace { get; }
        public bool IsDefinition { get; }
        public bool IsStatic { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsAbstract { get; }
        public bool IsSealed { get; }
        public bool IsExtern { get; }
        public bool IsImplicitlyDeclared { get; }
        public bool CanBeReferencedByName { get; }
        public ImmutableArray<Location> Locations { get; }
        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
        public Accessibility DeclaredAccessibility { get; }
        public ISymbol OriginalDefinition => this;
        public bool HasUnsupportedMetadata { get; }

        public void Accept(SymbolVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }

        public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISymbol? other, SymbolEqualityComparer equalityComparer)
        {
#pragma warning disable RS1024 // Symbols should be compared for equality
            return other == this;
#pragma warning restore RS1024 // Symbols should be compared for equality
        }

        public bool Equals(ISymbol? other)
        {
#pragma warning disable RS1024 // Symbols should be compared for equality
            return other == this;
#pragma warning restore RS1024 // Symbols should be compared for equality
        }

        public ImmutableArray<AttributeData> GetAttributes()
        {
            throw new NotImplementedException();
        }

        public string? GetDocumentationCommentId()
        {
            throw new NotImplementedException();
        }

        public string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null)
        {
            throw new NotImplementedException();
        }

        public string ToDisplayString(SymbolDisplayFormat? format = null)
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
        {
            throw new NotImplementedException();
        }

        public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
        {
            throw new NotImplementedException();
        }
    }
}
