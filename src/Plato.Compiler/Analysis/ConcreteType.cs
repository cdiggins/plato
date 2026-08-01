using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Analysis
{
    /// <summary>
    /// Holds a list of all the functions associated with the type, and the implemented concepts.
    /// </summary>
    public class ConcreteType
    {
        public string Name => TypeDef.Name;
        public TypeDef TypeDef { get; }
        public IReadOnlyList<TypeParameterDef> Parameters { get; }
        public LibraryFunctions Libraries { get; }
        public TypeSubstitutions Substitutions { get; }
        public IReadOnlyList<InterfaceImplementation> Interfaces { get; }
        public IReadOnlyList<InterfaceImplementation> AllInterfaces { get; }
        public IReadOnlyList<FunctionInstance> ConcreteFunctions { get; }
        public IReadOnlyList<FunctionInstance> FieldFunctions { get; }
        public IReadOnlyList<FunctionInstance> DeclaredFunctions { get; }
        public IReadOnlyList<FunctionInstance> ImplementedFunctions { get; }
        public IReadOnlyList<FunctionInstance> UnimplementedFunctions { get; }
        public IReadOnlyList<List<FunctionInstance>> InterfaceFunctionGroups { get; }

        public ConcreteType(TypeDef typeDef, LibraryFunctions libraries)
        {
            Verifier.AssertNotNull(typeDef, nameof(typeDef));
            Verifier.AssertNotNull(libraries, nameof(libraries));
            Verifier.Assert(typeDef.IsConcrete());
            TypeDef = typeDef;
            Libraries = libraries;
            Substitutions = new TypeSubstitutions("Self", TypeDef.ToTypeExpression());
            Interfaces = typeDef.Implements.Select(CreateConceptImplementation).ToList();
            AllInterfaces = Interfaces.AllDescendants().Distinct(i => i.ToString()).OrderBy(i => i.ToString()).ToList();
            ConcreteFunctions = libraries.GetFunctionsForType(TypeDef.ToTypeExpression())
                .Select(f => CreateFunctionInstance(f, FunctionInstanceKind.ConcreteType)).ToList();
            FieldFunctions = TypeDef.Fields
                .Select(f => CreateFunctionInstance(f.Function, FunctionInstanceKind.FieldType)).ToList();

            ImplementedFunctions = AllInterfaces.SelectMany(c => c.ImplementedFunctions).Concat(ConcreteFunctions)
                .Concat(FieldFunctions).ToList();
            DeclaredFunctions = AllInterfaces.SelectMany(c => c.DeclaredFunctions).Distinct(d => d.SignatureId)
                .ToList();

            _ownTypeParameterNames = TypeDef.TypeParameters.Select(tp => tp.Name).ToHashSet();
            _implementationsBySignature = FirstBySignature(ImplementedFunctions, f => f.SignatureId);
            _implementationsByCanonicalSignature = _ownTypeParameterNames.Count == 0
                ? new Dictionary<string, FunctionInstance>()
                : FirstBySignature(ImplementedFunctions, CanonicalCandidateSignature);

            UnimplementedFunctions = DeclaredFunctions.Where(f => ImplementationFor(f) == null).ToList();

            InterfaceFunctionGroups = ConcreteFunctions
                .Concat(GetInterfaceFunctions())
                .GroupBy(f => f.SignatureId)
                .Select(g => g.ToList()).ToList();
        }

        // -------------------------------------------------------------------
        // Pairing an obligation with the implementation that discharges it.
        //
        // The primary key is the rendered SignatureId: the substituted signatures
        // must match textually. For a GENERIC concrete type that key can never
        // fire. The obligation is substituted with the type's OWN type parameter
        // (ColumnCount(Array2D<T>):Integer), while the only way to WRITE the
        // implementation is over a type variable (ColumnCount(xs: Array2D<$T>)),
        // and 'T' is not the string the analysis gives that variable — so every
        // obligation of a generic type reported as unimplemented and the writer
        // emitted a throwing member (plato-376).
        //
        // The fallback is a one-way positional unification. Both signatures are
        // re-rendered with their type variables (and, on the obligation side, the
        // declaring type's own parameters) renamed to #0, #1, ... in order of first
        // appearance. First-occurrence renaming makes the variable NAME irrelevant
        // while preserving its REPETITION pattern, so Sample(Tween<$A>, Number):$B
        // does not discharge Sample(Tween<T>, Number):T. It is one-way because only
        // a candidate's VARIABLE may stand in for the obligation's parameter: a
        // candidate naming a concrete type there (ColumnCount(Array2D<Integer>))
        // renders that name and matches nothing generic. Every other position must
        // still agree exactly.
        //
        // Duplicate detection (LINT004) deliberately does NOT use this: two library
        // functions differing only in a variable name are still two declarations.
        // -------------------------------------------------------------------

        private readonly HashSet<string> _ownTypeParameterNames;
        private readonly IReadOnlyDictionary<string, FunctionInstance> _implementationsBySignature;
        private readonly IReadOnlyDictionary<string, FunctionInstance> _implementationsByCanonicalSignature;

        /// <summary>
        /// The implementation that discharges <paramref name="obligation"/>, or null when the
        /// obligation is unimplemented. This is the single pairing every consumer must use — the
        /// linter and the writers have to agree on which members are going to throw.
        /// </summary>
        public FunctionInstance ImplementationFor(FunctionInstance obligation)
            => _implementationsBySignature.TryGetValue(obligation.SignatureId, out var exact)
                ? exact
                : MentionsOwnTypeParameter(obligation)
                  && _implementationsByCanonicalSignature.TryGetValue(
                      CanonicalObligationSignature(obligation), out var unified)
                    ? unified
                    : null;

        private static Dictionary<string, FunctionInstance> FirstBySignature(
            IEnumerable<FunctionInstance> functions, Func<FunctionInstance, string> key)
            => functions.GroupBy(key).ToDictionary(g => g.Key, g => g.First());

        /// <summary>True when the obligation is keyed by a type parameter of the declaring type,
        /// which is the only shape the unification fallback exists for.</summary>
        private bool MentionsOwnTypeParameter(FunctionInstance f)
            => f.ParameterTypes.Append(f.ReturnType)
                .SelectMany(t => t.SelfAndDescendants())
                .Any(t => t.Def.Kind == TypeKind.TypeParameter && _ownTypeParameterNames.Contains(t.Name));

        /// <summary>The obligation's signature with its type parameters and variables canonically renamed.</summary>
        private static string CanonicalObligationSignature(FunctionInstance f)
            => CanonicalSignature(f, t => t.Def.Kind == TypeKind.TypeParameter || t.Def.IsTypeVariable());

        /// <summary>The candidate's signature with its type VARIABLES canonically renamed; a concrete
        /// type keeps its name, which is what makes the unification one-way.</summary>
        private static string CanonicalCandidateSignature(FunctionInstance f)
            => CanonicalSignature(f, t => t.Def.IsTypeVariable());

        private static string CanonicalSignature(FunctionInstance f, Func<TypeInstance, bool> renameable)
        {
            var canonical = new Dictionary<string, string>();

            string Render(TypeInstance t)
            {
                if (renameable(t))
                {
                    if (!canonical.TryGetValue(t.Name, out var name))
                        canonical.Add(t.Name, name = $"#{canonical.Count}");
                    return name;
                }
                return t.Args.Count == 0
                    ? t.Name
                    : $"{t.Name}<{t.Args.Select(Render).JoinStringsWithComma()}>";
            }

            // Parameters first, so "first appearance" is source order.
            var parameters = f.ParameterTypes.Select(Render).ToList().JoinStringsWithComma();
            return $"{f.Name}({parameters}):{Render(f.ReturnType)}";
        }

        public InterfaceImplementation CreateConceptImplementation(TypeExpression type)
            => new InterfaceImplementation(Libraries, this, Substitutions.Add(type), type);

        public FunctionInstance CreateFunctionInstance(FunctionDef function, FunctionInstanceKind kind)
            => new FunctionInstance(function, TypeDef, null, kind, Substitutions);

        public IEnumerable<FunctionInstance> GetInterfaceFunctions()
            => Interfaces.SelectMany(c => c.AllFunctions().Where(FunctionMatches)).ToList();

        public bool FunctionMatches(FunctionInstance f)
        {
            // NOTE: we assume that the first function matches. 
            //var tmp = ConceptImplementation
            var t = f.ParameterTypes[0];

            if (t.Def == TypeDef || t.Def.Equals(TypeDef))
                return true;
            
            if (!t.Def.IsInterface())
                throw new System.Exception("Expected an interface type in first position");

            if (TypeDef.Implements(t.Expr))
                return true;

            return false;
        }

        public IReadOnlyList<TypeExpression> DistinctFieldTypes
            => TypeDef.Fields.Select(f => f.Type).Distinct().ToList();

        public override string ToString()
            => $"Concrete:{TypeDef}";
    }
}