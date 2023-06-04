using System.Collections.Generic;
using System.Linq;

namespace PlatoTypeInference
{
    /// <summary>
    /// A Poly type is effectively a generic type.
    /// A type with forall quantified type variables. 
    /// Technically polytypes include monotypes, but being 
    /// pedantic would just be confusing. 
    /// </summary>
    public class PolyType : BaseType
    {
        public override string Name { get; }
        
        public IReadOnlyList<TypeVariable> Parameters { get; }
        public TypeList TypeList { get; }

        public PolyType(IEnumerable<TypeVariable> variables, TypeList types)
        {
            Parameters = variables.ToList();
            TypeList = types;
            Name = $"\\({string.Join(",", Parameters)}){TypeList.Name}";
        }
    }
}
