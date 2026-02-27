using Microsoft.CodeAnalysis;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    public abstract class SyntaxEmitter<TSyntax> : ISyntaxEmitter<TSyntax> where TSyntax : SyntaxNode
    {
        public Type SyntaxType => typeof(TSyntax);
        public abstract bool TryEmit(TSyntax node, TranslatorSyntaxVisitor visitor);
        public bool TryEmit(SyntaxNode node, TranslatorSyntaxVisitor visitor) => TryEmit((TSyntax)node, visitor);
    }
}
