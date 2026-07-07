using System;
using Ara3D.Parakeet;

namespace Ara3D.Geometry.AST
{
    public class AstBuilder
    {
        public ILocation Location;

        public AstBuilder(ILocation location)
        {
            Location = location;
        }

        public AstIdentifier ToIdent(string text)
            => new AstIdentifier(Location, text);

        public AstVarDef ToVarDef(string name, AstNode value, AstTypeNode type = null)
            => new AstVarDef(Location, ToIdent(name), value, type);

        public AstLoop ToForLoop()
            => throw new NotImplementedException();

        public AstNode ToInc(string name)
            => throw new NotImplementedException();

        public AstNode ToInlineCall(AstLambda lambda, params AstNode[] arguments)
            => throw new NotImplementedException();

        // If I had "Map" in the Ast, then a Map/Map fusion would be easier.
        // If I had "Reduce" in the AST, then a Map/Reduce fusion would be easier as well. 
        // If I had "Filter" in the AST, then a Map/Filter/Reduce fusion would be easier. 
    }
}