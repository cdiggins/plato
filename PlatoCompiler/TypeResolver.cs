using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Plato.Compiler
{
    /// <summary>
    /// How to test. GEt the list of functions.I want to iterate on the types. Figure out the
    /// types of the parameters, figure out the types of the variables, figure out the functions
    /// called figure out the return values.
    /// Get all possible functions. Filter based on the types of the arguments, and the number of the
    /// arguments.   
    /// </summary>
    public class TypeResolver   
    {
        public Dictionary<ParameterSymbol, List<Constraint>> ParameterConstraints { get; }
            = new Dictionary<ParameterSymbol, List<Constraint>>();

        public Operations Ops { get; }
        public IReadOnlyList<TypeDefSymbol> Types => Ops.Types;
        public IReadOnlyList<FunctionSymbol> Functions { get; }
        public IReadOnlyList<ParameterSymbol> Parameters { get; }

        public Dictionary<Symbol, TypeDefSymbol> ExpressionTypes { get; }
            = new Dictionary<Symbol, TypeDefSymbol>();

        public TypeDefSymbol GetType(Symbol s)
            => s == null 
                ? null 
                : ExpressionTypes.TryGetValue(s, out var r) 
                    ? r 
                    : null;

        public TypeResolver(Operations ops)
        {
            Ops = ops;

            // Get the functions declared as methods
            Functions = Types.SelectMany(t => t.GetDescendantSymbols().OfType<MethodDefSymbol>().Select(md => md.Function)).ToList();

            // Update the type associated with each function. 
            foreach (var f in Functions)
                AddType(f, f.Type?.Def);

            // Get the parameters 
            Parameters = Functions.SelectMany(f => f.Parameters).ToList();

            // Update the type associated with the parameter. 
            foreach (var p in Parameters)
                AddType(p, p.Type?.Def);

            // Update the type of all literals 
            foreach (var ls in Functions.SelectMany(f => f.GetDescendantSymbols()).OfType<LiteralSymbol>())
                AddType(ls, ComputeType(ls));
                

            // Update the type of the first parameter of extension libraries 
            foreach (var type in Types)
            {
                if (type.Kind != TypeKind.Library)
                    continue;

                var relatedType = Types.FirstOrDefault(t => t.Kind != TypeKind.Library && t.Name == type.Name);
                
                if (relatedType == null)
                    continue;

                Debug.Assert(relatedType != null);

                foreach (var m in type.Methods)
                {
                    if (m.Function.Parameters.Count == 0)
                        continue;

                    var p = m.Function.Parameters[0];

                    // If there was no type declared, we are using the related type
                    if (p.Type?.Def == null)
                        AddType(p, relatedType);
                }
            }

            // Compute and store all of the parameter constraints. 
            foreach (var f in Functions)
            {
                var lookup = Constraints.GetParameterConstraints(f);

                foreach (var kv in lookup)
                    ParameterConstraints.Add(kv.Key, kv.Value);
            }

            // Prepare for the type 
            foreach (var p in Parameters)
            {
                if (GetType(p) != null)
                    continue;
                var candidates = GetCandidateTypes(p).ToList();
                if (candidates.Count == 1)
                    AddType(p, candidates[0]);
            }

            ComputeExpressionTypes();
        }

        public void AddType(Symbol symbol, TypeDefSymbol type)
        {
            if (!ExpressionTypes.ContainsKey(symbol))
                ExpressionTypes.Add(symbol, type);
            else if (ExpressionTypes[symbol] == null)
                ExpressionTypes[symbol] = type;
        }

        public bool InheritsFrom(TypeDefSymbol self, TypeDefSymbol other)
        {
             
        }

        public bool Implements(TypeDefSymbol self, TypeDefSymbol other)
        {

        }

        public bool IsSuperType(TypeDefSymbol self, TypeDefSymbol other)
        {
            if (self.Equals(other))
                return true;
            if (InheritsFrom(self, other)) 
                return true;
            if (Implements(self, other))
                return true;
            return false;
        }

        public bool IsSuperType(TypeDefSymbol self, IEnumerable<TypeDefSymbol> others)
        {

        }

        public TypeDefSymbol Unify(IEnumerable<TypeDefSymbol> conceptsA, IEnumerable<TypeDefSymbol> conceptsB)
        {
            // Which concepts in A supercede all of those in B? 
            var superTypesA = conceptsA.Where(c => IsSuperType(conceptsB));

            // Which concepts in B supercede all of those in B
        }

        public TypeDefSymbol Unify(TypeDefSymbol a, TypeDefSymbol b)
        {
            // If one type inher
            
            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Equals(b))
                return a;

            // If one type is a concept, and the other is a regular type, then choose the type. 
            if (a.IsType() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsType())
                return a;

            if (a.IsType() && b.IsType())
            {
                var aConcepts = a.Implements.Select(i => i.Def);
                var bConcepts = b.Implements.Select(i => i.Def);
                return Unify(aConcepts, bConcepts);
            }

            if (a.IsConcept() && b.IsConcept())
            {
                var aConcepts = a.GetSelfAndAllInheritedTypes();
                var bConcepts = b.GetSelfAndAllInheritedTypes();
                return Unify(aConcepts, bConcepts);
            }
            return b;
        }

        public TypeDefSymbol Choose(FunctionGroupSymbol fs)
        {
            Debug.WriteLine($"TODO: function group choosing");
            return ComputeType(fs.Functions[0]);
        }


        TypeDefSymbol InternalComputeType(Symbol s)
        {
            switch (s)
            {
                case ArgumentSymbol argumentSymbol:
                    // NOTE: the fact that we are calling a function, creates a constraint that might determine the function  
                    return ComputeType(argumentSymbol.Original);

                case AssignmentSymbol assignmentSymbol:
                    throw new NotImplementedException("Not implemented yet?");

                case ConditionalExpressionSymbol conditionalExpressionSymbol:
                    // NOTE: the condition component is guaranteed to be a boolean.
                    // The fact that it must be a boolean, might affect what is called? 
                    return Unify(
                        ComputeType(conditionalExpressionSymbol.IfTrue),
                        ComputeType(conditionalExpressionSymbol.IfFalse));

                case FunctionGroupSymbol functionGroupSymbol:
                    return Choose(functionGroupSymbol);

                case FunctionCallSymbol functionCallSymbol:
                    return ComputeType(functionCallSymbol.Function);

                case LiteralSymbol literalSymbol:
                    switch (literalSymbol.Type)
                    {
                        case LiteralTypes.Int:
                            return PrimitiveTypes.Int;
                        case LiteralTypes.Float:
                            return PrimitiveTypes.Float;
                        case LiteralTypes.Bool:
                            return PrimitiveTypes.Bool;
                        case LiteralTypes.String:
                            return PrimitiveTypes.String;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case NoValueSymbol noValueSymbol:
                    throw new NotImplementedException("Not implemented yet?");

                case RefSymbol refSymbol:
                    return ComputeType(refSymbol.Def);

                case TypeRefSymbol typeRefSymbol:
                    return typeRefSymbol.Def;

                case FieldDefSymbol fieldDefSymbol:
                    throw new NotImplementedException("Should not be called");

                case FunctionSymbol functionSymbol:
                    return GetType(functionSymbol);

                case MethodDefSymbol methodDefSymbol:
                    return ComputeType(methodDefSymbol.Function);

                case ParameterSymbol parameterSymbol:
                    return GetType(parameterSymbol);

                case PredefinedSymbol predefinedSymbol:
                    // Intrinsic or Tuple
                    return predefinedSymbol.Type?.Def;

                case TypeParameterDefSymbol typeParameterDefSymbol:
                    throw new NotImplementedException("What are the predefined symbols?");

                case TypeDefSymbol typeDefSymbol:
                    return typeDefSymbol;

                case VariableSymbol variableSymbol:
                    throw new NotImplementedException("Not supported yet");

                default:
                    throw new ArgumentOutOfRangeException(nameof(s));
            }
        }

        public TypeDefSymbol ComputeType(Symbol s)
        {
            // TODO: the "Self" type is special. 

            if (s == null)
                return null;

            // Is the type already computed? Just return it then. 
            var r = GetType(s);
            if (r != null)
                return r;

            var tmp = InternalComputeType(s);
            AddType(s, tmp);
            return tmp;
        }


        public void ComputeExpressionTypes()
        {
            foreach (var f in Functions)
            {
                var t = ComputeType(f.Body);
                AddType(f, t);
            }
        }

        public List<Constraint> GetParameterConstraints(ParameterSymbol ps)
            => ParameterConstraints.TryGetValue(ps, out var r) ? r : new List<Constraint>();

        public IEnumerable<TypeDefSymbol> GetCandidateTypes(ParameterSymbol ps)
        {
            var r = GetType(ps);
            if (r != null) return new[] { r };

            // Get the constraints 
            var constraints = ParameterConstraints.TryGetValue(ps, out var v) ? v : Enumerable.Empty<Constraint>();

            // First look to see if the parameter is invoked. If so, then we are going to treat it as a function 
            if (constraints.Any(c => c is FunctionCallConstraint))
            {
                // TODO: create the correct function 
                return new[] { PrimitiveTypes.Function };
            }

            // Get type candidates 
            var candidates = new List<TypeDefSymbol>();
            foreach (var c in constraints)
                if (c is FunctionArgConstraint fac)
                    candidates.AddRange(Types.Where(t => Satisfies(t, fac)));

            if (candidates.Count == 0)
                candidates.Add(PrimitiveTypes.Any);

            candidates = candidates.Distinct().ToList();

            // TODO: find concepts and merge candidates where appropriate. 
            // Maybe I need a "t => Satisfies(x)" where x is all constraints ?
            // Right now, the problem is that a type only satisfies the predicate if IT satisfies. 
            // In some cases, the question is whether something it implements the predicate, or a function satisfies it, or something.
            // And this gets super vague, because the more we know about functions the better we can answer that questions, 
            // but the list gets really huge of all of the things we have to consider. 
            // This idea of "does type X satisfy some predicate, or set of predicates" needs to be able to be answered quickly. 
            // But we also know that the answer will change over time. 
            // Is it possible to just put all of the constraints together ... maybe answering them as a group would be faster. 

            return candidates;
        }

        public bool Satisfies(TypeDefSymbol type, FunctionArgConstraint fac) 
            => type.Methods.Any(m => m.Name == fac.Name);
    }
}