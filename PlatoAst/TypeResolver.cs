using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PlatoAst
{

    /// <summary>
    /// How to test. GEt the list of functions.I want to iterate on the types. Figure out the
    /// types of the parameters, figure out the types of the variables, figure out the functions
    /// called figure out the return values.
    /// Get all possible functions. FIlter based on the types of the arguments, and the number of the
    /// arguments.   
    /// </summary>
    public class TypeResolver
    {
        public Dictionary<ParameterSymbol, List<TypeDefSymbol>> CandidateTypes
            = new Dictionary<ParameterSymbol, List<TypeDefSymbol>>();

        public Dictionary<ParameterSymbol, List<Constraint>> ParameterConstraints { get; }
            = new Dictionary<ParameterSymbol, List<Constraint>>();

        public List<ParameterSymbol> DeclaredTypes = new List<ParameterSymbol>();

        public Operations Ops { get; }
        public IReadOnlyList<TypeDefSymbol> Types => Ops.Types;
        public IReadOnlyList<FunctionSymbol> Functions { get; }
        public IReadOnlyList<ParameterSymbol> Parameters { get; } 

        public TypeResolver(Operations ops)
        {
            Ops = ops;
            Functions = Types.SelectMany(t => t.GetDescendantSymbols().OfType<FunctionSymbol>()).ToList();
            Parameters = Functions.SelectMany(f => f.Parameters).ToList();
            foreach (var p in Parameters)
            {
                var constraints = new List<Constraint>();
                
                if (p.Type != null)
                    constraints.Add(new DeclaredConstraint(p.Type));

                ParameterConstraints.Add(p, constraints);
            }

            foreach (var f in Functions)
            {
                var localConstraints = Constraints.GetParameterConstraints(f);
                foreach (var kv in localConstraints)
                {
                    ParameterConstraints[kv.Key].AddRange(kv.Value);
                }
            }

            foreach (var type in Types)
            {
                if (type.Kind != "module")
                    continue;

                var relatedType = Types.FirstOrDefault(t => t.Kind != "module" && t.Name == type.Name);
                
                if (relatedType == null)
                    continue;

                Debug.Assert(relatedType != null);

                foreach (var m in type.Methods)
                {
                    if (m.Function.Parameters.Count == 0)
                        continue;

                    var p = m.Function.Parameters[0];
                    
                    if (p.Type != null)
                    {
                        var constraint = new DeclaredConstraint(relatedType.ToRef);
                        ParameterConstraints[p].Add(constraint);
                    }
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

            // The first attempt might be to look at all of the 

            // First look to see if the parameter is invoked. If so, then we are going to treat it as a function 
            if (constraints.Any(c => c is FunctionCallConstraint))
            {
                // TODO: create the correct function 
                return new[] { PrimitiveTypes.Function };
            }

            var decls = constraints.OfType<DeclaredConstraint>().ToList();
            if (decls.Count > 1)
            {
                throw new Exception("Can't have more than one declared constraint");
            }

            if (decls.Count == 1)
            {
                return new [] { decls[0].Type.Def };
            }

            var candidates = new List<TypeDefSymbol>();
            foreach (var c in constraints)
            {
                if (c is FunctionArgConstraint fac)
                {
                    candidates.AddRange(Types.Where(t => Satisfies(t, fac)));
                }
            }

            // TODO: if one of the candidates is a concept then remove any types implementing the concept 
            // TODO: remove duplicates 

            if (candidates.Count == 0)
                candidates.Add(PrimitiveTypes.Any);

            candidates = candidates.Distinct().ToList();

            return candidates;
        }

        public bool Satisfies(TypeDefSymbol type, FunctionArgConstraint fac)
        {
            return type.Methods.Any(m => m.Name == fac.Name);
        }


        // TODO: this should be moved the type resolver. 
        public string GetBestCandidate(FunctionArgConstraint fac)
        {
            var name = fac.Name;
            var members = Ops.GetMembers(name);

            // NOTE: this is not accurate. It has to do with the actual type based on
            // the position. 
            var position = fac.Position;

            var tmp = members.Where(pair => pair.Type.Kind == "concept").ToList();
            if (tmp.Count == 0)
                tmp = members.ToList();

            if (tmp.Count > 0)
            {
                var t = tmp[0].Type;
                var m = tmp[0].Member;
                if (m is FieldDefSymbol fds)
                {
                    return fds.Type?.Name ?? "dynamic";
                }

                if (m is MethodDefSymbol mds)
                {
                    if (fac.ArgumentCount > mds.Function.Parameters.Count)
                    {
                        return "outofrange";
                    }

                    var param = mds.Function.Parameters[position];

                    // TODO: right now dynamic is just code for must be inferred later. 
                    return param.Type?.Name ?? "dynamic";
                }
            }

            return "Any";
        }

        // A list of methods for which the types are not known. 
        // Any method that references a name which is in an untyped method is not known  
        public Dictionary<string, List<MethodDefSymbol>> UntypedMethods = new Dictionary<string, List<MethodDefSymbol>>();

        // TODO: what methods are known types? 
        // TODO: constraint to method. 
        // TODO: what methods are available for each name. 
     }
}