using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;

namespace PlatoIR
{
    public class ExpressionIR : IR
    {
        protected ExpressionIR(params ExpressionIR[] args)
            => Args = args?.ToList() ?? new List<ExpressionIR>();
        public List<ExpressionIR> Args { get; }

        public virtual IEnumerable<ExpressionIR> GetExpressions() => Args;
        public virtual IEnumerable<VariableDeclarationIR> GetDeclarations() => Enumerable.Empty<VariableDeclarationIR>();

        public TypeReferenceIR ExpressionType { get; set; }

        public IEnumerable<ExpressionIR> GetAllExpressions() =>
            GetExpressions()
                .SelectMany(x => x?.GetAllExpressions() ?? Enumerable.Empty<ExpressionIR>())
                .Where(x => x != null).Append(this);
    }

    public class InvocationIR : ExpressionIR
    {
        public InvocationIR(ExpressionIR function, params ExpressionIR[] args)
            : base(args) => Function = function;
        public ExpressionIR Function { get; }
        public override IEnumerable<ExpressionIR> GetExpressions() => Args.Prepend(Function);
    }

    public class OperationIR : InvocationIR
    {
        public OperationIR(string op, MethodReferenceIR function, params ExpressionIR[] args)
            : base(function, args) => Operator = op;
        public string Operator { get; }
    }

    public class PrefixOperatorIR : OperationIR
    {
        public PrefixOperatorIR(string op, MethodReferenceIR function, ExpressionIR arg)
            : base(op, function, arg) { }
    }

    public class PostfixOperatorIR : OperationIR
    {
        public PostfixOperatorIR(string op, MethodReferenceIR function, ExpressionIR arg)
            : base(op, function, arg) { }
    }

    public class BinaryOperatorIR : OperationIR
    {
        public BinaryOperatorIR(string op, MethodReferenceIR function, ExpressionIR operand1, ExpressionIR operand2)
            : base(op, function, operand1, operand2) { }
        public ExpressionIR Operand1 => Args[0];
        public ExpressionIR Operand2 => Args[1];
    }

    public class TupleIR : ExpressionIR
    {
        public TupleIR(params ExpressionIR[] expressions)
            : base(expressions) { }
    }

    public class ArrayIR : ExpressionIR
    {
        public ArrayIR(params ExpressionIR[] expressions)
            : base(expressions) { }
    }

    public class CastIR : ExpressionIR
    {
        public CastIR(TypeReferenceIR type, ExpressionIR expr)
            : base(expr) => CastType = type;
        public TypeReferenceIR CastType { get; }
    }

    public class SwitchIR : ExpressionIR
    {
        public SwitchIR(ExpressionIR control, params ExpressionIR[] cases)
            : base(cases.Prepend(control).ToArray())
        { }
    }

    public class DiscardIR : ExpressionIR { }

    public class DefaultIR : ExpressionIR
    {
        public DefaultIR(TypeReferenceIR type)
            => DefaultType = type;
        public TypeReferenceIR DefaultType { get;  }
    }

    public class BaseIR : ExpressionIR { }

    public class ThisIR : ExpressionIR { }

    public class SubscriptIR : ExpressionIR
    {
        public SubscriptIR(ExpressionIR reciever, ExpressionIR arg)
            : base(reciever, arg) { }

        public ExpressionIR Reciever => Args[0];
        public ExpressionIR Subscript => Args[1];
    }

    public class ThrowIR : ExpressionIR
    {
        public ThrowIR(ExpressionIR arg)
            : base(arg)
        { }
    }

    public class ConditionalIR : ExpressionIR
    {
        public ConditionalIR(ExpressionIR condition, ExpressionIR onTrue, ExpressionIR onFalse)
            : base(condition, onTrue, onFalse) { }

        public ExpressionIR Condition => Args[0];
        public ExpressionIR OnTrue => Args[1];
        public ExpressionIR OnFalse => Args[2];
    }

    public class TypeOfIR : ExpressionIR
    {
        public TypeOfIR(TypeReferenceIR type)
            => TypeArgument = type;
        public TypeReferenceIR TypeArgument { get; }
    }

    public class ParenthesizedIR : ExpressionIR
    {
        public ParenthesizedIR(ExpressionIR expr)
            : base(expr) { }
        public ExpressionIR Expression => Args[0];
    }

    public class NewIR : ExpressionIR
    {
        public NewIR(TypeReferenceIR type, params ExpressionIR[] args)
            : base(args) => CreatedType = type;
        public TypeReferenceIR CreatedType { get;  }
    }

    public class LambdaIR : ExpressionIR
    {
        public List<ReferenceIR> CapturedVariables { get; set; } = new List<ReferenceIR>();
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public StatementIR Body { get; set; }
    }

    public class LiteralIR : ExpressionIR
    {
        public LiteralIR(string text, object value)
            => (Text, Value) = (text, value);
        public string Text { get; }
        public object Value { get; }
    }

    public class AssignmentIR : ExpressionIR
    {
        public AssignmentIR(ExpressionIR lvalue, ExpressionIR rvalue)
            : base(lvalue, rvalue) { }
        public ExpressionIR LValue => Args[0];
        public ExpressionIR RValue => Args[1];
    }

    public class LetIR : ExpressionIR
    {
        public LetIR(VariableDeclarationIR varDecl, ExpressionIR expression)
            : base(expression) => Variable = varDecl;
        public VariableDeclarationIR Variable { get; }
        public ExpressionIR Expression => Args[0];
        public override IEnumerable<VariableDeclarationIR> GetDeclarations() => Enumerable.Repeat(Variable, 1);
        public override IEnumerable<ExpressionIR> GetExpressions() => new[] { Variable.InitialValue, Expression };
    }
}