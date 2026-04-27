using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetJs.Translator.CSharpToJavascript.SyntaxEmitter.Pointer
{
    //Handles expression like pointer[2] = value where pointer is a pointer type
    sealed class PointerArrayElementSetAccessSyntaxEmitter : SyntaxEmitter<AssignmentExpressionSyntax>
    {
        //TODO: Rewrite this to use GetAt or SetAt depending on whether being read or assigned
        //Current implementation do both automagically, but it allocate a temp reference on heap(returned by get_Item) and slower
        public override bool TryEmit(AssignmentExpressionSyntax node, TranslatorSyntaxVisitor visitor)
        {
            if (node.Left is ElementAccessExpressionSyntax elementAccess)
            {
                var type = visitor.Global.ResolveSymbol(visitor.GetExpressionReturnSymbol(elementAccess.Expression), visitor)?.GetTypeSymbol();
                if (type != null)
                {
                    if (type.IsPointer(out _))
                    {
                        visitor.Visit(elementAccess.Expression);
                        visitor.CurrentTypeWriter.Write(node, ".SetAt(");
                        int ix = 0;
                        visitor.Visit(node.Right);
                        visitor.CurrentTypeWriter.Write(node, ", ");
                        foreach (var arg in elementAccess.ArgumentList.Arguments)
                        {
                            if (ix > 0)
                                visitor.CurrentTypeWriter.Write(node, ", ");
                            visitor.Visit(arg);
                            ix++;
                        }
                        visitor.CurrentTypeWriter.Write(node, ")");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
