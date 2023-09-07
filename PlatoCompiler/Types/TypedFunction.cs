using System;
using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler.Types
{
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