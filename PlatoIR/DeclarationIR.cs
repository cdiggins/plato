using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class DeclarationIR : IR
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public TypeReferenceIR Type { get; set; }
        public virtual IEnumerable<ExpressionIR> Expressions
            => Enumerable.Empty<ExpressionIR>();
        public string StaticModifier
            => IsStatic ? "static" : "";
        public bool IsVoid => Type.IsVoid;
    }

    public class MethodDeclarationIR : DeclarationIR
    {
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public BlockStatementIR Body { get; set; }
        public List<TypeParameterDeclarationIR> TypeParameters { get; set;  } = new List<TypeParameterDeclarationIR>();

        public string TypeParametersString
            => TypeParameters?.Count > 0 ? $"<{string.Join(", ", TypeParameters)}>" : "";
        
        public override string ToString()
            => $"public {StaticModifier} {Type} {Name}{TypeParametersString}({string.Join(", ", Parameters)}) {Body}";
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);

        public override string ToString()
            => $"{Type} {Name}" + (InitialValue == null ? "" : $" = {InitialValue}");
    }

    public class TypeParameterDeclarationIR : DeclarationIR
    {
        public override string ToString()
            => $"{Name}";
    }

    public class ParameterDeclarationIR : DeclarationIR
    {
        public ExpressionIR DefaultValue { get; set; }
        public bool IsThisParameter { get; set; }
        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(DefaultValue, 1);

        public override string ToString()
            => (IsThisParameter ? "this " : "") + 
            $"{Type} {Name}" + (DefaultValue == null ? "" : $" = {DefaultValue}");
    }

    public class ConstructorDeclarationIr : MethodDeclarationIR
    {
    }

    public class FieldDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);

        public override string ToString()
            => $"public readonly {StaticModifier} {Type} {Name}" + (InitialValue == null ? "" : $"= {InitialValue}") + ";";
    }

    public class PropertyDeclarationIR : DeclarationIR
    {
        public FieldReferenceIR Field { get; set; }
        public MethodDeclarationIR Getter { get; set; }
        public string TypeKind { get; set; }

        public override string ToString()
            => $"public {StaticModifier} {Type} {Name} {{ get {Getter.Body} }}";
    }

    // TODO: why is this a method declaration IR, when it has a method declaration IR? 
    public class IndexerDeclarationIR : MethodDeclarationIR
    {
        public MethodDeclarationIR Getter { get; set; }

        public override string ToString()
            => $"public {StaticModifier} {Type} this[{string.Join(", ", Getter.Parameters)}] {{ get {Getter.Body} }}";
    }

    public class OperationDeclarationIR : MethodDeclarationIR
    {
    }

    public class TypeDeclarationIR : DeclarationIR
    {
        public TypeDeclarationIR(string kind, string name)
            => (Name, Kind) = (name, kind);
        public string Kind { get; set; }
        public List<TypeReferenceIR> Bases { get; set; } = new List<TypeReferenceIR>();
        public List<FieldDeclarationIR> Fields { get; set; } = new List<FieldDeclarationIR>();
        public List<MethodDeclarationIR> Methods { get; set; } = new List<MethodDeclarationIR>();
        public List<ConstructorDeclarationIr> Constructors { get; set; } = new List<ConstructorDeclarationIr>();
        public List<PropertyDeclarationIR> Properties { get; set; } = new List<PropertyDeclarationIR>();
        public List<IndexerDeclarationIR> Indexers { get; set; } = new List<IndexerDeclarationIR>();
        public List<OperationDeclarationIR> Operations { get; set; } = new List<OperationDeclarationIR>();
        public List<TypeParameterDeclarationIR> TypeParameters { get; set; } = new List<TypeParameterDeclarationIR>();

        public IR SetKind(string kind)
        {
            Kind = kind;
            return this;
        }

        public bool IsValueType 
            => Kind.Contains("struct");

        public string TypeParametersString
            => TypeParameters?.Count > 0 ? $"<{string.Join(", ", TypeParameters)}>" : "";

        public string BasesString
            => Bases?.Count > 0 ? $": {string.Join(", ", Bases)}" : "";

        public override string ToString()
            => $@"public {StaticModifier} {Kind} {Name}{TypeParametersString} {BasesString}
{{
    {string.Join("\r\n", Fields)}
    {string.Join("\r\n", Properties)}
    {string.Join("\r\n", Methods)}
    {string.Join("\r\n", Constructors)}
    {string.Join("\r\n", Indexers)}
    {string.Join("\r\n", Operations)}
}}";
    }

    public class NamespaceDeclarationIR : DeclarationIR
    {
        public List<NamespaceReferenceIR> Usings { get; set; } = new List<NamespaceReferenceIR>();
        public List<TypeDeclarationIR> Types { get; set; } = new List<TypeDeclarationIR>();
    }
}