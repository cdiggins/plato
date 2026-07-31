using System.Collections.Generic;
using System.Linq;

namespace PlatoTypeInference
{
    /// <summary>
    /// Structs, tuple, class, interface, function, union, array 
    /// With no generics, or with generics substituted.
    /// In other words this a monotype.
    /// </summary>
    public class TypeList : BaseType
    {
        public override string Name { get; }

        public IReadOnlyList<BaseType> Types { get; }

        public TypeList(IEnumerable<BaseType> types)
        {
            Types = types.ToList();
            Name = "[" + string.Join(",", Types) + "]";
        }
    }
}