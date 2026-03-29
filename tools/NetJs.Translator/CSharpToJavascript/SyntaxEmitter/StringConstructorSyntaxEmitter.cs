using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    sealed class StringConstructorSyntaxEmitter : SyntaxEmitter<ObjectCreationExpressionSyntax>
    {
        public override bool TryEmit(ObjectCreationExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            return false;
        }
    }
}
