using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    /// <summary>
    /// Wrapper around a parameter in a library function parameter. 
    /// </summary>
    public class ParamInfo
    {
        public ParameterDefinition Parameter { get; }
        public string Name => Parameter.Name;
        public TypeInfo Type { get; }

        public ParamInfo(ParameterDefinition parameter)
        {
            Debug.Assert(parameter != null);
            Parameter = parameter;
            Type = new TypeInfo(parameter.Type);
        }
    }

    // 
    public class TypeVar
    {
        public string Name => $"T{Index}";
        public TypeParameterDefinition Definition { get; }
        public bool HasDefinition => Definition != null;
        public TypeExpression Constraint { get; }
        public int Index { get; }

        public TypeVar(int index, TypeParameterDefinition def, TypeExpression constraint)
        {
            Definition = def;
            Index = index;
            Constraint = constraint;
        }
    }

    /// <summary>
    /// Contains extra information about a type used in a library function.
    /// A type used in a library function is either 
    /// </summary>
    public class TypeInfo
    {
        public TypeExpression Expression { get; }
        public string Name => Expression.Name;
        public bool IsSelfConstrained => Definition.IsSelfConstrained();
        public TypeDefinition Definition => Expression.Definition;
        public bool IsGeneric => TypeParameters.Count > 0;
        public bool IsConcept => Definition.IsConcept();
        public bool IsConcrete => Definition.IsConcrete();

        public TypeInfo(TypeExpression expr)
        {
            Expression = expr;
            Debug.Assert(Expression != null);
            Debug.Assert(Definition != null);
            Debug.Assert(Expression.TypeArgs.Count == 0);
            // Either IsConcept, or IsConcrete, but never both or neither. 
            Debug.Assert(IsConcept ^ IsConcrete);
        }

        public IReadOnlyList<TypeParameterDefinition> TypeParameters
            => Definition.TypeParameters;

        public IEnumerable<string> TypeParameterNames
            => TypeParameters.Select(tp => tp.Name);

        public string TypeParameterStr
            => TypeParameters.Count > 0 ? "" : "<" + TypeParameterNames.JoinStringsWithComma() + ">";

        public override string ToString()
            => $"{Name}{TypeParameterStr}";

        public override int GetHashCode()
            => ToString().GetHashCode();

        public override bool Equals(object obj)
            => obj is TypeInfo ti && ToString() == ti.ToString();
    }

    /// <summary>
    /// Represents a library function. Used for analyzing library functions and generating code.
    /// </summary>
    public class LibraryFuncInfo
    {
        public string Name => Definition.Name;
        public string TypeSignature { get; }
        public bool HasBody => Definition.Body != null;
        public FunctionDefinition Definition { get; }
        public TypeDefinition ParentType { get; }
        public IReadOnlyList<TypeVar> TypeVariables { get; }
        public IReadOnlyList<ParamInfo> Parameters { get; }

        // Note: when a function returns a concept 
        public bool ReturnsAConcept { get; }
        
        public TypeInfo ReturnType { get; }
        public TypeInfo FirstParameterType => Parameters.Count > 0 ? Parameters[0].Type : null;
        public bool IsFirstParameterConcept => FirstParameterType?.IsConcept == true;
        public bool IsGeneric { get; }
        public int ParameterCount => Parameters.Count;
        public bool HasFixedReturnType;
        
        // public static Comparable Min(this Comparable a, Comparable b) => throw new NotImplementedException();
        // In this case, we are returning a or b. So it is a T where T: Comparable<T>
        // In Plato world, it is a T where T: Comparable. 
        
        // 

        public LibraryFuncInfo(FunctionDefinition definition, TypeDefinition parentType)
        {
            Definition = definition;
            ParentType = parentType;
            Debug.Assert(HasBody);
            Debug.Assert(ParentType.IsLibrary());
            Debug.Assert(definition.ReturnType != null);
            ReturnType = new TypeInfo(definition.ReturnType);
            ReturnsAConcept = ReturnType.Definition.IsConcept();
            Parameters = Definition.Parameters.Select(p => new ParamInfo(p)).ToList();

           
        }

    }
}