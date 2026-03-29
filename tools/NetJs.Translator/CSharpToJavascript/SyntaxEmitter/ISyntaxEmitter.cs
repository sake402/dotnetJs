using Microsoft.CodeAnalysis;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    public interface ISyntaxEmitter
    {
        Type SyntaxType { get; }
        bool TryEmit(SyntaxNode node, TranslatorSyntaxVisitor visitor);
    }

    public interface ISyntaxEmitter<TSyntax> : ISyntaxEmitter where TSyntax : SyntaxNode
    {
        bool TryEmit(TSyntax node, TranslatorSyntaxVisitor visitor);
    }
}
