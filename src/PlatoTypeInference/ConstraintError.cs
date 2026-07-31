namespace PlatoTypeInference
{
    public class ConstraintError : Constraint
    {
        public string Message { get; }
        
        public ConstraintError(BaseType typeA, BaseType typeB, string message)
            : base(typeA, typeB)
        {
            Message = message;
        }
    }
}