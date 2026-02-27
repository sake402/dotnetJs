using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetJs.Translator.CSharpToJavascript.SyntaxEmitter
{
    //Handles expression like pointer[2] where pointer is a pointer type
    sealed class PointerArrayElementAccessSyntaxEmitter : SyntaxEmitter<ElementAccessExpressionSyntax>
    {
        public override bool TryEmit(ElementAccessExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            var type = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Expression), visitor)?.GetTypeSymbol();
            if (type != null)
            {
                if (type.IsPointer(out var pointedType))
                {
                    visitor.Visit(node.Expression);
                    visitor.Writer.Write(node, ".get_Item(");
                    int ix = 0;
                    foreach (var arg in node.ArgumentList.Arguments)
                    {
                        if (ix > 0)
                            visitor.Writer.Write(node, ", ");
                        visitor.Visit(arg);
                        ix++;
                    }
                    visitor.Writer.Write(node, ")");
                    visitor.Writer.Write(node, ".");
                    visitor.Writer.Write(node, Constants.RefValueName);
                    return true;
                }
            }
            return false;
        }
    }
}
