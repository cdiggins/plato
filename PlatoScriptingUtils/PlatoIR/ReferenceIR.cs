using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class ReferenceIR : ExpressionIR
    {
        protected ReferenceIR(string name) => Name = name ?? "#unknown";
        public string Name { get; }
        public abstract DeclarationIR Declaration { get; }
    }

    public abstract class MemberReferenceIR : ReferenceIR
    {
        protected MemberReferenceIR(string name, ExpressionIR receiver) 
            : base(name) => Receiver = receiver;
        public ExpressionIR Receiver { get; }
    }

    public class PropertyReferenceIR : MemberReferenceIR
    {
        public PropertyReferenceIR(string name, ExpressionIR receiver, PropertyDeclarationIR propertyDeclaration)
            : base(name, receiver) => PropertyDeclaration = propertyDeclaration;
        public PropertyDeclarationIR PropertyDeclaration { get; }
        public override DeclarationIR Declaration => PropertyDeclaration;
    }

    public class FieldReferenceIR : MemberReferenceIR
    {
        public FieldReferenceIR(string name, ExpressionIR receiver, FieldDeclarationIR fieldDeclaration)
            : base(name, receiver) => FieldDeclaration = fieldDeclaration;
        public FieldDeclarationIR FieldDeclaration { get; }
        public override DeclarationIR Declaration => FieldDeclaration;
    }

    public class FunctionReferenceIR : ReferenceIR
    {
        public FunctionReferenceIR(string name, FunctionDeclarationIR functionDeclaration)
            : base(name) => FunctionDeclaration = functionDeclaration;
        public FunctionDeclarationIR FunctionDeclaration { get; }
        public override DeclarationIR Declaration => FunctionDeclaration;
    }

    public class MethodReferenceIR : MemberReferenceIR
    {
        public MethodReferenceIR(string name, ExpressionIR reciever, MethodDeclarationIr methodDeclaration, IEnumerable<TypeReferenceIR> typeArgs = null)
            : base(name, reciever) => (MethodDeclaration, TypeArguments) = (methodDeclaration, typeArgs?.ToList() ?? new List<TypeReferenceIR>());
        public MethodDeclarationIr MethodDeclaration { get; }
        public List<TypeReferenceIR> TypeArguments { get; }
        public override DeclarationIR Declaration => MethodDeclaration;
    }

    public class TypeReferenceIR : ReferenceIR
    {
        public TypeReferenceIR(string name, TypeDeclarationIR type, IEnumerable<TypeReferenceIR> typeArgs = null)
            : base(name) => (TypeDeclaration, TypeArguments) = (type, typeArgs?.ToList() ?? new List<TypeReferenceIR>());
        public TypeDeclarationIR TypeDeclaration { get; }
        public List<TypeReferenceIR> TypeArguments { get; }
        public override DeclarationIR Declaration => TypeDeclaration;
    }

    public class VariableReferenceIR : ReferenceIR
    {
        public VariableReferenceIR(string name, VariableDeclarationIR variableDeclaration)
            : base(name) => VariableDeclaration = variableDeclaration;
        public VariableDeclarationIR VariableDeclaration { get; }
        public override DeclarationIR Declaration => VariableDeclaration;
    }

    public class ParameterReferenceIR : ReferenceIR
    {
        public ParameterReferenceIR(string name, ParameterDeclarationIR parameterDeclaration)
            : base(name) => ParameterDeclaration = parameterDeclaration;
        public ParameterDeclarationIR ParameterDeclaration { get; }
        public override DeclarationIR Declaration => ParameterDeclaration;
    }

    public class TypeParameterReferenceIR : ReferenceIR
    {
        public TypeParameterReferenceIR(string name, TypeParameterDeclarationIR typeParameterDeclaration)
            : base(name) => TypeParameterDeclaration = typeParameterDeclaration;
        public TypeParameterDeclarationIR TypeParameterDeclaration { get; }
        public override DeclarationIR Declaration => TypeParameterDeclaration;
    }
}