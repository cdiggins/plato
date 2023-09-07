using System;
using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler.Types
{
    public class TypedFunctionVariation
    {
        public TypedFunction Original { get; }
        public TypedFunction Current { get; }

        public static IEnumerable<TypedFunctionVariation> CreateVariations(TypedFunction original, IReadOnlyList<Type> types)
        {
            Dictionary<Type, Type> lookup;

            // TODO:
            // All functions that take a number as the first argument are extended to support all types that implement Numerical.
            // In that function all instances of "number" are replaced with the type. This gives us "addition", "subtraction", etc.
            // Where this fails is that it doesn't give us ScalarArithmetic. 

            // All functions on a type T are extended to accept all types that implement Array<T> 

            // This isn't true "mapping", there are two types of mapping:
            // Square(x: Number) => mapping Square(x: Array<Number>) => mapped types Square(x: Vector), Square(x: Interval), Square(x ...)
            
            // but here is the thing. Why don't I just write Square(x: Numerical) => x * x; 
            // Then the Square function works on all Numerical types! 

            // 


            throw new NotImplementedException();
        }
    }

    public class TypedFunction
    {
        public IReadOnlyList<Type> Parameters { get; }
        public Type ReturnType { get; }
        public Type FunctionType { get; }
        public FunctionDefinition Definition { get; }
        public string Name => Definition.Name;

        public TypedFunction(FunctionDefinition definition, TypeReference functionType)
        {
            var nArgs = functionType.TypeArguments.Count;
            Debug.Assert(nArgs >= 1);
            Definition = definition;
            FunctionType = functionType;
            ReturnType = functionType.TypeArguments[nArgs - 1];
            Parameters = functionType.TypeArguments.Take(nArgs - 1).ToList();

            // TEMP: this might not be appropriate now. 
            // NOTE: all the type variables appearing in return type, must first appear on the left.
            //CollectTypeVariables(Parameters);
            //ValidateReturnTypeVariables(ReturnType);
        }

        public override string ToString()
            => $"{Name}({string.Join(", ", Parameters)}) -> {ReturnType}";

        public override bool Equals(object obj)
            => obj != null
               && obj is TypedFunction other
               && GetType() == other.GetType()
               && Name == other.Name
               && Equals(ReturnType, other.ReturnType)
               && Parameters.SequenceEqual(other.Parameters);

        public override int GetHashCode()
            => Hasher.Hash(Parameters.Cast<object>().Append(Name).Append(ReturnType).ToArray());
    }
}