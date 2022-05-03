using System;
using System.Collections.Generic;

namespace PlatoIR
{
    public class ExpressionIR : IR 
    {
        public TypeDeclarationIR ExpressionType { get; set; }
        public List<ExpressionIR> Args { get; set; } = new List<ExpressionIR>();
    }

    public class NameIR : ExpressionIR
    {
        public ExpressionIR Object { get; set; }
        public string Name { get; set; }
        public DeclarationIR ReferencedIR { get; set; }
    }

    public class InvocationIR : ExpressionIR
    {
        public ExpressionIR Function { get; set; }
    }

    public class OperationIR : InvocationIR
    {
        public string Operator { get; set; }
    }
    
    public class PrefixOperatorIr : OperationIR { }
    public class PostfixOperatorIr : OperationIR {}

    public class BinaryOperatorIr : OperationIR
    {
        public ExpressionIR Operand1 => Args[0];
        public ExpressionIR Operand2 => Args[1];
    }

    public class KnownFunctionIR : ExpressionIR
    {
        public FunctionDeclarationIR FunctionDeclaration { get; set; }
    }

    public class GenericFunctionIR : ExpressionIR
    {
        public FunctionDeclarationIR FunctionDeclaration { get; set; }
        public List<TypeDeclarationIR> TypeArguments { get; } = new List<TypeDeclarationIR>();
    }

    public class TupleIR : ExpressionIR {}
    public class ArrayIR : ExpressionIR { }

    public class CastIR : ExpressionIR
    {
        public TypeReferenceIR CastType{ get; set; }
    }
    public class SwitchIR : ExpressionIR { }
    public class DiscardIR : ExpressionIR { }

    public class DefaultIR : ExpressionIR
    {
        public TypeReferenceIR DefaultType { get; set; }
    }
    public class BaseIR : ExpressionIR { }
    public class ThisIR : ExpressionIR { }
    public class SubscriptIR : ExpressionIR { }
    public class ThrowIR : ExpressionIR {}
    public class ConditionalIR : ExpressionIR {}
    public class TypeOfIR : ExpressionIR
    {
        public TypeReferenceIR TypeArgument { get; set; }
    }

    public class ParenthesizedIR : ExpressionIR
    { }

    public class NewIR : ExpressionIR
    {
        public TypeReferenceIR CreatedType { get; set; }
    }

    public class KnownType : ExpressionIR
    {
        public TypeDeclarationIR ReferencedTypeDeclaration { get; set; }
    }

    public enum BuiltInTypes
    {
        Int,
        Float,
        Type,
    }

    public class BuiltInTypeIR : ExpressionIR
    {
        public BuiltInTypeIR(string name)
            => Name = name;
        public string Name { get; }
    }

    public class ArgumentIR : ExpressionIR
    {
        public ArgumentIR(string name, ExpressionIR value)
            => (Name, Value) = (name, value);
        public string Name { get; }
        public ExpressionIR Value { get; }
    }

    public class LambdaIR : ExpressionIR
    {
        public List<DeclarationIR> CapturedVariables { get; set; } = new List<DeclarationIR>();
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public StatementIR Body { get; set; }
    }

    public class LiteralIR : ExpressionIR
    {
        public string Text;
        public object Value;
    }

    public class AssignmentIR : ExpressionIR
    {
        public ExpressionIR LValue;
        public ExpressionIR RValue => Args[0];
    }
}