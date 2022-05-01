using System.Collections.Generic;

namespace PlatoIR
{
    public abstract class ReferenceIR : ExpressionIR
    {
        public abstract string Name { get; }
    }

    public abstract class MemberReferenceIR : ReferenceIR
    {
        public ExpressionIR ThisObject { get; set; }
    }

    public class PropertyReferenceIR : MemberReferenceIR
    {
        public override string Name => Property.Name;
        public PropertyIR Property { get; set; }
    }

    public class FieldReferenceIR : MemberReferenceIR
    {
        public override string Name => Field.Name;
        public FieldIR Field { get; set; }
    }

    public class FunctionReferenceIR : ReferenceIR
    {
        public override string Name => Function.Name;
        public FunctionIR Function { get; set; }
    }

    public class MethodReferenceIR : MemberReferenceIR
    {
        public override string Name => Method.Name;
        public MethodIR Method { get; set; }
        public List<TypeIR> TypeArguments { get; } = new List<TypeIR>();
    }

    public class TypeReferenceIR : ReferenceIR
    {
        public override string Name => ReferencedType.Name;
        public TypeIR ReferencedType { get; set; }
        public List<TypeIR> TypeArguments { get; } = new List<TypeIR>();
    }

    public class VariableReferenceIR : ReferenceIR
    {
        public override string Name => Variable.Name;
        public VariableDeclarationIR Variable { get; set; }
    }

    public class ParameterReferenceIR : ReferenceIR
    {
        public override string Name => Parameter.Name;
        public ParameterIR Parameter { get; set; }
    }

    public class TypeParameterReferenceIR : ReferenceIR
    {
        public override string Name => TypeParameter.Name;
        public TypeParameterIR TypeParameter { get; set; }
    }
}