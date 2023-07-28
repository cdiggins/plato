using Plato.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
{
    public class MemberCandidate
    {
        public TypeDefSymbol Type { get; }
        public MemberDefSymbol Member { get; }
        public override string ToString()
        {
            return $"{Type.UniqueName}.{Member.UniqueName}";
        }

        public MemberCandidate(TypeDefSymbol type, MemberDefSymbol member)
            => (Type, Member) = (type, member);
    }

    public class Operations
    {
        public Dictionary<string, List<MemberCandidate>> Lookup { get; } 
            = new Dictionary<string, List<MemberCandidate>>();

        public IReadOnlyList<TypeDefSymbol> Types { get; }
        
        public Operations(IEnumerable<TypeDefSymbol> typeDefs)
        {
            Types = typeDefs.ToList();
            foreach (var typeDefSymbol in Types)
                AddMembers(typeDefSymbol);
        }

        public IReadOnlyList<MemberCandidate> GetMembers(string name)
            => Lookup.ContainsKey(name) ? Lookup[name] : new List<MemberCandidate>();


        public void AddMember(TypeDefSymbol type, MemberDefSymbol member)
        {
            if (!Lookup.ContainsKey(member.Name))
                Lookup.Add(member.Name, new List<MemberCandidate>());
            Lookup[member.Name].Add(new MemberCandidate(type, member));
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