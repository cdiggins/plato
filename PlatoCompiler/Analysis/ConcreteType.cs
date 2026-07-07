using System.Collections.Generic;
using System.Linq;
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

            var implementedSigs = new HashSet<string>(ImplementedFunctions.Select(f => f.SignatureId));
            UnimplementedFunctions = DeclaredFunctions.Where(f => !implementedSigs.Contains(f.SignatureId)).ToList();

            InterfaceFunctionGroups = ConcreteFunctions
                .Concat(GetInterfaceFunctions())
                .GroupBy(f => f.SignatureId)
                .Select(g => g.ToList()).ToList();
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