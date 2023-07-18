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
        public IEnumerable<TypeDefSymbol> Types { get; }
        public IReadOnlyList<FunctionSymbol> Functions { get; }
        public IReadOnlyList<ParameterSymbol> Parameters { get; } 

        public TypeGuesser(Operations ops, IEnumerable<TypeDefSymbol> typeDefs, IEnumerable<FunctionSymbol> functions)
        {
            Ops = ops;
            Types = typeDefs.ToList();
            Functions = functions.ToList();
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

            var candidates = new List<TypeDefSymbol>();
            foreach (var c in constraints)
            {
                if (c is FunctionArgConstraint fac)
                {
                    candidates.AddRange(Types.Where(t => Satisfies(t, fac)));
                }
            }

            if (candidates.Count == 0)
                candidates.Add(Primitives.Any); 

            return candidates;
        }

        public bool Satisfies(TypeDefSymbol type, FunctionArgConstraint fac)
        {
            return type.Methods.Any(m => m.Name == fac.Name);
        }

        // A list of methods for which the types are not known. 
        // Any method that references a name which is in an untyped method is not known  
        public Dictionary<string, List<MethodDefSymbol>> UntypedMethods = new Dictionary<string, List<MethodDefSymbol>>();

        // TODO: what methods are known types? 
        // TODO: constraint to method. 
        // TODO: what methods are available for each name. 
     }
}