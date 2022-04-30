using System.Collections.Generic;

namespace PlatoIR
{
    public class DeclarationIR : IR
    {
        public bool IsStatic { get; set; }
        public string Name { get; set; }
        public TypeIR Type { get; set; }
        public TypeIR ParentType { get; set;  }
        public StatementIR EnclosingStatement { get; set; }
        public ExpressionIR EnclosingExpression { get; set; }
        public bool IsMembeDeclaration => Type != null;
    }

    public class FunctionIR : DeclarationIR
    {
        public List<ParameterIR> Parameters { get; } = new List<ParameterIR>();
        public StatementIR Body { get; set; }
        public List<TypeParameterIR> TypeParameters { get; } = new List<TypeParameterIR>();
        public TypeIR ReturnType { get; set; }
    }

    public class VariableDeclarationIR : DeclarationIR
    {
        public ExpressionIR InitialValue { get; set; }
    }

    public class TypeIR : DeclarationIR
    {
        public List<TypeParameterIR> TypeParameters { get; } = new List<TypeParameterIR>();
    }

    public class TypeParameterIR : DeclarationIR
    { }

    public class ParameterIR : DeclarationIR
    {
    }

    public class MethodIR : FunctionIR
    {
    }

    public class ConstructorIR : FunctionIR
    {
    }

    public class FieldIR : DeclarationIR
    {
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

    public class PrimitiveTypeIR : TypeIR
    {
    }

    public class ClassIR : TypeIR
    {
        public ClassIR BaseClass { get; set; }
        public List<InterfaceIR> Interface { get; } = new List<InterfaceIR>();
        public List<FieldIR> Fields { get; } = new List<FieldIR>();
        public List<MethodIR> Methods { get; } = new List<MethodIR>();
        public List<ConstructorIR> Constructors { get; } = new List<ConstructorIR>();
        public List<PropertyIR> Properties { get; } = new List<PropertyIR>();
        public List<IndexerIR> Indexers { get; } = new List<IndexerIR>();
    }

    public class InterfaceIR : TypeIR
    {
        public List<InterfaceIR> Interfaces { get; } = new List<InterfaceIR>();
    }
}