using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    /*
     *        
       public FunctionDef FindImplicitCast(IType from, IType to)
       {
           var name = to.GetTypeDefinition()?.Name;
           if (string.IsNullOrEmpty(name)) return null;
           var funcs = FunctionDefinitions.Where(fd => fd.FunctionType == FunctionType.Cast).Select(GetProcessedFunctionAnalysis).ToList();
           funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
           funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
           if (funcs.Count == 0) return null;
           if (funcs.Count > 1) throw new Exception("Ambiguous cast functions");
           return funcs[0].Def;
       }

       public FunctionDef FindCastConstructor(IType from, IType to)
       {
           var name = to.GetTypeDefinition()?.Name;
           if (string.IsNullOrEmpty(name)) return null;
           var funcs = FunctionDefinitions.Where(fd => fd.FunctionType == FunctionType.Constructor).Select(GetProcessedFunctionAnalysis).ToList();
           funcs = funcs.Where(fd => fd?.DeclaredReturnType?.Equals(to) == true).ToList();
           funcs = funcs.Where(fd => fd.ParameterTypes.Count == 1 && fd.ParameterTypes[0].Equals(from)).ToList();
           if (funcs.Count == 0) return null;
           if (funcs.Count > 1) throw new Exception("Ambiguous constructor functions");
           return funcs[0].Def;
       }
     */

    public class CastDetails
    {
        public readonly TypeExpression From;
        public readonly TypeExpression To;
        public readonly int Depth;
        public readonly FunctionDef CastFunction;

        public CastDetails(TypeExpression from, TypeExpression to, int depth = 0, FunctionDef cast = null)
        {
            From = from;
            To = to;
            Depth = depth;
            CastFunction = cast;
        }

        public override string ToString()
            => $"Cast<{From},{To},{Depth},{CastFunction}>";
    }

    public class FunctionArgAnalysis : IComparable<FunctionArgAnalysis>
    {
        public int Index { get; }
        public string Name => Parameter.Name;
        public ParameterDef Parameter { get; }
        public TypeExpression ArgType { get; }
        public TypeExpression ParameterType => Parameter.Type;
        public CastDetails Cast { get; }
        public bool Valid => Cast != null;
        public int Depth => Cast?.Depth ?? -1;
        public List<TypeReplacement> TypeReplacements { get; } = new List<TypeReplacement>();

        public FunctionArgAnalysis(int index, ParameterDef parameter, TypeExpression argType, CastDetails cast)
        {
            Index = index;
            Parameter = parameter;
            ArgType = argType;
            Cast = cast;
            if (Cast != null)
                GatherTypeReplacements(Cast.From, Cast.To);
        }

        public void GatherTypeReplacements(TypeExpression from, TypeExpression to)
        {
            if (to.IsTypeVariable)
            {
                TypeReplacements.Add(new TypeReplacement(to.Def, from));
            }
            else
            {
                if (from.TypeArgs.Count != to.TypeArgs.Count)
                    throw new Exception(
                        $"Mismatched number of type arguments: this cast should not be possible from {from} to {to}");
                for (var i = 0; i < from.TypeArgs.Count; ++i)
                    GatherTypeReplacements(from.TypeArgs[i], to.TypeArgs[i]);
            }
        }

        public int CompareTo(FunctionArgAnalysis arg2)
        {
            if (!ArgType.Equals(arg2.ArgType))
                throw new Exception("Expected both arguments to be the same");
            if (ParameterType.Equals(arg2.ParameterType))
                return 0;
            
            if (ParameterType.Def.IsConcrete())
            {
                if (!arg2.ParameterType.Def.IsConcrete())
                    return -1;

                // Both types are concrete. If one has the correct na
                if (ParameterType.Name.Equals(ArgType.Name))
                {
                    if (!arg2.ParameterType.Name.Equals(ArgType.Name))
                        return -1;
                    return 0;
                }
                else
                {
                    if (arg2.ParameterType.Name.Equals(ArgType.Name))
                        return +1;
                    return 0;
                }

                // Both types are concrete, but neither have the correct name.
                return 0;
            }

            if (arg2.ParameterType.Def.IsConcrete())
                return +1;

            if (ParameterType.Def.IsConcept())
            {
                if (!ParameterType.Def.IsConcept())
                    return +1;

                if (Depth < arg2.Depth)
                    return -1;
                if (Depth > arg2.Depth)
                    return +1;

                return 0;
            }

            if (arg2.ParameterType.Def.IsConcept())
                return +1;

            Debug.Assert(ParameterType.Def.IsTypeVariable());
            Debug.Assert(arg2.ParameterType.Def.IsTypeVariable());
            
            return 0;
        }
    }

    public class TypeReplacement
    {
        public readonly TypeDef Variable;
        public readonly TypeExpression Type;

        public TypeReplacement(TypeDef def, TypeExpression type)
            => (Variable, Type) = (def, type);
    }

    public class FunctionCallAnalysis : IComparable<FunctionCallAnalysis>
    {
        public List<FunctionArgAnalysis> Args { get; } = new List<FunctionArgAnalysis>();
        public FunctionDef FunctionDef { get; }
        public TypeExpression DeclaredReturnType => FunctionDef.ReturnType;
        public TypeExpression DeterminedReturnType { get; }
        public bool HasBody => FunctionDef.Body != null;
        public bool Valid { get; }
        public bool ArityMatches { get; }
        public Dictionary<string, TypeDef> TypeVariables { get; } = new Dictionary<string, TypeDef>();
        public Dictionary<string, List<TypeExpression>> TypeReplacements = new Dictionary<string, List<TypeExpression>>();

        public FunctionCallAnalysis(Compilation compilation, FunctionDef def, IReadOnlyList<TypeExpression> args)
        {
            FunctionDef = def;
            ArityMatches = FunctionDef.Parameters.Count == args.Count;
                
            if (!ArityMatches) return;

            for (var i = 0; i < args.Count; i++)
            {
                var argType = args[i];
                var parameter = FunctionDef.Parameters[i];
                var cast = compilation.CanCast(argType, parameter.Type);
                var arg = new FunctionArgAnalysis(i, parameter, argType, cast);
                foreach (var tr in arg.TypeReplacements)
                {
                    var name = tr.Variable.Name;
                    if (!TypeReplacements.ContainsKey(name))
                        TypeReplacements.Add(name, new List<TypeExpression>());
                    TypeReplacements[name].Add(tr.Type);    
                }
                GatherTypeVariables(arg.ArgType);
                Args.Add(arg);
            }
            Valid = Args.All(a => a.Valid);
            DeterminedReturnType = ApplyTypeReplacements(DeclaredReturnType);
        }

        public void GatherTypeVariables(TypeExpression expr)
        {
            if (expr.Def.IsTypeVariable())
                TypeVariables.Add(expr.Name, expr.Def);
            foreach (var te in expr.TypeArgs)
                GatherTypeVariables(te);
        }

        public TypeExpression ApplyTypeReplacements(TypeExpression expr)
        {
            if (expr.IsTypeVariable)
            {
                var name = expr.Name;
                if (TypeReplacements.TryGetValue(name, out var options))
                {
                    if (options.Count > 1)
                    {
                        Debug.WriteLine($"Ambiguity of type replacements for {name} = {options.JoinStringsWithComma()}");
                    }
                    return options[0];
                }

                return expr;
            }
            if (expr.TypeArgs.Count == 0)
                return expr;
            var def = expr.Def;
            var args = expr.TypeArgs.Select(ApplyTypeReplacements).ToArray();
            return new TypeExpression(def, args);
        }

        public int CompareTo(FunctionCallAnalysis other)
        {
            if (Args.Count != other.Args.Count)
                throw new Exception("Expected both functions to have the same number of arguments");

            for (var i = 0; i < Args.Count; i++)
            {
                var a1 = Args[i];
                var a2 = other.Args[i];
                var tmp = a1.CompareTo(a2);
                if (tmp < 0)
                    return -1;
                if (tmp > 0)
                    return +1;
            }

            return 0;
        }
    }
}