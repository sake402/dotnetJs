using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.String
{
    sealed class MaterializeFastAllocatedStringOnAssignmentSyntaxEmitter : SyntaxEmitter<AssignmentExpressionSyntax>
    {
        public override bool TryEmit(AssignmentExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {

            return false;
        }
    }
}
