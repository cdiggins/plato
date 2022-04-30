using System;
using System.Collections.Generic;

namespace PlatoIR
{
    public class ExpressionIR : IR 
    {
        public TypeIR Type { get; set; }
        public virtual IEnumerable<DeclarationIR> Declarations => Array.Empty<DeclarationIR>();
        public virtual IEnumerable<ExpressionIR> Children => Array.Empty<ExpressionIR>();
    }

    public class ThrowExpressionIR : ExpressionIR
    {
        public ExpressionIR Expression { get; set; }
    }

    public class InvocationIR : ExpressionIR
    {
        public ExpressionIR This { get; set; }
        public ExpressionIR Function { get; set; }
        public List<ExpressionIR> Arguments { get; } = new List<ExpressionIR>();
    }

    public enum OperatorType
    {
        PrefixUnary,
        PostfixUnary,
        Binary,
        Ternary,
    }

    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Increment,
        Decrement,
        Not,
        And,
        Or,
        XOr,
        Equals,
        NotEquals,
        LessThan,
        LessThanEqualTo,
        GreaterThan,
        GreaterThanEqualTo,
        ShiftLeft,
        ShiftRight,
        NullCoalesce,
    }

    public class OperationIR : InvocationIR
    {
        public string OperatorToken { get; set; }
    }

    public class KnownFunctionIR : ExpressionIR
    {
        public FunctionIR Function { get; set; }
    }

    public class GenericFunctionIR : ExpressionIR
    {
        public FunctionIR Function { get; set; }
        public List<TypeIR> TypeArguments { get; } = new List<TypeIR>();
    }

    public enum BuiltInFunctions
    {
        Tuple,
        Array,
        Cast, 
        New,
        Switch,
        Discard,
        Default,
        SizeOf,
        Base,
        This,
        Subscript,
        InterpolatedString,
    }

    public class BuiltInFunctionIR : ExpressionIR
    {
        public BuiltInFunctions Function { get; set; }
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

    public class ConditionalExpressionIR : ExpressionIR
    {
        public ExpressionIR Condition { get; set; }
        public ExpressionIR OnTrue { get; set; }
        public ExpressionIR OnFalse { get; set; }
    }

    public class ArgumentIR : ExpressionIR
    {
        public string Name { get; set; }
        public ExpressionIR Value { get; }
    }

    public class LambdaIR : ExpressionIR
    {
        public List<ParameterIR> CapturedVariables { get; } = new List<ParameterIR>();

        public MethodReferenceIR Method { get; set; }
    }

    public class LiteralIR : ExpressionIR
    {
        public object Value;
    }

    public class AssignmentIR : ExpressionIR
    {
        public ReferenceIR LValue;
        public ExpressionIR RValue;
    }
}