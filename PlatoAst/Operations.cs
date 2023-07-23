using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class Operations
    {
        public Dictionary<string, List<(TypeDefSymbol, MemberDefSymbol)>> Lookup { get; } 
            = new Dictionary<string, List<(TypeDefSymbol, MemberDefSymbol)>>();

        public IReadOnlyList<TypeDefSymbol> Types { get; }
        
        public Operations(IEnumerable<TypeDefSymbol> typeDefs)
        {
            Types = typeDefs.ToList();
            foreach (var typeDefSymbol in Types)
                AddMembers(typeDefSymbol);
        }

        public IReadOnlyList<(TypeDefSymbol, MemberDefSymbol)> GetMembers(string name)
            => Lookup[name];

        public IReadOnlyList<(TypeDefSymbol, MemberDefSymbol)> GetMembers(string name, int parameterCount)
            => Lookup.TryGetValue(LookupName(name, parameterCount), out var result) 
                ? result 
                : new List<(TypeDefSymbol, MemberDefSymbol)>();

        public static string LookupName(string name, int parameterCount)
            //=> $"{name}#{parameterCount}";
            => $"{name}";

        public static string LookupName(MemberDefSymbol member)
        {
            if (member is MethodDefSymbol mds)
                return LookupName(mds.Name, mds.Function.Parameters.Count);
            return LookupName(member.Name, 1);
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