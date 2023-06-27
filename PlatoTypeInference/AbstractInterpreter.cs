using System;
using System.Collections.Generic;
using Plato;

namespace PlatoAbstractSyntax
{
    public interface IContext
    {
        IContext Parent { get; }
        IContext PushNewContext();
        IMultiDictionary<string, IValue> Bindings { get; }
        System.Collections.Generic.IDictionary<IExpression, IValue> Results { get; }
        IValue Retrieve(string name);
        IContext Bind(string name, IValue value);
        IContext BindUniquely(string name, IValue value);
        IContext Rebind(string name, IValue value);
        IContext BindOrRebing(string name, IValue value);
    }

    public interface IExpression 
    {
        // TODo:
        //IArray<IExpression> Children { get; }
    }

    public class IdentifierExpression : IExpression
    {
        public string Name { get; }
        public IType Type { get; }

        public IdentifierExpression(string name, IType type)
            => (Name, Type) = (name, type);
        
        public IValue Evaluate(IContext context)
            => context.Retrieve(Name);
    }

    public class AbstractExpression : IExpression
    {
        public IArray<IExpression> Children { get; }
        public Func<IArray<IValue>, IContext, IValue> Func { get; }

        public int Count { get; }
        public IExpression this[int input] => throw new System.NotImplementedException();
        public IIterator<IExpression> Iterator { get; }
        
        public IValue Evaluate(IContext context)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class BinaryBooleanOperation : IExpression
    {
        public int Count => 2;
        public Func<bool, bool, bool> Func { get; }
        public IExpression Left;
        public IExpression Right;

        public IValue Evaluate(IContext context)
            => throw new NotImplementedException();

    }

    public class FunctionInvocation : IExpression
    {
        public IType Type { get; }
        public IReadOnlyList<IExpression> Parameters { get; }

        public IValue Evaluate(IContext context)
        {
            /*
            var parameters = Parameters.Select(p => p.Evaluate(context)).ToList();
            var function = parameters[0] as IFunctionValue;
            if (function == null) throw new Exception("First parameter must be a function");
            return function.Invoke(parameters.Skip(1).ToList());
            */
            throw new NotImplementedException();
        }
    }

    /*
     * // Function call
     * // Numeric operators (binary, unary)
            // Boolean operators (binary, unary)
            // Overloaded operators
            // Literals (string, number, boolean, null)
            // Bitwise operators 
            // new operators 
            // Methods
            // typeof
            // nameof
            // Indexers 
            // Abstract function calls
            // Property accessors
            // Closures 
            // Conditionals 
            // Null coalescing operator 
            // Loops: ?? (while loop, etc. can all be expressed in a function form, should I?) 
            // Compound assignment
            // throw 
            // using? 
            // Try-catch-finally? 
            // Tuple
            // string interpolation
            // indexing 

     */


    public interface IValue : IExpression
    {
        IType Type { get; }
    }

    public interface IFunctionValue : IValue
    {
        IValue Invoke(IReadOnlyList<IValue> parameters);
    }
    
    public interface IValueArray : IValue, IArray<IValue>
    {   
    }

    public interface IType 
    {
    }

    public class Literal : IValue
    {
        public object Value { get; }
        public Literal(object value)
            => (Value, Type) = (value, value.GetType().ToIType());
        public IValue Evaluate(IContext context)
            => this;
        public IType Type { get; }
    }

    public class FixedType: IType
    {
        public Type Type { get; }

        public FixedType(Type type)
            => Type = type;
    }
    
    public class FixedType<T> : IType
    {
        public Type Type => typeof(T);
    }

    public interface ITypeArray : IType, IArray<IType>
    {           
    }

    //public class TypeArray : ITypeArray {  }

    public interface IVariableDeclaration
    { }

    public interface IVariableBinding
    {
    }

    public static class Extensions
    {
        public static IType ToIType(this Type type)
            => new FixedType(type);
    }
}