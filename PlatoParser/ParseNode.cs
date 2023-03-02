using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlatoParser
{
    /// <summary>
    /// A node generated from successful "NodeRule" parses. 
    /// These nodes are stored as a linked list, and can be converted 
    /// to a tree representation after all parsing is completed. 
    /// </summary>
    public class ParseNode
    {
        public int Start { get; }
        public int End { get; }
        public Rule Rule { get; }
        public string Name => Rule.Name;
        public ParseNode Previous { get; }
        public ParserInput Input { get; }
        public int Count => Math.Max(End - Start, 0);
        public string Contents => Input.Text.Substring(Start, Count);
        
        public override string ToString()
            => $"Node {Name}:{Start}:{End}:{EllidedContents}";

        public const int MaxLength = 20;

        public string EllidedContents
            => Count < MaxLength
            ? Contents : $"{Contents.Substring(0, MaxLength - 1)}...";

        public ParseNode(ParserInput input, Rule rule, int start, int end, ParseNode previous)
            => (Input, Rule, Start, End, Previous) = (input, rule, start, end, previous);

        public ParseTree ToParseTree()
            => ToParseTreeAndNode().Item1;

        public (ParseTree, ParseNode) ToParseTreeAndNode()
        {
            var node = this;
            if (node == null) return (null, null);
            var prev = node.Previous;
            var children = new List<ParseTree>();
            while (prev != null && node.IsParentOf(prev))
            {
                ParseTree child;
                (child, prev) = prev.ToParseTreeAndNode();
                children.Add(child);
            }
            children.Reverse();
            return (new ParseTree(node, children), prev);
        }

        public IReadOnlyList<ParseNode> AllPreviousNodes()
        {
            var r = new List<ParseNode>();
            for (var node=this; node != null; node = node.Previous)
            {
                r.Add(node);
            }
            r.Reverse();
            return r;
        }

        public bool IsChildOf(ParseNode parent)
            => parent.IsParentOf(this);

        public bool IsParentOf(ParseNode other)
        {
            var node = this;
            if (node == null || other == null) return false;
            if (other.Start >= node.End) return false;
            if (other.End <= node.Start) return false;

            // In this case it was a child
            if (other.Start >= node.Start)
            {
                Debug.Assert(node.End >= node.End);
                return true;
            }

            // Otherwise it was a sibling
            Debug.Assert(other.End <= node.Start);
            return false;
        }
    }
}