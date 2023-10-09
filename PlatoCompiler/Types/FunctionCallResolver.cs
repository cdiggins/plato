using System;
using System.Collections.Generic;
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
        public IReadOnlyList<TypeExpression> ArgTypes { get; }
        public TypeExpression ResultType { get; set; }

        public FunctionCallResolver(Compiler compiler,
            FunctionGroupReference fgr,
            IReadOnlyList<TypeExpression> argTypes)
        {
            Compiler = compiler;
            Fgr = fgr;
            ArgTypes = argTypes;

            if (ArgTypes.Any(t => t == null))
                throw new Exception("Null argument type");

            // First we are going to look for a viable result from the function group 
            var funcs = fgr.Definition.Functions;
            if (funcs.Count == 0)
                throw new Exception($"No functions found in group {Fgr}");
            if (HasSingleReturnType(funcs)) return;

            var funcs2 = GetCallableFunctions(funcs);
            if (HasSingleReturnType(funcs2)) return; 

            var funcs3 = GetBestFunctions(funcs2);
            if (HasSingleReturnType(funcs3)) return;

            // TODO: I don't think I should look at reified functions if the input is a Concept. 

            // Now look at the reified functions 
            var funcs4 = Compiler.ReifiedFunctionsByName[Fgr.Name];
            
            // This should never happen
            if (funcs4.Count == 0)
                throw new Exception($"No functions associated with function group reference {Fgr}");

            var funcs5 = GetCallableFunctions(funcs4);
            if (HasSingleReturnType(funcs5)) return;

            var funcs6 = GetBestFunctions(funcs5);
            if (HasSingleReturnType(funcs6)) return;

            throw new Exception("Could not find distinct return type");
        }

        public bool HasSingleReturnType(IEnumerable<IFunction> functions)
        {
            var results = functions.Select(f => f.ReturnType).Distinct().ToList();
            if (results.Count != 1)
                return false;
            ResultType = results[0];
            return true;
        }

        public List<IFunction> GetCallableFunctions(IEnumerable<IFunction> functions)
        {
            return functions
                .Where(IsFunctionCallable)
                .ToList();
        }

        public List<IFunction> GetBestFunctions(IEnumerable<IFunction> functions)
        {
            var r = GetCallableFunctions(functions);

            if (r.Count > 1)
            {
                // Reduce the best functions list.

                // starting at the first argument try to find the best overload. 
                var pos = 0;
                while (pos < ArgTypes.Count)
                {
                    // Group possibilities according to score. 
                    var tmp = r
                        .GroupBy(tf => GetScoreForOverload(tf, pos))
                        .OrderBy(g => g.Key)
                        .First()
                        .ToList();

                    // Groups are supposed to non empty according to definition
                    if (tmp.Count == 0)
                        throw new Exception("Should be impossible groups should never be empty");

                    // This is our new Best functions list 
                    r = tmp;

                    // Only one best function so we can leave early. 
                    if (r.Count == 1)
                        return r;

                    // There are multiple best functions, so we are going to look at the next argument/parameter
                    pos++;
                }
            }            

            return r;
        }

        public int GetScoreForOverload(IFunction f, int pos)
            => CastingScore(ArgTypes[pos], f.GetParameterType(pos));

        public bool IsCastFunction(IFunction f, TypeDefinition td)
            => f.Name == $"To{td.Name}" && f.NumParameters == 0 && f.ReturnType.Definition.Equals(td);

        public ReifiedFunction GetCastFunction(TypeDefinition a, TypeDefinition b)
        {
            if (Compiler.ReifiedTypes.ContainsKey(a.Name))
            {
                var rt = Compiler.ReifiedTypes[a.Name];
                return rt.Functions.FirstOrDefault(f => IsCastFunction(f, b));
            }

            // TODO: what about non-reified types? e.g. from concept to concept? 

            return null;
        }

        public bool CastFunctionExists(TypeDefinition a, TypeDefinition b)
            => GetCastFunction(a, b) != null;

        public bool CanCast(TypeExpression at, TypeExpression pt)
            => CastingScore(at, pt) > 0;
        
        public int CastingScore(TypeExpression at, TypeExpression pt)
        {
            if (at == null)
                throw new ArgumentNullException($"Argument type");
            
            if (pt == null)
                throw new ArgumentNullException($"Parameter type");
            
            if (at.Definition.IsLibrary())
                throw new ArgumentException($"Argument type {at} cannot be library");

            if (pt.Definition.IsLibrary())
                throw new Exception($"Parameter types {pt} cannot be libraries");

            // If they are the same type, done and done. 
            if (at.Equals(pt))
                return 1;

            // Are we passing to a concrete type? 
            // Always prefer to successfully pass to a concrete type 
            if (pt.Definition.IsConcrete())
            {
                if (at.Definition.IsConcrete())
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

                if (at.Definition.IsPrimitive())
                    return 0;

                throw new Exception($"Expected argument to be concrete type, concept, primitive, or type variable");
            }

            if (pt.Definition.IsConcept())
            {
                if (at.Definition.IsConcrete())
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

            // When expecting primitives, only a primitive will 
            if (pt.Definition.IsPrimitive())
            {
                // They aren't exact types.
                if (at.Definition.Equals(pt.Definition))
                    return 50;

                return 0;
            }

            throw new Exception($"Unsupported parameter type {pt}");
        }

        public bool IsFunctionValid(IFunction f) 
            => f.NumParameters == ArgTypes.Count;

        public bool IsFunctionCallable(IFunction f)
        {
            if (!IsFunctionValid(f))
                return false;

            for (var i = 0; i < f.NumParameters; ++i)
            {
                var paramType = f.GetParameterType(i);
                var argType = ArgTypes[i];
                if (!CanCast(argType, paramType))
                    return false;
            }

            return true;
        }
    }
}