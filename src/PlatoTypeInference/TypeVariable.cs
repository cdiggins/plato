// Represents an unresolved variable in the inference term model.
namespace PlatoTypeInference
{
    public class TypeVariable : BaseType
    {
        public override string Name { get; }
        public TypeVariable(string name) => Name = name;
    }
}