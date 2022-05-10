using System;
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

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            Parameters?.Visit(action);
            Body?.Visit(action);
            TypeParameters?.Visit(action);
        }

        public string TypeParametersString
            => TypeParameters?.Count > 0 ? $"<{string.Join(", ", TypeParameters)}>" : "";
        
        public override string ToString()
            => $"public {StaticModifier} {Type} {Name}{TypeParametersString}({string.Join(", ", Parameters)}) {Body}";

        public override IR Clone()
            => new MethodDeclarationIR()
            {
                Parameters = Parameters.Clone().ToList(),
                Body = Body.TypedClone(),
                TypeParameters = TypeParameters.Clone().ToList(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            InitialValue?.Visit(action);
        }

        public override string ToString()
            => $"{Type} {Name}" + (InitialValue == null ? "" : $" = {InitialValue}");

        public override IR Clone()
            => new VariableDeclarationIR()
            {
                InitialValue = InitialValue.TypedClone(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class TypeParameterDeclarationIR : DeclarationIR
    {
        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
        }

        public override string ToString()
            => $"{Name}";

        public override IR Clone()
            => new TypeParameterDeclarationIR()
            {
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class ParameterDeclarationIR : DeclarationIR
    {
        public ExpressionIR DefaultValue { get; set; }
        public bool IsThisParameter { get; set; }
        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(DefaultValue, 1);

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            DefaultValue?.Visit(action);
        }

        public override string ToString()
            => (IsThisParameter ? "this " : "") + 
            $"{Type} {Name}" + (DefaultValue == null ? "" : $" = {DefaultValue}");
    
        public override IR Clone()
            => new ParameterDeclarationIR()
            {
                DefaultValue = DefaultValue.TypedClone(),
                IsThisParameter = IsThisParameter,
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class ConstructorDeclarationIr : MethodDeclarationIR
    {
        public override IR Clone()
            => new ConstructorDeclarationIr()
            {
                Parameters = Parameters.Clone().ToList(),
                Body = Body.TypedClone(),
                TypeParameters = TypeParameters.Clone().ToList(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class FieldDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            InitialValue?.Visit(action);
        }

        public override string ToString()
            => $"public readonly {StaticModifier} {Type} {Name}" + (InitialValue == null ? "" : $"= {InitialValue}") + ";";

        public override IR Clone()
            => new FieldDeclarationIR()
            {
                InitialValue = InitialValue.TypedClone(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class PropertyDeclarationIR : DeclarationIR
    {
        public FieldReferenceIR Field { get; set; }
        public MethodDeclarationIR Getter { get; set; }
        public string TypeKind { get; set; }

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            Getter?.Visit(action);
        }

        public override string ToString()
            => $"public {StaticModifier} {Type} {Name} {{ get {Getter.Body} }}";

        public override IR Clone()
            => new PropertyDeclarationIR()
            {
                Field = Field.TypedClone(),
                Getter = Getter.TypedClone(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class IndexerDeclarationIR : MethodDeclarationIR
    {
        public MethodDeclarationIR Getter { get; set; }

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            Getter?.Visit(action);
        }

        public override string ToString()
            => $"public {StaticModifier} {Type} this[{string.Join(", ", Getter.Parameters)}] {{ get {Getter.Body} }}";

        public override IR Clone()
            => new IndexerDeclarationIR()
            {
                Getter = Getter.TypedClone(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class OperationDeclarationIR : MethodDeclarationIR
    {
        public override IR Clone()
            => new OperationDeclarationIR()
            {
                Parameters = Parameters.Clone().ToList(),
                Body = Body.TypedClone(),
                TypeParameters = TypeParameters.Clone().ToList(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
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

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            Bases?.Visit(action);
            Fields?.Visit(action);
            Methods?.Visit(action);
            Constructors?.Visit(action);
            Properties?.Visit(action);
            Indexers?.Visit(action);
            Operations?.Visit(action);
            TypeParameters?.Visit(action);
        }

        public string TypeParametersString
            => TypeParameters?.Count > 0 ? $"<{string.Join(", ", TypeParameters)}>" : "";

        public string BasesString
            => Bases?.Count > 0 ? $": {string.Join(", ", Bases)}" : "";

        public override string ToString()
            => $@"public {StaticModifier} {Kind} {Name}{TypeParametersString} {BasesString}
{{
    {string.Join("\n", Fields)}
    {string.Join("\n", Properties)}
    {string.Join("\n", Methods)}
    {string.Join("\n", Constructors)}
    {string.Join("\n", Indexers)}
    {string.Join("\n", Operations)}
}}";

        public override IR Clone()
            => new TypeDeclarationIR(Kind, Name)
            {
                Bases = Bases.Clone().ToList(),
                Fields = Fields.Clone().ToList(),
                Methods = Methods.Clone().ToList(),
                Constructors = Constructors.Clone().ToList(),
                Properties = Properties.Clone().ToList(),
                Indexers = Indexers.Clone().ToList(),
                Operations = Operations.Clone().ToList(),
                TypeParameters = TypeParameters.Clone().ToList(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }

    public class NamespaceDeclarationIR : DeclarationIR
    {
        public List<NamespaceReferenceIR> Usings { get; set; } = new List<NamespaceReferenceIR>();
        public List<TypeDeclarationIR> Types { get; set; } = new List<TypeDeclarationIR>();

        public override void Visit(Func<IR, bool> action)
        {
            if (!action(this)) return;
            Usings?.Visit(action);
            Types?.Visit(action);
        }

        public override IR Clone()
            => new NamespaceDeclarationIR()
            {
                Usings = Usings.Clone().ToList(),
                Types = Types.Clone().ToList(),
                Type = Type.TypedClone(),
                IsStatic = IsStatic,
                Name = Name,
            };
    }
}