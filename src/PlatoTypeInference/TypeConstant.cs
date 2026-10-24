// Represents a named constant type in the inference term model.
namespace PlatoTypeInference
{
    public class TypeConstant : BaseType
    {
        public override string Name { get; }
        public TypeConstant(string name) => Name = name;
    }
}