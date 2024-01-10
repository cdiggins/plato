using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    /// <summary>
    /// Maps CSharp to Plato nodes, and keeps the semantic information. 
    /// </summary>
    public class PlatoSemanticMapping
    {
        private int _id;
        public int NextId => _id++;

        public Dictionary<int, SyntaxNode> IdsToSyntaxNode = new Dictionary<int, SyntaxNode>();
        public Dictionary<SyntaxNode, int> SyntaxNodesToIds = new Dictionary<SyntaxNode, int>();
        public Dictionary<int, PlatoSyntaxNode> Children { get; } = new Dictionary<int, PlatoSyntaxNode>();
        public Dictionary<int, ISymbol> IdsToSymbols = new Dictionary<int, ISymbol>();
        public IEnumerable<PlatoSyntaxNode> PlatoSyntaxNodes => Children.Values;
        public IEnumerable<SyntaxNode> CSharpSyntaxNodes => SyntaxNodesToIds.Keys;
        public Dictionary<SyntaxNode, SemanticModel> Models = new Dictionary<SyntaxNode, SemanticModel>();

        public SemanticModel Model { get; set; }

        public T Add<T>(T r, ISymbol symbol = null)
            where T : PlatoSyntaxNode
        {
            if (r == null)
                return null;
            if (symbol == null)
                return r;
            if (IdsToSymbols.ContainsKey(r.Id))
            {
                // This is called by various symbols, adding things and generating types.
                // Right now, the ID is not correct. 
                //Debug.Assert(IdsToSymbols[r.Id].Name == symbol.Name);
            }
            else
            {
                IdsToSymbols.Add(r.Id, symbol);
            }

            return r;
        }

        public static SyntaxNode GetSymbolNode(ISymbol symbol)
        {
            var location = symbol.Locations.FirstOrDefault();
            return location?.SourceTree?.GetRoot()?.FindNode(location.SourceSpan);
        }

        public PlatoSyntaxNode GetPlatoSyntaxNode(SyntaxNode node)
            => Children[SyntaxNodesToIds[node]];

        public T Add<T>(Func<T> f, SyntaxNode node = null)
            where T : PlatoSyntaxNode
            => Add(f(), node);

        public T Add<T>(T r, SyntaxNode node = null)
            where T : PlatoSyntaxNode
        {
            var id = r.Id;
            if (id >= 0 && node != null)
            {
                if (!Children.ContainsKey(id))
                    Children.Add(id, r);
                if (!IdsToSyntaxNode.ContainsKey(id))
                    IdsToSyntaxNode.Add(id, node);
                if(!SyntaxNodesToIds.ContainsKey(node))
                    SyntaxNodesToIds.Add(node, id);
                if (!Models.ContainsKey(node))
                    Models.Add(node, Model);
            }
            return r;
        }
    }
}