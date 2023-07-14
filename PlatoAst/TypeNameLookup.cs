using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class MemberNames
    {
        public MemberNames(AstTypeDeclaration declaration, IEnumerable<MemberNames> inherited)
        {
            Declaration = declaration;
            InheritedTypes = inherited.ToDictionary(mn => mn.Name, mn => mn);
            foreach (var m in Declaration.Members)
                Members.Add(m.Name, m);
            foreach (var tp in Declaration.TypeParameters)
                Members.Add(tp.Name, tp);
        }

        public string Name => Declaration.Name;
        public AstTypeDeclaration Declaration { get; }
        public Dictionary<string, MemberNames> InheritedTypes { get; }
        public Dictionary<string, AstNode> Members { get; } = new Dictionary<string, AstNode>();
    }

    public class TypeNames
    {
        public Dictionary<string, MemberNames> Dictionary { get; } 
            = new Dictionary<string, MemberNames>();

        public IReadOnlyList<AstTypeDeclaration> TypeDeclarations { get; }

        public TypeNames(IEnumerable<AstTypeDeclaration> types)
        {
            TypeDeclarations = types.ToList();
            foreach (var t in TypeDeclarations)
            {
                AddOrGetMemberNames(t);
            }
        }

        public MemberNames AddOrGetMemberNames(string name)
        {
            return AddOrGetMemberNames(TypeDeclarations.FirstOrDefault(t => t.GetName() == name));
        }

        public MemberNames AddOrGetMemberNames(AstTypeDeclaration type)
        {
            if (Dictionary.ContainsKey(type.GetName()))
                return Dictionary[type.GetName()];
            var r = new MemberNames(type,
                type.BaseTypes.Select(bt => AddOrGetMemberNames(bt.Name)));
            Dictionary.Add(type.GetName(), r);
            return r;
        }
    }

    public static class Extensions
    {
        public static string GetName(this AstTypeDeclaration type)
        {
            return type.Name;
        }
    }
}