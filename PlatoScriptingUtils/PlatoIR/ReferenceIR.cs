using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class ReferenceIR : ExpressionIR
    {
        protected ReferenceIR(string name, IEnumerable<TypeReferenceIR> typeArgs) =>
            (Name, TypeArguments) = (name ?? "#unknown", typeArgs?.ToList() ?? new List<TypeReferenceIR>());
        public string Name { get; }
        public List<TypeReferenceIR> TypeArguments { get; }
        public abstract DeclarationIR Declaration { get; }
    }

    public abstract class MemberReferenceIR : ReferenceIR
    {
        protected MemberReferenceIR(string name, IEnumerable<TypeReferenceIR> typeArgs)
            : base(name, typeArgs)
        {
        }
    }

    public class PropertyReferenceIR : MemberReferenceIR
    {
        public PropertyReferenceIR(string name, PropertyDeclarationIR propertyDeclaration)
            : base(name, null) => PropertyDeclaration = propertyDeclaration;
        public PropertyDeclarationIR PropertyDeclaration { get; }
        public override DeclarationIR Declaration => PropertyDeclaration;
    }

    public class FieldReferenceIR : MemberReferenceIR
    {
        public FieldReferenceIR(string name, FieldDeclarationIR fieldDeclaration)
            : base(name, null) => FieldDeclaration = fieldDeclaration;
        public FieldDeclarationIR FieldDeclaration { get; }
        public override DeclarationIR Declaration => FieldDeclaration;
    }

    public class MethodReferenceIR : MemberReferenceIR
    {
        public MethodReferenceIR(string name, MethodDeclarationIR methodDeclaration, IEnumerable<TypeReferenceIR> typeArgs)
            : base(name, typeArgs) => MethodDeclaration = methodDeclaration;
        public MethodDeclarationIR MethodDeclaration { get; }
        public override DeclarationIR Declaration => MethodDeclaration;
    }

    public class TypeReferenceIR : ReferenceIR
    {
        public TypeReferenceIR(string name, TypeDeclarationIR type, IEnumerable<TypeReferenceIR> typeArgs)
            : base(name, typeArgs) => TypeDeclaration = type;
        public TypeReferenceIR(string name, TypeParameterDeclarationIR typeParameter)
            : base(name, null) => TypeParameterDeclaration = typeParameter;
        public TypeDeclarationIR TypeDeclaration { get; }
        public TypeParameterDeclarationIR TypeParameterDeclaration { get; }
        public override DeclarationIR Declaration 
            => TypeDeclaration as DeclarationIR ?? TypeParameterDeclaration;

    }

    public class VariableReferenceIR : ReferenceIR
    {
        public VariableReferenceIR(string name, VariableDeclarationIR variableDeclaration)
            : base(name, null) => VariableDeclaration = variableDeclaration;
        public VariableDeclarationIR VariableDeclaration { get; }
        public override DeclarationIR Declaration => VariableDeclaration;
    }

    public class ParameterReferenceIR : ReferenceIR
    {
        public ParameterReferenceIR(string name, ParameterDeclarationIR parameterDeclaration)
            : base(name, null) => ParameterDeclaration = parameterDeclaration;
        public ParameterDeclarationIR ParameterDeclaration { get; }
        public override DeclarationIR Declaration => ParameterDeclaration;
    }
}