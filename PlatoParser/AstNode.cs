using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatoParser
{
    public interface ITree<T> where T:ITree<T>
    {
        IReadOnlyList<T> Children { get; } 
    }

    public static class TreeHelpers
    {
        public static TAcc Aggregate<T, TAcc>(this ITree<T> tree, TAcc init, Func<TAcc, T, TAcc> func) where T:ITree<T>
            => tree.Children.Aggregate(init, (acc, curr) => curr.Aggregate(acc, func)); 
    }

    public class AstNode : ITree<AstNode>
    {
        public int Count => Children.Count;
        public AstNode this[int index] => Children[index];
        public AstNode(IReadOnlyList<AstNode> children) => Children = children;
        public IReadOnlyList<AstNode> Children { get; }
        public virtual AstNode Transform(Func<AstNode, AstNode> f) => throw new NotImplementedException();
        public static implicit operator AstNode(string text) => new AstLeaf(text);
        public bool IsLeaf => this is AstLeaf;
        public override string ToString() => $"[{GetType().Name}: {string.Join(" ", Children)}]";
    }

    public class AstSequence : AstNode
    {
        public AstSequence(params AstNode[] children) : base(children) { }
    }

    public class AstChoice: AstNode
    {
        public AstChoice(params AstNode[] children) : base(children) { }
        public AstNode Node => Children[0];
    }

    public class AstLeaf : AstNode
    {
        public string Text { get; }
        public AstLeaf(string text) : base(Array.Empty<AstNode>()) => Text = text;
        public override string ToString() => Text;
    }

    public class AstZeroOrMore<T> : AstNode where T : AstNode
    {
        public new T this[int index] => (T)Children[index];
        public AstZeroOrMore(T node) : base(new[] { node }) { }
    }
   
    public class AstOptional<T> : AstNode where T : AstNode
    {
        public T Node => (T)Children[0];
        public AstOptional(T node) : base(new[] { node }) { }
        public static implicit operator T(AstOptional<T> self) => self.Node;
    }
}
