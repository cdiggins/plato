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
        public Compiler Compiler { get; }
        public FunctionGroupReference Fgr { get; }
        public IReadOnlyList<TypeExpressionSymbol> ArgTypes { get; }

        // Derived values 
        public TypeExpressionSymbol BestReturnType { get; }
        public bool Ambiguous => ReturnTypes.Count > 1;

        public IReadOnlyList<TypeExpressionSymbol> ReturnTypes { get; }
        public bool FoundResult => BestFunctions.Count > 0;

        // Intermediate values for the process 
        public IReadOnlyList<ReifiedFunction> AllFunctions { get; }
        public IReadOnlyList<ReifiedFunction> ValidFunctions { get; }
        public IReadOnlyList<ReifiedFunction> BestFunctions { get; }

        public string DebugString =>
            $"Success = {FoundResult && !Ambiguous}, Best type = {BestReturnType}, Function was {Fgr.Definition.DebugString}, Args were {string.Join(",", ArgTypes)}";

        public FunctionCallResolver(Compiler compiler, FunctionGroupReference fgr, IReadOnlyList<TypeExpressionSymbol> argTypes)
        {
            Compiler = compiler;
            Fgr = fgr;
            ArgTypes = argTypes;

            if (argTypes.Any(t => t == null))
                throw new Exception("Null argument type");

            AllFunctions = Compiler.ReifiedFunctionsByName[fgr.Name];
            
            // This should never happen
            if (AllFunctions.Count == 0)
                throw new Exception($"No functions associated with function group reference {fgr}");

            ValidFunctions = AllFunctions.Where(IsFunctionCallable).ToList();

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
            {
                Debug.WriteLine("Failed to resolved function call");
                //throw new Exception("Failed to resolve function call");
                BestReturnType = AllFunctions[0].ReturnType;
            }

            if (Ambiguous)
            {
                //throw new Exception("Found multiple return types");
                Debug.WriteLine("Found multiple return types");
            }
        }

        public int GetScoreForOverload(ReifiedFunction tf, int pos)
        {
            // Get the type of the first parameter 
            var pt = tf.ParameterTypes[pos];

            // Get the type of the second parameter 
            var at = ArgTypes[pos];

            // 0. Are they both the same concrete type and exactly the same?
            if (at.Definition.IsConcreteType() && at.Equals(pt))
                return 0;
            
            // 1. Are we passing a concrete type to another concrete type and it has an implicit "cast"?
            if (at.Definition.IsConcreteType() && pt.Definition.IsConcreteType() && at.Definition.CanCastTo(pt.Definition))
                return 1;

            // 2. Are we casting from a concrete type to one of the concepts it implemented
            if (at.Definition.IsConcreteType() && pt.Definition.IsConcept() && at.Definition.Implements(pt.Definition))
                return 2;

            // 3. Are they both the same concept
            if (at.Definition.IsConcept() && pt.Equals(at)) 
                return 3;

            // 4. Are we casting from a concept to one of the concepts it inherits 
            if (at.Definition.IsConcept() && pt.Definition.IsConcept() && pt.Definition.InheritsFrom(at.Definition))
                return 4;

            // 5. Are we casting from a type variable to a non-type-variable
            //if (at.IsTypeVar() && !pt.IsTypeVar())
                return 5;

            // 6. everything else
            //return 6;
        }

        public bool IsFunctionCallable(ReifiedFunction ft)
        {
            if (ft.ParameterTypes.Count != ArgTypes.Count)
                return false;

            for (var i = 0; i < ft.ParameterTypes.Count; ++i)
            {
                var paramType = ft.ParameterTypes[i];
                var argType = ArgTypes[i];
                if (!argType.Definition.CanCastTo(paramType.Definition))
                    return false;
            }

            return true;
        }
    }
}