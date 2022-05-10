using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class ReferenceIR : ExpressionIR
    {
        protected ReferenceIR(string name, ExpressionIR receiver, IEnumerable<TypeReferenceIR> typeArgs) =>
            (Name, Receiver, TypeArguments) = (name ?? "#unknown", receiver, typeArgs?.ToList() ?? new List<TypeReferenceIR>());
        public string Name { get; }
        public ExpressionIR Receiver { get; set; }
        public List<TypeReferenceIR> TypeArguments { get; set; }
        public abstract DeclarationIR Declaration { get; }

        public override void Visit(Func<IR, bool> action)
        {
            action(this);
        }

        public string TypeArgumentsString =>
            TypeArguments?.Count > 0 ? $"<{string.Join(", ", TypeArguments)}>" : "";

        public string ReceieverString =>
            Receiver != null ? $"{Receiver}." : "";

        public override string ToString()
            => $"{ReceieverString}{Name}{TypeArgumentsString}";
    }

    public class UnknownReferenceIR : ReferenceIR
    {
        public UnknownReferenceIR(string name, ExpressionIR reciever, IEnumerable<TypeReferenceIR> types)
            : base(name, reciever, types)
        {}

        public override DeclarationIR Declaration => null;

        public override IR Clone()
            => new UnknownReferenceIR(Name, Receiver.TypedClone(), TypeArguments.Clone());
    }

    public class NamespaceReferenceIR : ReferenceIR
    {
        public NamespaceReferenceIR(string name, ExpressionIR reciever, NamespaceDeclarationIR namespaceDeclaration)
            : base(name, reciever, null)
        { }

        public NamespaceDeclarationIR Namespace { get; }
        public override DeclarationIR Declaration => Namespace;

        public override IR Clone()
            => new NamespaceReferenceIR(Name, Receiver.TypedClone(), Namespace);
    }

    public class PropertyReferenceIR : ReferenceIR
    {
        public PropertyReferenceIR(string name, ExpressionIR receiver, PropertyDeclarationIR propertyDeclaration)
            : base(name, receiver, null) => PropertyDeclaration = propertyDeclaration;
        public PropertyDeclarationIR PropertyDeclaration { get; }
        public override DeclarationIR Declaration => PropertyDeclaration;

        public override IR Clone()
            => new PropertyReferenceIR(Name, Receiver.TypedClone(), PropertyDeclaration);
    }

    public class FieldReferenceIR : ReferenceIR
    {
        public FieldReferenceIR(string name, ExpressionIR reciever, FieldDeclarationIR fieldDeclaration)
            : base(name, reciever, null) => FieldDeclaration = fieldDeclaration;
        public FieldDeclarationIR FieldDeclaration { get; }
        public override DeclarationIR Declaration => FieldDeclaration;
        public override IR Clone()
            => new FieldReferenceIR(Name, Receiver.TypedClone(), FieldDeclaration);
    }

    public class MethodReferenceIR : ReferenceIR
    {
        public MethodReferenceIR(string name, ExpressionIR reciever, MethodDeclarationIR methodDeclaration, IEnumerable<TypeReferenceIR> typeArgs)
            : base(name, reciever, typeArgs) => MethodDeclaration = methodDeclaration;
        public MethodDeclarationIR MethodDeclaration { get; }
        public override DeclarationIR Declaration => MethodDeclaration;
        public override IR Clone()
            => new MethodReferenceIR(Name, Receiver.TypedClone(), MethodDeclaration, TypeArguments.Clone());
    }

    public class TypeReferenceIR : ReferenceIR
    {
        public TypeReferenceIR(string name, ExpressionIR reciever, TypeDeclarationIR type, TypeParameterDeclarationIR typeParameter, IEnumerable<TypeReferenceIR> typeArgs)
            : base(name, reciever, typeArgs) => (TypeDeclaration, TypeParameterDeclaration) = (type, typeParameter);
        public TypeReferenceIR(string name, ExpressionIR reciever, TypeDeclarationIR type, IEnumerable<TypeReferenceIR> typeArgs)
            : this(name, reciever, type, null, typeArgs) { }
        public TypeReferenceIR(string name, ExpressionIR reciever, TypeParameterDeclarationIR typeParameter)
            : this(name, reciever, null, typeParameter, null) {  }
        public TypeDeclarationIR TypeDeclaration { get; }
        public TypeParameterDeclarationIR TypeParameterDeclaration { get; }
        public override DeclarationIR Declaration 
            => TypeDeclaration as DeclarationIR ?? TypeParameterDeclaration;
        public bool IsVoid => Name.ToLowerInvariant() == "void";

        public override string ToString()
            => IsVoid ? "void" : base.ToString();

        public override IR Clone()
            => new TypeReferenceIR(Name, Receiver.TypedClone(), TypeDeclaration, TypeParameterDeclaration, TypeArguments.Clone());
    }

    public class VariableReferenceIR : ReferenceIR
    {
        public VariableReferenceIR(string name, VariableDeclarationIR variableDeclaration)
            : base(name, null, null) => VariableDeclaration = variableDeclaration;
        public VariableDeclarationIR VariableDeclaration { get; }
        public override DeclarationIR Declaration => VariableDeclaration;
        public override IR Clone()
            => new VariableReferenceIR(Name, VariableDeclaration);
    }

    public class ParameterReferenceIR : ReferenceIR
    {
        public ParameterReferenceIR(string name, ParameterDeclarationIR parameterDeclaration)
            : base(name, null, null) => ParameterDeclaration = parameterDeclaration;
        public ParameterDeclarationIR ParameterDeclaration { get; }
        public override DeclarationIR Declaration => ParameterDeclaration;
        public override IR Clone()
            => new ParameterReferenceIR(Name, ParameterDeclaration);
    }
}