using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace Plato.Compiler
{
    public class Type
    {
        public string Name { get; }
        public Symbol Declaration { get; }
        public int Id = Symbol.NextId++; 
        
        public Type(string name, Symbol declaration)
        {
            Name = name;
            Declaration = declaration;
        }

        public override string ToString()
        {
            return $"{Name}_{Id}";
        }
    }

    public class PrimitiveType : Type
    {
        public PrimitiveType(string name, TypeDefSymbol symbol)
            : base(name, symbol)
        { }
    }

    public class AnyType : Type
    {
        public AnyType()
            : base("Any", null)
        { }
    }

    public class SelfType : Type
    {
        public SelfType()
            : base("Self", null)
        { }
    }

    public class TypeReference : Type
    {
        public IReadOnlyList<Type> TypeArguments { get; }
        public TypeReference(TypeRefSymbol symbol)
            : base(symbol.Name, symbol)
        {
            var nArgs = symbol.TypeArgs.Count;
            var nParams = symbol.Def.TypeParameters.Count;
            if (nArgs > nParams)
                throw new Exception($"Passed too many type arguments {nArgs} expected {nParams}");
            var list = new List<Type>();
            for (var i = 0; i < nParams; ++i)
            {
                var p = symbol.Def.TypeParameters[i];
                var arg = i < nArgs ? new TypeReference(symbol.TypeArgs[i]) : null;
                if (arg != null)
                    list.Add(arg);
                else if (p.Constraint != null)
                    list.Add(new TypeReference(p.Constraint));
                else
                    list.Add(new AnyType());
            }

            TypeArguments = list;
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeArguments);
            return $"{base.ToString()}<{tps}>";
        }
    }

    public class TypedSymbol
    {
        public Type Type { get; }
        public Symbol Symbol { get; }

        public TypedSymbol(Type type, Symbol symbol)
        {
            Type = type;
            Symbol = symbol;
        }
    }
    
    public class TypeConstraint
    {
        public Type Type { get; }

        protected TypeConstraint(Type type)
            => Type = type;
    }

    /*
    public class TypeConstraintSet : TypeConstraint
    {
        public List<TypeConstraint> Options { get; } = new List<TypeConstraint>();
    }

    // This is like a derived constraint. It is created whenever the number of 
    // possible functions is changed.  
    public class OneOfConstraint : TypeConstraint
    {
        public List<AbstractType> Options { get; } = new List<AbstractType>();
    }

    public class FunctionGroup
    {
        public IReadOnlyList<Function> Options { get; }
    }

    public class InvokedFunction
    {
        public FunctionSymbol Symbol { get; }
        public AbstractType ReturnType { get; }
        public IReadOnlyList<TypedParameter> Parameter { get;}

        public Function(FunctionSymbol symbol)
        {
            // TODO: create the function and the children. 
        }

        public List<TypeConstraint> Constraints { get; }
    }

    public class ArgumentConstraint : TypeConstraint
    {
        public int Position { get; set; }
        public int ArgumentCount { get; set; }
        public FunctionGroup FunctionGroup { get; set; }
    }

    public class KnownTypeConstraint : TypeConstraint
    {
    }

    public class FunctionContext
    {
        public FunctionSymbol Function { get; set; }
        public string Name => Function.Name;
        public AbstractType ArgumentTypes { get; set; }
    }

    public class CanCastConstraint : TypeConstraint
    {
        // For example: the return value of a function, or the condition of a conditional expression
        public AbstractType TargetType { get; set; }
    }

    public class ImplementsConstraint : TypeConstraint
    {
        public TypeDefSymbol Concept { get; }
    }

    public class SelfConstraint : TypeConstraint
    {
        // Here and above, I think maybe the 
        public TypeDefSymbol Concept { get; }
    }

    public class TypeParameterConstraint : TypeConstraint
    {
        public GenericParameter Parameter { get; }
    }

    public class UnifiesConstraint : TypeConstraint
    {
        // Both sides of a condition expression, or when a "self" or type parameter are used multiple times. 
        public AbstractType OtherType { get; }
    }

    public class IsCalledConstraint : TypeConstraint
    {
        public List<AbstractType> ArgumentTypes { get; }
    }

    public class TypeParameterCountConstraint : TypeConstraint
    {
        public int Count { get; }
    }

    public class IsTypeParameter : TypeConstraint
    {
        public TypeDefSymbol Concept { get; }
    }

    public class ResolutionContext
    {
    }

    public class TypeArgument
    {
        public string Name { get; set; }
        public List<TypeConstraint> Constraints { get; } = new List<TypeConstraint>();
    }

    public class GenericFunction
    {
        public List<TypeArgument> TypeArguments { get; set; }
        public string Name => Function.Name;
        public FunctionSymbol Function { get; set; }
    }

    public class TypeInstance
    {
        public TypeDefSymbol ReferredDef; 

        public TypeInstance(TypeDefSymbol symbol)
        { }
    }

    public class TypeOptions
    {
    }

    public class TypeResolutionContext
    {
    }

    public class TypeResolutionResult 
    { }

    public class FunctionArgType { }

    public class FunctionCallType { }
    
    public class FunctionCallResultType { }
    
    public class FunctionConstraints { }

    public class GenericParameter 
    { }

    public class GenericTypedFunction
    {
        public string Name => FunctionDef.Name;
        public TypeDefSymbol ContainingTypeDef { get;  }
        public FunctionSymbol FunctionDef { get; }
        public List<TypedParameter> Parameters { get; } = new List<TypedParameter>();
        public AbstractType ReturnType { get; }

        public List<GenericParameter> TypeTypeParameters { get; } = new List<GenericParameter>();
        public List<GenericParameter> FunctionTypeParameters { get; } = new List<GenericParameter>();
        
        public bool IsGeneric => FunctionTypeParameters.Count > 0;

        public void Validate()
        {

            if (ContainingTypeDef.IsType())
            {
                throw new Exception("Only concepts and libraries can have functions");
            }

            if (ContainingTypeDef.IsConcept())
            {
                throw new Exception("Concept functions should not be generic");
            }
        }

        public GenericTypedFunction(TypeDefSymbol containingType, FunctionSymbol function)
        {
            ContainingTypeDef = containingType;
            
            FunctionDef = function;
            foreach (var p in function.Parameters)
            {
                // var tp = new TypedParameter
            }
        }
    }

    public class TypedFunction
    {
        public GenericTypedFunction Definition { get; }
        public List<TypedParameter> Parameters { get; }
        public Dictionary<GenericParameter, Type> TypeArg { get; }
    }

    public class TypeArgument
    {
        public TypeArgument(GenericParameter parameter, )
    }

    public class TypedParameter
    {
        public string Name => Symbol.Name;
        public Type Type { get; }
        public ParameterSymbol Symbol { get; }

        public TypedParameter(Type type, ParameterSymbol symbol)
            => (Type, Symbol) = (type, symbol);
    }

    
    public class GenericTypeReference
    {
        public string Name { get; set; }
    }

    public class ConceptDeclaration { }

    public class TypeDeclaration { }

    // Note: a concept is either parameterized, or it is a type. 

    public class ClassReference : Type
    {

    }
    */

    // A function signature generates types (and constraints)
    // It is like a function. 
    public class FunctionSignature
    {
        public IReadOnlyList<Type> Parameters { get; }
        public Type ReturnType { get; }
        public string Name { get; }

        public FunctionSignature(FunctionSymbol f)
        {
            Name = f.Name;
            Parameters = f.Parameters.Select(p => new TypeReference(p.Type)).ToList();
            ReturnType = new TypeReference(f.Type);
        }

        public override string ToString()
        {
            var parameters = string.Join(", ", Parameters);
            return $"{Name}({parameters}): {ReturnType}";
        }
    }

    public class Function
    {
        public FunctionSignature Signature { get; }
        public Function(FunctionSymbol f)
            => Signature = new FunctionSignature(f);

        public override string ToString()
        {
            return Signature.ToString();
        }
    }

    public class TypeParameter : Type
    {
        public int Index { get; }

        public TypeParameter(int index, TypeParameterDefSymbol symbol)
            : base(symbol.Name, symbol)
        {
            Index = index;
        }

        public override string ToString()
        {
            return $"{Name}{Index}";
        }
    }

    // If I reference a concept, then the types are going to be different. 
    // Well technically no ... the constraints are going to be different. 
    // I can still work out many things bout the concept. 
    // 
    // Basically the goal is to get the function signatures correct for the functions in the concept. 
    // They need to become proper type generators.
    public class Concept
    {
        public IReadOnlyList<TypeReference> InheritedTypes { get; }
        public IReadOnlyList<Function> Functions { get; }
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public SelfType Self { get; } = new SelfType();
        public TypeDefSymbol Symbol { get; }
        public string Name => Symbol.Name;

        public Concept(TypeDefSymbol symbol)
        {
            if (!symbol.IsConcept())
                throw new Exception("Expected a concept");
            Symbol = symbol;
            TypeParameters = symbol.TypeParameters.Select((tp, i) => new TypeParameter(i, tp)).ToList();
            InheritedTypes = symbol.Inherits.Select(i => new TypeReference(i)).ToList();
            Functions = symbol.Functions.Select(f => new Function(f)).ToList();
        }
    }

    public class AbstractTypeResolver
    {
        public IReadOnlyList<Concept> Concepts { get; }
        
        public AbstractTypeResolver(IReadOnlyList<TypeDefSymbol> types)
        {
            Concepts = types.Where(t => t.IsConcept()).Select(t => new Concept(t)).ToList();

            // Steps.
            // 1) Create all of the generic type references
            // 1a) first from the concepts.
            // 1b) then from the libraries. 

            // 2) Type check the concepts. 
            // Can it be created etc. 
            // Any function groups that are called, we have to substitute types for them.
            // Then find out which function groups are valid. Then figure out what their return types are. 

            // Once a function is chosen, it may introduce new constraints on the types that are passed in. 
            // However, those constraints only exist "if" we call that function. 
            // So there is a range of possibilities. Did we call function A, or did we call function B? 
            // This potentially can grow. 

            // We may need to dynamically reduce the list of possibilities. Once we reject something, just toss it out. 
        }

        /*
        public bool IsGenericFunction(FunctionSymbol f) => throw new NotImplementedException();
        public GenericFunction GetGenericFunction(FunctionSymbol f) => throw new NotImplementedException();
        public bool IsWellTyped(GenericFunction f) => throw new NotImplementedException();
        public List<TypeConstraint> GetConstraints(ParameterSymbol p) => throw new NotImplementedException();
        public List<TypeConstraint> GetResultConstraints(FunctionSymbol f) => throw new NotImplementedException();
        public int GetNumDistinctTypeParameters(GenericFunction f) => throw new NotImplementedException();
        public List<TypeDefSymbol> GetTypesThatSatisfy(TypeConstraint tc) => throw new NotImplementedException();
        public List<TypeDefSymbol> GetTypesThatSatisfy(IEnumerable<TypeConstraint> tc) => throw new NotImplementedException();
        public bool Satisfies(TypeDefSymbol td, TypeConstraint tc) => throw new NotImplementedException();
        public bool CanCall(GenericFunction f, List<TypedSymbol> args) => throw new NotImplementedException();
        */
        
    }
}