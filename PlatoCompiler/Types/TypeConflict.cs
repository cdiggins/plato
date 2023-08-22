using System.Collections.Generic;

namespace Plato.Compiler.Types
{
    public class TypeConflict
    {
        public TypeConflict(string message, IReadOnlyList<TypeConstraint> constraints = null)
        {
            Message = message;
            Constraints = constraints ?? new List<TypeConstraint>();
        }
        public IReadOnlyList<TypeConstraint> Constraints { get; }
        public string Message { get; }
    }
}