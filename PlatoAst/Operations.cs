using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class Operations
    {
        public IEnumerable<TypeDefSymbol> TypeDefSymbols => TypeLookup.Values.Select(v => v.Type);

        public Dictionary<string, TypeOperations> TypeLookup { get; } = new Dictionary<string, TypeOperations>();

        public Operations(IEnumerable<TypeDefSymbol> typeDefs)
        {
            foreach (var typeDefSymbol in typeDefs)
            {
                if (typeDefSymbol.Kind != "module")
                {
                    TypeLookup.Add(typeDefSymbol.Name, new TypeOperations(typeDefSymbol));
                }
            }
        }
    }

    public class TypeOperations
    {
        public TypeDefSymbol Type { get; }
        public List<Symbol> Members = new List<Symbol>();

        public void AddMembers(TypeDefSymbol type)
        {
            if (type == null)
                return;

            foreach (var method in type.Methods)
            {
                Members.Add(method);
            }

            foreach (var field in type.Fields)
            {
                Members.Add(field);
            }

            foreach (var tmp in type.Inherits)
            {
                if (tmp != null)
                    AddMembers(tmp.Def);
            }

            foreach (var tmp in type.Implements)
            {
                if (tmp != null)
                    AddMembers(tmp.Def);
            }
        }

        public TypeOperations(TypeDefSymbol type)
        {
            Type = type;
            AddMembers(type);
        }
    }
}