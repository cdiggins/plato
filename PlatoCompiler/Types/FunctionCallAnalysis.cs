using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
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

    public class FunctionArgAnalysis
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
                TypeReplacements.Add(new TypeReplacement(to.Def, from));
            if (from.TypeArgs.Count != to.TypeArgs.Count)
                throw new Exception(
                    $"Mismatched number of type arguments: this cast should not be possible from {from} to {to}");
            for (var i=0; i < from.TypeArgs.Count; ++i)
                GatherTypeReplacements(from.TypeArgs[i], to.TypeArgs[i]);
        }
    }

    public class TypeReplacement
    {
        public readonly TypeDef Variable;
        public readonly TypeExpression Type;

        public TypeReplacement(TypeDef def, TypeExpression type)
            => (Variable, Type) = (def, type);
    }

    public class FunctionCallAnalysis
    {
        public FunctionAnalysis Function { get; }
        public List<FunctionArgAnalysis> Args { get; } = new List<FunctionArgAnalysis>();
        public FunctionDef FunctionDef => Function.Def;
        public Compilation Compilation => Function.Compilation;
        public TypeExpression DeclaredReturnType => FunctionDef.ReturnType;
        public TypeExpression DeterminedReturnType { get; }
        public bool HasBody => Function.Def.Body != null;
        public bool Valid { get; }
        public bool ArityMatches { get; }
        public Dictionary<string, TypeDef> TypeVariables { get; } = new Dictionary<string, TypeDef>();
        public Dictionary<string, List<TypeExpression>> TypeReplacements = new Dictionary<string, List<TypeExpression>>();

        public FunctionCallAnalysis(FunctionAnalysis function, IReadOnlyList<TypeExpression> args)
        {
            ArityMatches = function.Def.Parameters.Count == args.Count;
                
            if (!ArityMatches) return;

            for (var i = 0; i < args.Count; i++)
            {
                var argType = args[i];
                var parameter = function.Def.Parameters[i];
                var cast = Compilation.CanCast(argType, parameter.Type);
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
    }
}