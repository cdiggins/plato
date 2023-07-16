using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class Operations
    {
        public Dictionary<string, TypeOperations> Lookup { get; }

        public Operations(IEnumerable<TypeDefSymbol> typeDefs)
        {
            Lookup = typeDefs.ToDictionary(td => td.Name, td => new TypeOperations(td));
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