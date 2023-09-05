using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// Technically this is used for resolving calls to function groups.
    /// When a function can be one or more, this decides 
    /// </summary>
    public class FunctionCallResolver
    {
        // Passed as arguments
        public TypeFactory Factory { get; }
        public FunctionGroupReference Fgr { get; }
        public IReadOnlyList<Type> ArgTypes { get; }

        // Derived values 
        public bool IsMapped { get; }
        public Type BestReturnType { get; }
        public bool Ambiguous => ReturnTypes.Count > 1;

        public IReadOnlyList<Type> ReturnTypes { get; }
        public bool FoundResult => BestFunctions.Count > 0;

        // Intermediate values for the process 
        public IReadOnlyList<TypedFunction> AllFunctions { get; }
        public IReadOnlyList<TypedFunction> ValidFunctions { get; }
        public IReadOnlyList<TypedFunction> BestFunctions { get; }

        public string DebugString =>
            $"Success = {FoundResult && !Ambiguous}, Mapped = {IsMapped}, Best type = {BestReturnType}, Function was {Fgr.Definition.DebugString}, Args were {string.Join(",", ArgTypes)}";

        public FunctionCallResolver(TypeFactory factory, FunctionGroupReference fgr, IReadOnlyList<Type> argTypes)
        {
            Factory = factory;
            Fgr = fgr;
            ArgTypes = argTypes;

            if (argTypes.Any(t => t == null))
                throw new Exception("Null argument type");

            AllFunctions = fgr.Definition.Functions.Select(Factory.GetTypedFunction).ToList();
            
            // This should never happen
            if (AllFunctions.Count == 0)
                throw new Exception($"No functions associated with function group reference {fgr}");

            ValidFunctions = AllFunctions.Where(IsFunctionCallable).ToList();

            // If there are no valid functions, then we might require a mapped function . 
            if (ValidFunctions.Count == 0)
            {
                ValidFunctions = AllFunctions.Where(IsFunctionMappable).ToList();
                if (ValidFunctions.Count > 0)
                {
                    IsMapped = true;
                }
            }

            BestFunctions = ValidFunctions;

            if (BestFunctions.Count > 1)
            {
                // Reduce the best functions list.
                
                // starting at the first argument try to find the best overload. 
                var pos = 0;

                while (pos < ArgTypes.Count)
                {
                    // Group possibilities according to score. 
                    var groups = BestFunctions.GroupBy(tf => GetScoreForOverload(tf, 0)).OrderBy(g => g.Key);
                    var tmp = groups.First().ToList();

                    // Groups are supposed to non empty according to definition
                    if (tmp.Count == 0)
                        throw new Exception("Should be impossible!");

                    // This is our new Best functions list 
                    BestFunctions = tmp;
                    
                    // Only one best function so we can leave early. 
                    if (BestFunctions.Count == 1)
                        break;

                    // There are multiple best functions, so we are going to look at the next argument/parameter
                    pos++;
                }
            }

            ReturnTypes = BestFunctions.Select(tf => tf.ReturnType).Distinct().ToList();
            BestReturnType = ReturnTypes.FirstOrDefault();

            if (BestReturnType == null)
                throw new Exception("Failed to resolve function call");

            if (Ambiguous)
                throw new Exception("Found multiple return types");

            if (IsMapped)
                BestReturnType = Factory.CreateArray(BestReturnType);
        }

        public int GetScoreForOverload(TypedFunction tf, int pos)
        {
            // Get the type of the first parameter 
            var pt = tf.Parameters[pos];

            // Get the type of the second parameter 
            var at = IsMapped ? RemoveArrayType(ArgTypes[pos]) : ArgTypes[pos];

            // 0. Are they both the same concrete type and exactly the same?
            if (at.IsConcreteType() && at.Equals(pt))
                return 0;
            
            // 1. Are we passing a concrete type to another concrete type and it has an implicit "cast"?
            if (at.IsConcreteType() && pt.IsConcreteType() && at.CanCastTo(pt))
                return 1;

            // 2. Are we casting from a concrete type to one of the concepts it implemented
            if (at.IsConcreteType() && pt.IsConcept() && at.Implements(pt))
                return 2;

            // 3. Are they both the same concept
            if (at.IsConcept() && pt.Equals(at)) 
                return 3;

            // 4. Are we casting from a concept to one of the concepts it inherits 
            if (at.IsConcept() && pt.IsConcept() && pt.InheritsFrom(at))
                return 4;

            // 5. Are we casting from a type variable to a non-type-variable
            if (at.IsTypeVar() && !pt.IsTypeVar())
                return 5;

            // 6. everything else
            return 6;
        }

        public bool IsFunctionCallable(TypedFunction ft)
        {
            if (ft.Parameters.Count != ArgTypes.Count)
                return false;

            for (var i = 0; i < ft.Parameters.Count; ++i)
            {
                var paramType = ft.Parameters[i];
                var argType = ArgTypes[i];
                if (!argType.CanCastTo(paramType))
                    return false;
            }

            return true;
        }

        public bool IsFunctionMappable(TypedFunction ft)
        {
            if (ft.Parameters.Count != ArgTypes.Count)
                return false;

            for (var i = 0; i < ft.Parameters.Count; ++i)
            {
                var paramType = ft.Parameters[i];
                var argType = RemoveArrayType(ArgTypes[i]);
                
                if (!argType.CanCastTo(paramType))
                    return false;
            }

            return true;
        }

        public static Type RemoveArrayType(Type t)
        {
            // If t is an array, get the parameter. 
            if (t is TypeReference tr)
            {
                if (tr.Definition.Name == "Array")
                {
                    if (tr.TypeArguments.Count == 1)
                        return tr.TypeArguments[0];
                    if (tr.TypeArguments.Count > 1)
                        throw new Exception("Too many type arguments for an array!");

                    // NOTE: if we get here, it suggest that the type factory really should have created a single type variable for us 
                    throw new Exception("Arrays should have exactly one type argument");
                }
            }

            return t;
        }

    }
}