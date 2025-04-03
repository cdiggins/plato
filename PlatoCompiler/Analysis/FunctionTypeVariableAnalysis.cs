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
        public readonly string Name;
        public string Source => SourceType.ToString();

        public ConstrainedTypeVariable(TypeInstance src, TypeInstance tv, TypeInstance constraint = null)
        {
            SourceType = src;
            if (!src.Def.IsInterface() && !src.Def.IsTypeVariable())
                throw new Exception("Expected an interface or type variable");
            if (!tv.Def.IsTypeVariable())
                throw new Exception("Expected a type variable");
            Name = tv.Name;
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
        public readonly List<TypeInstance> ParameterTypes = new List<TypeInstance>();
        public bool ConsiderInterfaces;

        public IReadOnlyList<string> GetTypeVariableNames() 
            => TypeVariables.Select(tv => tv.Name).OrderBy(t => t).ToList();

        public FunctionTypeVariableAnalysis(IReadOnlyList<TypeInstance> parameterTypes, TypeInstance resultType, bool considerInterfaces)
        {
            ConsiderInterfaces = considerInterfaces;
            if (parameterTypes.Count > 0)
            {
                ConsiderInterfaces = false;
                ParameterTypes.Add(GetOrGenerateType(parameterTypes[0]));
                ConsiderInterfaces = considerInterfaces;

                for (var i = 1; i < parameterTypes.Count; i++)
                    ParameterTypes.Add(GetOrGenerateType(parameterTypes[i]));
            }

            ReturnType = ReplaceType(resultType);
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

        public TypeInstance GetOrGenerateType(TypeInstance ti)
        {
            // Process the arguments recursively to generate type variables
            var args = ti.Args.Select(GetOrGenerateType).ToList();

            // If the type is not a type variable or an interface, then we create a new type instance and return it 
            // This is because we might have a class with a type-variable in it. 
            if (!ti.Def.IsTypeVariable() && (!ti.Def.IsInterface() || !ConsiderInterfaces))
                return new TypeInstance(ti.Expr, args);

            // NOTE: this is a special case for the array interfaces. 
            // What I've observed is that there are two use cases for interfaces. 
            // As traits or concepts, and as actual interfaces. 
            // This is a bit of a hack or wokaround. I might not want to do this for interfaces at all. 
            if (ti.Def.Name == "IArray" || ti.Def.Name == "IArray2D" || ti.Def.Name == "IArray3D")
                return new TypeInstance(ti.Expr, args);

            if (TryGetTypeVariable(ti, out var result))
                return result;

            var rep = ti.ToString();
            for (var i = 0; i < TypeVariables.Count; i++)
            {
                if (TypeVariables[i].Source == rep)
                    return TypeVariables[i].TypeVariable;
            }

            var tv = CreateTypeVariable();
            
            // The constraint refers to the type variable itself
            if (args.Count > 0)
                    args[0] = tv;
            
            var constraint = ti.Def.IsInterface() 
                ? new TypeInstance(ti.Expr, args) 
                : null;

            var ctv = new ConstrainedTypeVariable(ti, tv, constraint);
            TypeVariables.Add(ctv);
            return ctv.TypeVariable;
        }

        public TypeInstance ReplaceType(TypeInstance ti)
        {
            if (TryGetTypeVariable(ti, out var result))
                return result;

            var args = ti.Args.Select(ReplaceType).ToList();
            return new TypeInstance(ti.Expr, args);
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