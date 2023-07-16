using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public class TypeGuesser
    {
        public Dictionary<ParameterSymbol, List<TypeDefSymbol>> CandidateTypes
            = new Dictionary<ParameterSymbol, List<TypeDefSymbol>>();

        public Dictionary<ParameterSymbol, List<Constraint>> ParameterConstraints { get; }
            = new Dictionary<ParameterSymbol, List<Constraint>>();

        public List<ParameterSymbol> DeclaredTypes = new List<ParameterSymbol>();

        public Operations Ops { get; }
        public IReadOnlyList<TypeDefSymbol> Types { get; }
        public IReadOnlyList<FunctionSymbol> Functions { get; }
        public IReadOnlyList<ParameterSymbol> Parameters { get; } 

        public TypeGuesser(IEnumerable<TypeDefSymbol> types)
        {
            Types = types.ToList();
            Ops = new Operations(Types);
            Functions = Types.GetAllFunctions().ToList();
            Parameters = Functions.SelectMany(f => f.Parameters).ToList();

            foreach (var f in Functions)
            {
                var localConstraints = Constraints.GetParameterConstraints(f);
                foreach (var kv in localConstraints)
                {
                    ParameterConstraints.Add(kv.Key, kv.Value);
                }
            }

            foreach (var p in Parameters)
            {
                if (p.Type != null)
                {
                    DeclaredTypes.Add(p);
                    continue;
                }

                CandidateTypes.Add(p, GetCandidateTypes(p).ToList()); 
            }
        }

        public IEnumerable<TypeDefSymbol> GetCandidateTypes(ParameterSymbol ps)
        {
            if (!ParameterConstraints.TryGetValue(ps, out var constraints))
                return Enumerable.Empty<TypeDefSymbol>();

            // NOW: the question is, I have a list of constraints. Which types might be appropriate? 

            // THe first attempt might be to look at all of the 

            // First look to see if the parameter is invoked. If so, then we are going to treat it as a function 
            if (constraints.Any(c => c is FunctionCallConstraint))
            {
                // TODO: create the correct function 
                return new[] { Primitives.Function };
            }

            var candidates = Types;
            foreach (var c in constraints)
            {
                if (c is MemberAccessConstraint mac)
                {
                    candidates = candidates.Where(t => Satisfies(t, mac)).ToList();
                }
            }

            return candidates;
        }

        public bool Satisfies(TypeDefSymbol type, MemberAccessConstraint mac)
        {
            var typeOps = Ops.Lookup[type.Name];
            foreach (var m in typeOps.Members)
            {
                if (m is FieldDefSymbol fds)
                {
                    if (fds.Name == mac.MemberAccess.Name)
                        return true;
                }
            }

            return false;
        }
    }
}