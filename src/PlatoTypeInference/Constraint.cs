namespace PlatoTypeInference
{
    public class Constraint
    {
        public BaseType First { get; }
        public BaseType Second { get; }
        public TypeVariable Variable => First as TypeVariable;
        public bool IsError => Variable == null;

        public Constraint(BaseType first, BaseType second)
        {
            First = first;
            Second = second;
        }
    }
}