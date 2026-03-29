using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace NetJs.Translator.CSharpToJavascript
{
    class AssociateSyntaxFactoryNewNodeVisitor : CSharpSyntaxVisitor, IDisposable
    {
        Dictionary<SyntaxNode, SyntaxNode> _factoryAssociationNode;
        List<SyntaxNode> toRemove = new List<SyntaxNode>();
        SyntaxNode original;
        SyntaxNode newNode;

        SyntaxNode currentNewNode;

        public AssociateSyntaxFactoryNewNodeVisitor(Dictionary<SyntaxNode, SyntaxNode> factoryAssociationNode, SyntaxNode original, SyntaxNode newNode)
        {
            _factoryAssociationNode = factoryAssociationNode;
            this.original = original;
            this.newNode = newNode;
            currentNewNode = newNode;
        }

        public override void DefaultVisit(SyntaxNode originalNode)
        {
            Debug.Assert(originalNode.GetType() == currentNewNode.GetType());
            _factoryAssociationNode.Add(currentNewNode, originalNode);
            toRemove.Add(currentNewNode);
            var originalChildren = originalNode.ChildNodes();
            var newChildren = currentNewNode.ChildNodes();
            Debug.Assert(originalChildren.Count() == newChildren.Count());
            int ix = 0;
            foreach (var originalChild in originalChildren)
            {
                var newChild = newChildren.ElementAt(ix);
                Debug.Assert(newChild.GetType() == originalChild.GetType());
                //_factoryAssociationNode[newNode] = oldNode;
                currentNewNode = newChild;
                Visit(originalChild);
                ix++;
            }
            //base.DefaultVisit(node);
        }

        public void Dispose()
        {
            toRemove.ForEach(t => _factoryAssociationNode.Remove(t));
        }
    }
}
