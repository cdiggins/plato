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
        public TypeExpressionSymbol ResultType { get; }

        public FunctionCallResolver(Compiler compiler,
            FunctionGroupReference fgr,
            IReadOnlyList<TypeExpressionSymbol> argTypes)
        {
            Compiler = compiler;
            Fgr = fgr;
            ArgTypes = argTypes;

            if (ArgTypes.Any(t => t == null))
                throw new Exception("Null argument type");

            // First we are going to look for a viable result from the function group 
            var functions = fgr.Definition.Functions;
            if (functions.Count == 0)
                throw new Exception($"No functions found in group {Fgr}");
            functions = functions.Where(f => f.Parameters.Count == ArgTypes.Count).ToList();
            if (functions.Count == 0)
                throw new Exception($"No functions in group found with correct number of arguments {ArgTypes.Count}");
            var returnTypes = functions.Select(f => f.ReturnType).Distinct().ToList();
            if (returnTypes.Count == 1)
            { 
                ResultType = returnTypes[0];
                return;
            }

            // There was no result so far

            // TODO: I really don't think I should look at reified functions if the input is a Concept. 

            // Now look at the reified functions 
            var allFunctions = Compiler.ReifiedFunctionsByName[Fgr.Name];
            
            // This should never happen
            if (allFunctions.Count == 0)
                throw new Exception($"No functions associated with function group reference {Fgr}");

            allFunctions = allFunctions.Where(f => f.ParameterTypes.Count == ArgTypes.Count).ToList();
            if (allFunctions.Count == 0)
                throw new Exception($"No available functions found matching number of arguments {ArgTypes.Count}");

            var validFunctions = allFunctions.Where(IsFunctionCallable).ToList();
            if (validFunctions.Count == 0)
                throw new Exception($"Could find not callable functions");

            var bestFunctions = validFunctions;
            if (bestFunctions.Count > 1)
            {
                // Reduce the best functions list.
                
                // starting at the first argument try to find the best overload. 
                var pos = 0;
                while (pos < ArgTypes.Count)
                {
                    // Group possibilities according to score. 
                    var groups = bestFunctions.GroupBy(tf => GetScoreForOverload(tf, 0)).OrderBy(g => g.Key);
                    var tmp = groups.First().ToList();

                    // Groups are supposed to non empty according to definition
                    if (tmp.Count == 0)
                        throw new Exception("Should be impossible!");

                    // This is our new Best functions list 
                    bestFunctions = tmp;
                    
                    // Only one best function so we can leave early. 
                    if (bestFunctions.Count == 1)
                        break;

                    // There are multiple best functions, so we are going to look at the next argument/parameter
                    pos++;
                }
            }

            if (bestFunctions.Count == 0)
                throw new Exception("Should always have at least one Best function");

            returnTypes = bestFunctions.Select(tf => tf.ReturnType).Distinct().ToList();

            if (returnTypes.Count > 1)
                throw new Exception("Found too many matching types");

            ResultType = returnTypes.FirstOrDefault();

            /* TODO: possible fall-back ... use the original function definitions?

            var groupFuncs = Fgr.Definition.Functions;
            if (groupFuncs.Count == 1)
            {
                ResultType = groupFuncs.F;
            }
            */
        }

        public int GetScoreForOverload(ReifiedFunction tf, int pos)
            => CastingScore(ArgTypes[pos], tf.ParameterTypes[pos]);

        public bool IsCastFunction(ReifiedFunction rf, TypeDefinitionSymbol td)
            => rf.Name == $"To{td.Name}" && rf.ParameterTypes.Count == 0 && rf.ReturnType.Definition.Equals(td);

        public ReifiedFunction GetCastFunction(TypeDefinitionSymbol a, TypeDefinitionSymbol b)
        {
            if (Compiler.ReifiedTypes.ContainsKey(a.Name))
            {
                var rt = Compiler.ReifiedTypes[a.Name];
                return rt.Functions.FirstOrDefault(f => IsCastFunction(f, b));
            }

            // TODO: what about non-reified types? e.g. from concept to concept? 

            return null;
        }

        public bool CastFunctionExists(TypeDefinitionSymbol a, TypeDefinitionSymbol b)
            => GetCastFunction(a, b) != null;

        public bool CanCast(TypeExpressionSymbol at, TypeExpressionSymbol pt)
            => CastingScore(at, pt) > 0;
        
        public int CastingScore(TypeExpressionSymbol at, TypeExpressionSymbol pt)
        {
            if (at == null)
                throw new ArgumentNullException($"Argument type");
            
            if (pt == null)
                throw new ArgumentNullException($"Parameter type");
            
            if (at.Definition.IsLibrary())
                throw new ArgumentException($"Argument type {at} cannot be library");

            if (pt.Definition.IsLibrary())
                throw new Exception($"Parameter types {pt} cannot be libraries");

            // Are we passing to a concrete type? 
            // Always prefer to successfully pass to a concrete type 
            if (pt.Definition.IsConcreteType())
            {
                if (at.Definition.IsConcreteType())
                {
                    if (at.Definition.Equals(pt.Definition))
                        return 1;

                    if (CastFunctionExists(at.Definition, pt.Definition))
                        return 2;

                    return 0;
                }

                if (at.Definition.IsConcept())
                {
                    if (CastFunctionExists(at.Definition, pt.Definition))
                        return 3;

                    // Definitely a conflict. The passed type is not implemented by the desired concept. 
                    return 0;
                }

                if (at.Definition.IsTypeVariable())
                {
                    // This might be compatible. It would create a constraint. 
                    // TODO: maybe I could check against that constraints declared for the type variable? 
                    return 20;
                }

                throw new Exception($"Expected argument to be concrete type, concept, or type variable");
            }

            if (pt.Definition.IsConcept())
            {
                if (at.Definition.IsConcreteType())
                {
                    // The passed concrete type implements the expected concept
                    if (at.Definition.Implements(pt.Definition))
                        return 3;

                    return 0;
                }

                if (at.Definition.IsConcept())
                {
                    // Same concept? 
                    if (at.Definition.Equals(pt.Definition))
                        return 4;

                    // Passing a sub-type to a super-type
                    if (at.Definition.Implements(pt.Definition))
                        return 5;

                    // Casting from concept a to concept b. 
                    if (CastFunctionExists(at.Definition, pt.Definition))
                        return 6;
                    
                    // Unrecognized concept 
                    return 0;
                }
            }

            if (pt.Definition.IsTypeVariable())
            {
                // TODO: create a constraint 
                return 30; 
            }

            throw new Exception($"Unsupported parameter type {pt}");
        }

        public bool IsFunctionCallable(ReifiedFunction ft)
        {
            if (ft.ParameterTypes.Count != ArgTypes.Count)
                return false;

            for (var i = 0; i < ft.ParameterTypes.Count; ++i)
            {
                var paramType = ft.ParameterTypes[i];
                var argType = ArgTypes[i];
                if (!CanCast(argType, paramType))
                    return false;
            }

            return true;
        }
    }
}