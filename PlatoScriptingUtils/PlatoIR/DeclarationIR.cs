using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public class DeclarationIR : IR
    {
        public bool IsStatic { get; set; }
        public DeclarationIR Parent { get; set; }
        public string Name { get; set; }
        public TypeReferenceIR Type { get; set; }
        public TypeReferenceIR ParentType { get; set;  }
        public bool IsMemberDeclaration => ParentType != null;
        public virtual IEnumerable<ExpressionIR> Expressions
            => Enumerable.Empty<ExpressionIR>();
    }

    public class MethodDeclarationIR : DeclarationIR
    {
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public BlockStatementIR Body { get; set; }
        public List<TypeParameterDeclarationIR> TypeParameters { get; set;  } = new List<TypeParameterDeclarationIR>();
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);
    }

    public class TypeParameterDeclarationIR : DeclarationIR
    { }

    public class ParameterDeclarationIR : DeclarationIR
    {
        public ExpressionIR DefaultValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(DefaultValue, 1);
    }

    public class ConstructorDeclarationIr : MethodDeclarationIR
    {
    }

    public class FieldDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }

        public override IEnumerable<ExpressionIR> Expressions
            => Enumerable.Repeat(InitialValue, 1);
    }

    public class PropertyDeclarationIR : DeclarationIR
    {
        public MethodDeclarationIR Getter { get; set; }
    }

    public class IndexerDeclarationIr : MethodDeclarationIR
    {
        public MethodDeclarationIR Getter { get; set; }
    }

    public class OperationDeclarationIr : MethodDeclarationIR
    {
    }

    public class TypeDeclarationIR : DeclarationIR
    {
        public TypeDeclarationIR(string kind, string name)
            => (Name, Kind) = (name, kind);
        public string Kind { get; }
        public TypeReferenceIR BaseClass { get; set; }
        public List<TypeReferenceIR> Interfaces { get; set; } = new List<TypeReferenceIR>();
        public List<FieldDeclarationIR> Fields { get; set; } = new List<FieldDeclarationIR>();
        public List<MethodDeclarationIR> Methods { get; set; } = new List<MethodDeclarationIR>();
        public List<ConstructorDeclarationIr> Constructors { get; set; } = new List<ConstructorDeclarationIr>();
        public List<PropertyDeclarationIR> Properties { get; set; } = new List<PropertyDeclarationIR>();
        public List<IndexerDeclarationIr> Indexers { get; set; } = new List<IndexerDeclarationIr>();
        public List<OperationDeclarationIr> Operations { get; set; } = new List<OperationDeclarationIr>();
        public List<TypeParameterDeclarationIR> TypeParameters { get; set; } = new List<TypeParameterDeclarationIR>();
    }

    public static class IRExtensions
    {
        public static T SetParent<T>(this T self, DeclarationIR parent) where T: DeclarationIR
        {
            self.Parent = parent;
            return self;
        }
    }
}