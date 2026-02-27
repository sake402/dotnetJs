using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace dotnetJs.Translator.CSharpToJavascript
{
    public class SingleNodeReplacer : CSharpSyntaxRewriter
    {
        CSharpSyntaxNode _targetNode;
        CSharpSyntaxNode _newNode;

        public SingleNodeReplacer(CSharpSyntaxNode targetNode, CSharpSyntaxNode newNode)
        {
            _targetNode = targetNode;
            _newNode = newNode;
        }

        public override SyntaxNode? DefaultVisit(SyntaxNode node)
        {
            if (node == _targetNode)
                return _newNode;
            return node;
        }

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node == _targetNode)
                return _newNode;
            if (node != null)
            {
                var result = ((CSharpSyntaxNode)node).Accept(this);
                return result!;
            }
            else
            {
                return null;
            }
        }
    }
}