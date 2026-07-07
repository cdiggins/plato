using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    /// <summary>
    /// Contains extra information about a type used in the analyses. 
    /// </summary>
    public class TypeInfo
    {
        public TypeExpression ActualType { get; }
        public TypeExpression OriginalType { get; }
        public string OriginalName => OriginalType.Name;
        public bool IsSelfConstrained => OriginalDef.IsSelfConstrained();
        public TypeDef OriginalDef => OriginalType.Def;
        public bool IsGeneric => TypeParameters.Count > 0;
        public bool IsConcept => OriginalDef.IsInterface();
        public bool IsConcrete => OriginalDef.IsConcrete();
        public string Name { get; }
        
        public TypeInfo(string name, TypeExpression original, TypeExpression actual)
        {
            Name = name;
            Debug.Assert(OriginalType != null);
            Debug.Assert(OriginalDef != null);
            Debug.Assert(OriginalType.TypeArgs.Count == 0);
            // Either IsConcept, or IsConcrete, but never both or neither. 
            Debug.Assert(IsConcept ^ IsConcrete);
        }

        public IReadOnlyList<TypeParameterDef> TypeParameters
            => OriginalDef.TypeParameters;

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
}