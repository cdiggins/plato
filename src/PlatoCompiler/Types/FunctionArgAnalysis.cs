using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Types
{
    public class FunctionArgAnalysis : IComparable<FunctionArgAnalysis>
    {
        public FunctionCallAnalysis Context { get; }
        public Compilation Compilation => Context.Compilation;
        public int Index { get; }
        public string Name => ParameterDef.Name;
        public TypeExpression OriginalParameterType => ParameterDef.Type;
        public ParameterDef ParameterDef { get; }
        public TypeExpression ArgType { get; }
        public TypeExpression ParameterType { get; } 

        public TypeRelation Relation { get; }
        public bool Valid => Relation != null;
        public int Depth => Relation?.Depth ?? -1;
        public List<TypeReplacement> TypeReplacements { get; } = new List<TypeReplacement>();
        public Dictionary<string, TypeExpression> TypeConstraints { get; } = new Dictionary<string, TypeExpression>();
   
        public FunctionArgAnalysis(FunctionCallAnalysis context, int index, ParameterDef parameterDef, TypeExpression parameterType, TypeExpression argType)
        {
            Context = context;
            Index = index;
            ParameterDef = parameterDef;
            ParameterType = parameterType;
            ArgType = argType;
            Relation = Compilation.GetRelation(argType, OriginalParameterType);
            if (Relation != null)
                GatherTypeReplacements(Relation.Expr, ParameterType);
        }
            
        public void GatherTypeReplacements(TypeExpression from, TypeExpression to)
        {
            if (to.IsTypeVariable)
            {
                TypeReplacements.Add(new TypeReplacement(to.Def, from));
            }
            else
            {
                if (from.Def is TypeParameterDef tpd)
                {
                    TypeConstraints.Add(tpd.Name, to);
                }
                else if (from.Def is TypeVariable tv)
                {
                    TypeConstraints.Add(tv.Name, to);
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

            if (ParameterType.Def.IsInterface())
            {
                if (!ParameterType.Def.IsInterface())
                    return +1;

                if (Depth < arg2.Depth)
                    return -1;
                if (Depth > arg2.Depth)
                    return +1;

                return 0;
            }

            if (arg2.ParameterType.Def.IsInterface())
                return +1;

            Debug.Assert(ParameterType.Def.IsTypeVariable());
            Debug.Assert(arg2.ParameterType.Def.IsTypeVariable());
            
            return 0;
        }
    }
}