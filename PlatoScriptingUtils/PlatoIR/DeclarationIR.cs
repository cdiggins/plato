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

    public class FunctionIR : DeclarationIR
    {
        public List<ParameterIR> Parameters { get; } = new List<ParameterIR>();
        public StatementIR Body { get; set; }
        public List<TypeParameterDeclarationIR> TypeParameters { get; } = new List<TypeParameterDeclarationIR>();
        public TypeReferenceIR ReturnTypeDeclaration { get; set; }
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }
    }

    public class TypeParameterDeclarationIR : DeclarationIR
    { }

    public class ParameterIR : DeclarationIR
    {
        public ExpressionIR DefaultValue { get; set; }
    }

    public class MethodIR : FunctionIR
    {
    }

    public class ConstructorIR : FunctionIR
    {
    }

    public class FieldIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }
    }

    public class PropertyIR : DeclarationIR
    {
        public MethodIR Getter { get; set; }
        public bool HasInit { get; set; }
    }

    public class IndexerIR : DeclarationIR
    {
        public MethodIR Getter { get; set; }
    }

    public class ConverterIR : DeclarationIR
    {
        public MethodIR Method { get; set; }
        public bool IsImplicit { get; set; }
    }

    public class OperationIR : DeclarationIR
    {
        public MethodIR Method { get; set; }
    }

    public class TypeDeclarationIR : DeclarationIR
    {
        public TypeDeclarationIR BaseClassDeclaration { get; set; }
        public List<TypeDeclarationIR> Interface { get; } = new List<TypeDeclarationIR>();
        public List<FieldIR> Fields { get; } = new List<FieldIR>();
        public List<MethodIR> Methods { get; } = new List<MethodIR>();
        public List<ConstructorIR> Constructors { get; } = new List<ConstructorIR>();
        public List<ConverterIR> Converters { get; } = new List<ConverterIR>();
        public List<PropertyIR> Properties { get; } = new List<PropertyIR>();
        public List<IndexerIR> Indexers { get; } = new List<IndexerIR>();
        public List<OperationIR> Operations { get; } = new List<OperationIR>();
        public List<TypeParameterDeclarationIR> TypeParameters { get; } = new List<TypeParameterDeclarationIR>();
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