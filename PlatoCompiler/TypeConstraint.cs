namespace Plato.Compiler
{
    public class TypeConstraint
    {
        public Type Type { get; }

        protected TypeConstraint(Type type)
            => Type = type;
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
}