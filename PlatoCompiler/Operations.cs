using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler
{
    public class MemberCandidate
    {
        public TypeDefinition Type { get; }
        public MemberDefinition Member { get; }
        public override string ToString()
        {
            return $"{Type.Name}.{Type.Id}.{Member.UniqueName}";
        }

        public MemberCandidate(TypeDefinition type, MemberDefinition member)
            => (Type, Member) = (type, member);
    }

    public class Operations
    {
        public Dictionary<string, List<MemberCandidate>> Lookup { get; } 
            = new Dictionary<string, List<MemberCandidate>>();

        public IReadOnlyList<TypeDefinition> Types { get; }
        
        public Operations(IEnumerable<TypeDefinition> typeDefs)
        {
            Types = typeDefs.ToList();
            foreach (var typeDefSymbol in Types)
                AddMembers(typeDefSymbol);
        }

        public IReadOnlyList<MemberCandidate> GetMembers(string name)
            => Lookup.ContainsKey(name) ? Lookup[name] : new List<MemberCandidate>();


        public void AddMember(TypeDefinition type, MemberDefinition member)
        {
            if (!Lookup.ContainsKey(member.Name))
                Lookup.Add(member.Name, new List<MemberCandidate>());
            Lookup[member.Name].Add(new MemberCandidate(type, member));
        }

        public void AddMembers(TypeDefinition type)
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