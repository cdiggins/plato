namespace PlatoTypeInference
{
    public class Substitution
    {
        public BaseType OldType { get; }
        public BaseType NewType { get; }

        public Substitution(BaseType oldType, BaseType newType)
        {
            OldType = oldType;
            NewType = newType;
        }
    }
}