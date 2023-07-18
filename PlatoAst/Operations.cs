using System.Collections.Generic;

namespace PlatoAst
{
    public class Operations
    {
        public Dictionary<string, List<(TypeDefSymbol, MemberDefSymbol)>> Lookup { get; } 
            = new Dictionary<string, List<(TypeDefSymbol, MemberDefSymbol)>>();
        
        public Operations(IEnumerable<TypeDefSymbol> typeDefs)
        {
            foreach (var typeDefSymbol in typeDefs)
                AddMembers(typeDefSymbol);
        }

        public static string LookupName(MemberDefSymbol member)
        {
            if (member is MethodDefSymbol mds)
                return $"{mds.Name}#{mds.Function.Parameters.Count}";
            return $"{member.Name}#0";
        }

        public void AddMember(TypeDefSymbol type, MemberDefSymbol member)
        {
            var name = LookupName(member);
            if (!Lookup.ContainsKey(name))
                Lookup.Add(name, new List<(TypeDefSymbol, MemberDefSymbol)>());
            Lookup[name].Add((type, member));
        }

        public void AddMembers(TypeDefSymbol type)
        {
            if (type == null)
                return;

            foreach (var method in type.Methods)
                AddMember(type, method);

            foreach (var field in type.Fields)
                AddMember(type, field);
        }
    }
}