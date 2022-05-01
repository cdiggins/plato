using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PlatoIR
{
    public class ExpressionIR : IR 
    {
        public TypeIR Type { get; set; }
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
    
    public class PrefixOperationIR : OperationIR { }
    public class PostfixOperationIR : OperationIR {}
    public class BinaryOperationIR : OperationIR {}

    public class KnownFunctionIR : ExpressionIR
    {
        public FunctionIR Function { get; set; }
    }

    public class GenericFunctionIR : ExpressionIR
    {
        public FunctionIR Function { get; set; }
        public List<TypeIR> TypeArguments { get; } = new List<TypeIR>();
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
        public TypeIR ReferencedType { get; set; }
    }

    public enum BuiltInTypes
    {
        Int,
        Float,
        Type,
    }

    public class BuiltInTypeIR : ExpressionIR
    {
        public string Name { get; set; }
    }

    public class ArgumentIR : ExpressionIR
    {
        public string Name { get; set; }
        public ExpressionIR Value { get; set; }
    }

    public class LambdaIR : ExpressionIR
    {
        public List<ParameterIR> CapturedVariables { get; } = new List<ParameterIR>();
        public FunctionReferenceIR Function { get; set; }
    }

    public class LiteralIR : ExpressionIR
    {
        public object Value;
    }

    public class AssignmentIR : ExpressionIR
    {
        public ReferenceIR LValue;
    }
}