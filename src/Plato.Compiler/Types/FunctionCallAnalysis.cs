using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.Compiler.Types
{
    public class FunctionCallAnalysis : IComparable<FunctionCallAnalysis>
    {
        public Compilation Compilation { get; }
        public List<FunctionArgAnalysis> Args { get; } = new List<FunctionArgAnalysis>();
        public FunctionDef FunctionDef { get; }
        public TypeExpression DeclaredReturnType { get; }
        public TypeExpression DeterminedReturnType { get; }
        public bool HasBody => FunctionDef.Body != null;
        public bool Valid { get; }
        public bool ArityMatches { get; }
        public Dictionary<string, TypeDef> TypeVariables { get; } = new Dictionary<string, TypeDef>();
        public Dictionary<string, List<TypeExpression>> TypeReplacements = new Dictionary<string, List<TypeExpression>>();

        public FunctionCallAnalysis(Compilation compilation, FunctionDef def, IReadOnlyList<TypeExpression> args)
        {
            Compilation = compilation;
            FunctionDef = def;
            ArityMatches = FunctionDef.Parameters.Count == args.Count;
            DeclaredReturnType = FunctionDef.ReturnType;
            DeterminedReturnType = DeclaredReturnType;

            if (!ArityMatches) return;

            for (var i = 0; i < args.Count; i++)
            {
                var argType = args[i];
                var parameter = FunctionDef.Parameters[i];
                var parameterType = parameter.Type;

                var determinedParameterType = parameterType;

                if (parameterType.Def.IsInterface())
                {
                    var tv = new TypeVariable(def.Scope, $"_T{i}");
                    determinedParameterType = tv.ToTypeExpression();

                    if (DeterminedReturnType.Equals(parameterType))
                    {
                        DeterminedReturnType = argType;
                    }
                }
                
                var arg = new FunctionArgAnalysis(this, i, parameter, determinedParameterType, argType);
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

            DeterminedReturnType = ApplyTypeReplacements(DeterminedReturnType);
        }

        public void GatherTypeVariables(TypeExpression expr)
        {
            if (expr.Def.IsTypeVariable())
                TypeVariables[expr.Name] = expr.Def;
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