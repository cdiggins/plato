using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.Compiler.Analysis
{
    public class ConstrainedTypeVariable
    {
        public readonly TypeInstance SourceType;
        public readonly TypeInstance Constraint;
        public readonly TypeInstance TypeVariable; 
        public readonly string ParameterName;
        public string Source => SourceType.ToString();

        public ConstrainedTypeVariable(TypeInstance src, TypeInstance tv, TypeInstance constraint = null)
        {
            SourceType = src;
            if (!src.Def.IsInterface() && !src.Def.IsTypeVariable())
                throw new Exception("Expected an interface or type variable");
            if (!tv.Def.IsTypeVariable())
                throw new Exception("Expected a type variable");
            ParameterName = tv.Name;
            TypeVariable = tv;
            Constraint = constraint;
        }
    }

    /// <summary>
    /// Used in a FunctionInstance to gather all interface references and type variables.
    /// They are going to ultimately get transformed into function type parameters.
    /// The interface in the default position, becomes a "Self" type parameter
    /// </summary>
    public class FunctionTypeVariableAnalysis
    {
        public readonly List<ConstrainedTypeVariable> TypeVariables = new List<ConstrainedTypeVariable>();
        public readonly TypeInstance ReturnType;
        public readonly List<TypeInstance> ParameterTypes;
        
        public IReadOnlyList<string> GetTypeVariableNames() 
            => TypeVariables.Select(tv => tv.ParameterName).OrderBy(t => t).ToList();

        public FunctionTypeVariableAnalysis(IReadOnlyList<TypeInstance> parameterTypes, TypeInstance resultType)
        {
            ParameterTypes = parameterTypes.Select(GetOrGenerateInterface).ToList();
            ReturnType = GetOrGenerateInterface(resultType);
        }

        public bool TryGetTypeVariable(TypeInstance input, out TypeInstance result)
        {
            result = null;
            var rep = input.ToString();
            for (var i = 0; i < TypeVariables.Count; i++)
            {
                if (TypeVariables[i].Source == rep)
                {
                    result = TypeVariables[i].TypeVariable;
                    return true;
                }
            }

            return false;
        }

        public TypeInstance GetOrGenerateInterface(TypeInstance ti)
        {
            // Process the arguments recursively to generate type variables
            var args = ti.Args.Select(GetOrGenerateInterface).ToList();

            // If the type is not a type variable or an interface, then we create a new type instance and return it 
            // This is because we might have a class with a type-variable in it. 
            if (!ti.Def.IsTypeVariable() && !ti.Def.IsInterface())
                return new TypeInstance(ti.Expr, args);

            // A special case for the "Self" type parameter
            if (ti.Name == "Self")
            {
                Debug.Assert(ti.Args.Count == 0);
                return ti;
            }


            if (TryGetTypeVariable(ti, out var result))
                return result;

            var rep = ti.ToString();
            for (var i = 0; i < TypeVariables.Count; i++)
            {
                if (TypeVariables[i].Source == rep)
                    return TypeVariables[i].TypeVariable;
            }

            var constraint = ti.Def.IsInterface() 
                ? new TypeInstance(ti.Expr, args) 
                : null;

            var tv = new ConstrainedTypeVariable(ti, CreateTypeVariable(), constraint);
            TypeVariables.Add(tv);
            return tv.TypeVariable;
        }

        public TypeInstance CreateTypeVariable()
        {
            var typeVariableName = $"_T{TypeVariables.Count}";
            var typeVariable = new TypeVariable(null, typeVariableName);
            return new TypeInstance(typeVariable.ToTypeExpression(), null);
        }

        public override string ToString()
        {
            var tmp = TypeVariables.Select(tv => $"{tv.TypeVariable}:{tv.Constraint}").ToList();
            return $"<{tmp.JoinStringsWithComma()}>";
        }
    }
}