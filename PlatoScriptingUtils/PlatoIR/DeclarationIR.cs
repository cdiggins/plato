using System;
using System.Collections.Generic;

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
    }

    public class FunctionDeclarationIR : DeclarationIR
    {
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public StatementIR Body { get; set; }
        public List<TypeParameterDeclarationIR> TypeParameters { get; set;  } = new List<TypeParameterDeclarationIR>();
        public TypeReferenceIR ReturnType { get; set; }
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }
    }

    public class TypeParameterDeclarationIR : DeclarationIR
    { }

    public class ParameterDeclarationIR : DeclarationIR
    {
        public ExpressionIR DefaultValue { get; set; }
    }

    public class MethodDeclarationIr : FunctionDeclarationIR
    {
    }

    public class ConstructorDeclarationIr : MethodDeclarationIr
    {
    }

    public class FieldDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }
    }

    public class PropertyDeclarationIR : DeclarationIR
    {
        public MethodDeclarationIr Getter { get; set; }
        public bool HasInit { get; set; }
    }

    public class IndexerDeclarationIr : MethodDeclarationIr
    {
        public MethodDeclarationIr Getter { get; set; }
    }

    public class ConverterDeclarationIr : MethodDeclarationIr
    {
        public bool IsImplicit { get; set; }
    }

    public class OperationDeclarationIr : MethodDeclarationIr
    {
    }

    public class TypeDeclarationIR : DeclarationIR
    {
        public TypeDeclarationIR BaseClassDeclaration { get; set; }
        public List<TypeDeclarationIR> Interface { get; set; } = new List<TypeDeclarationIR>();
        public List<FieldDeclarationIR> Fields { get; set; } = new List<FieldDeclarationIR>();
        public List<MethodDeclarationIr> Methods { get; set; } = new List<MethodDeclarationIr>();
        public List<ConstructorDeclarationIr> Constructors { get; set; } = new List<ConstructorDeclarationIr>();
        public List<ConverterDeclarationIr> Converters { get; set; } = new List<ConverterDeclarationIr>();
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