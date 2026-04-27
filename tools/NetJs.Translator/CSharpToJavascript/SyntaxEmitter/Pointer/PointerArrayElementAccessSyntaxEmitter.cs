using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Pointer
{
    //Handles expression like pointer[2] where pointer is a pointer type
    sealed class PointerArrayElementAccessSyntaxEmitter : SyntaxEmitter<ElementAccessExpressionSyntax>
    {
        //Current implementation do both automagically, but it allocate a temp reference on heap(returned by get_Item) and slower
        public override bool TryEmit(ElementAccessExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            var type = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(node.Expression), visitor)?.GetTypeSymbol();
            if (type != null)
            {
                if (type.IsPointer(out var pointedType))
                {
                    bool isGet = node.IsReadOnlyOperation();
                    if (isGet)
                    {
                        visitor.Visit(node.Expression);
                        visitor.CurrentTypeWriter.Write(node, ".GetAt(");
                        int ix = 0;
                        foreach (var arg in node.ArgumentList.Arguments)
                        {
                            if (ix > 0)
                                visitor.CurrentTypeWriter.Write(node, ", ");
                            visitor.Visit(arg);
                            ix++;
                        }
                        visitor.CurrentTypeWriter.Write(node, ")");
                    }
                    else
                    {
                        visitor.Visit(node.Expression);
                        visitor.CurrentTypeWriter.Write(node, ".get_Item(");
                        int ix = 0;
                        foreach (var arg in node.ArgumentList.Arguments)
                        {
                            if (ix > 0)
                                visitor.CurrentTypeWriter.Write(node, ", ");
                            visitor.Visit(arg);
                            ix++;
                        }
                        visitor.CurrentTypeWriter.Write(node, ")");
                        visitor.TryDereference(node);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
