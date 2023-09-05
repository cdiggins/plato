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

        public HashSet<TypeVariable> TypeVariables { get; } = new HashSet<TypeVariable>();

        public TypedFunction(FunctionDefinition definition, TypeReference functionType)
        {
            Debug.Assert(functionType.Definition.Equals(PrimitiveTypeDefinitions.Function));
            var nArgs = functionType.TypeArguments.Count;
            Debug.Assert(nArgs >= 1);
            Definition = definition;
            FunctionType = functionType;
            ReturnType = functionType.TypeArguments[nArgs - 1];
            Parameters = functionType.TypeArguments.Take(nArgs - 1).ToList();
            
            // NOTE: all the type variables appearing in return type, must first appear on the left.
            CollectTypeVariables(Parameters);
            ValidateReturnTypeVariables(ReturnType);
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

        private void CollectTypeVariables(IEnumerable<Type> types)
        {
            foreach (var t in types)
            { 
                if (t is TypeVariable tv)
                    TypeVariables.Add(tv);
                if (t is TypeReference tr)
                    CollectTypeVariables(tr.TypeArguments);
            }
        }

        private void ValidateReturnTypeVariables(Type t)
        {
            if (t is TypeVariable tv)
                if (!TypeVariables.Contains(tv))
                    throw new Exception($"Type variable {tv} occurs for the first time in return type.");
            if (t is TypeReference tr)
                foreach (var ta in tr.TypeArguments)
                    ValidateReturnTypeVariables(ta);
        }

        public void GatherSubstitutions(Type argType, Type paramType, Dictionary<TypeVariable, List<Type>> subs)
        {
            if (paramType is TypeVariable tv)
            {
                // TEMP: catching things early. 
                if (subs.ContainsKey(tv))
                    throw new Exception("Type unification not supported yet");

                if (!subs.ContainsKey(tv))
                    subs.Add(tv, new List<Type>());

                subs[tv].Add(argType);
            }

            if (paramType is TypeReference trDest)
            {
                if (argType is TypeReference trSrc)
                {
                    if (trDest.TypeArguments.Count != trSrc.TypeArguments.Count)
                    {
                        throw new Exception(
                            $"Number of type argument in source {trSrc.TypeArguments.Count} does not match destination {trDest.TypeArguments.Count}");
                    }

                    for (var i = 0; i < trSrc.TypeArguments.Count; ++i)
                    {
                        GatherSubstitutions(trSrc.TypeArguments[i], trDest.TypeArguments[i], subs);
                    }
                }
            }
        }

        public Type ApplySubstitutions(TypeFactory factory, Type src, Dictionary<TypeVariable, List<Type>> subs)
        {
            if (src is TypeVariable tv)
            {
                if (!subs.ContainsKey(tv))
                    return tv;
                var list = subs[tv];
                if (list.Count > 1)
                    throw new Exception("Type unification not supported yet");
                return list[0];
            }
             
            if (src.IsConcreteType())
                return src;

            if (src.IsConcept())
                return src;
            
            if (src is TypeReference tr)
            {
                var args = tr.TypeArguments.Select(arg => ApplySubstitutions(factory, arg, subs)).ToList();
                return new TypeReference(src.Definition, args, factory);
            }

            throw new Exception("Unexpected control flow within apply substitutions algorithm");
        }

        public Type GetReturnType(TypeFactory factory, IReadOnlyList<Type> argTypes)
        {
            if (argTypes.Count != Parameters.Count)
                throw new Exception(
                    $"Number of arguments {argTypes.Count} does not match number of parameters {Parameters.Count}");

            var substitutions = new Dictionary<TypeVariable, List<Type>>();
            for (var i = 0; i < argTypes.Count; ++i)
            {
                GatherSubstitutions(argTypes[i], Parameters[i], substitutions);
            }

            return ApplySubstitutions(factory, ReturnType, substitutions);
        }
    }
}