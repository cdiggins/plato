namespace PlatoIR
{
    public static class IRExtensions
    {
        public static VariableReferenceIR ToReference(this VariableDeclarationIR var)
            => new VariableReferenceIR(var.Name, var) { ExpressionType = var.Type };

        public static FieldReferenceIR ToReference(this FieldDeclarationIR var, ExpressionIR reciever = null)
            => new FieldReferenceIR(var.Name, reciever, var) { ExpressionType = var.Type };
    }
}