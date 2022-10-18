using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class ExpressionIR : IR
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

        public override string ToString()
            => $"{Function}({string.Join(",", Args)})";
    }

    public class OperationIR : InvocationIR
    {
        public OperationIR(string op, MethodReferenceIR function, params ExpressionIR[] args)
            : base(function, args) => Operator = op;
        public string Operator { get; }
        public new MethodReferenceIR Function => (MethodReferenceIR)base.Function;
    }

    public class PrefixOperatorIR : OperationIR
    {
        public PrefixOperatorIR(string op, MethodReferenceIR function, ExpressionIR arg)
            : base(op, function, arg) { }

        public ExpressionIR Operand => Args[0];

        public override string ToString()
            => $"{Operator}{Operand}";
    }

    public class PostfixOperatorIR : OperationIR
    {
        public PostfixOperatorIR(string op, MethodReferenceIR function, ExpressionIR arg)
            : base(op, function, arg) { }

        public ExpressionIR Operand => Args[0];

        public override string ToString()
            => $"{Operand}{Operator}";
    }

    public class BinaryOperatorIR : OperationIR
    {
        public BinaryOperatorIR(string op, MethodReferenceIR function, ExpressionIR operand1, ExpressionIR operand2)
            : base(op, function, operand1, operand2) { }
        public ExpressionIR Operand1 => Args[0];
        public ExpressionIR Operand2 => Args[1];

        public override string ToString()
            => $"{Operand1} {Operator} {Operand2}";
    }

    public class TupleIR : ExpressionIR
    {
        public TupleIR(params ExpressionIR[] expressions)
            : base(expressions) { }

        public override string ToString()
            => $"({string.Join(", ", Args)})";
    }

    public class ArrayIR : ExpressionIR
    {
        public ArrayIR(ExpressionIR size, params ExpressionIR[] expressions)
            : base(expressions) => Size = size;
        public ExpressionIR Size { get; }
        public override IEnumerable<ExpressionIR> GetExpressions()
        {
            return base.GetExpressions().Prepend(Size);
        }

        public TypeReferenceIR ElementType => ExpressionType.TypeArguments.FirstOrDefault();
        
        public override string ToString()
            => $"new {ElementType}[] {{ {string.Join(",", Args)} }}";
    }

    public class CastIR : ExpressionIR
    {
        public CastIR(TypeReferenceIR type, ExpressionIR expr)
            : base(expr) => CastType = type;
        public TypeReferenceIR CastType { get; }
        public ExpressionIR CastExpression => Args[0];

        public override string ToString()
            => $"({CastType}){CastExpression}";
    }

    public class SwitchIR : ExpressionIR
    {
        public SwitchIR(ExpressionIR control, params ExpressionIR[] cases)
            : base(cases.Prepend(control).ToArray())
        { }

        public ExpressionIR Control => Args[0];
        public IEnumerable<ExpressionIR> Cases => Args.Skip(1);

        public IEnumerable<(ExpressionIR, ExpressionIR)> Arms =>
            Enumerable.Range(0, (Args.Count - 1) / 2).Select(i => (Args[i * 2], Args[i * 2 + 1]));

        public override string ToString()
            => $"{Control} switch {{ {string.Join(",", Arms.Select(arm => $"{arm.Item1} => {arm.Item2}"))}}}";
    }

    public class DiscardIR : ExpressionIR
    {
        public override string ToString()
            => $"_";
    }

    public class DefaultIR : ExpressionIR
    {
        public DefaultIR(TypeReferenceIR type)
            => DefaultType = type;
        public TypeReferenceIR DefaultType { get; }

        public override string ToString()
            => $"default({DefaultType})";
    }

    public class BaseIR : ExpressionIR
    {
        public override string ToString()
            => $"base";
    }

    public class ThisIR : ExpressionIR
    {
        public override string ToString()
            => $"this";
    }

    public class SubscriptIR : ExpressionIR
    {
        public SubscriptIR(ExpressionIR reciever, ExpressionIR subscript)
            : base(reciever, subscript) { }

        public ExpressionIR Reciever => Args[0];
        public ExpressionIR Subscript => Args[1];

        public override string ToString()
            => $"{Reciever}[{Subscript}]";
    }

    public class ThrowIR : ExpressionIR
    {
        public ThrowIR(ExpressionIR arg)
            : base(arg)
        { }

        public ExpressionIR Expression => Args[0];

        public override string ToString()
            => $"throw {Expression}";
    }

    public class ConditionalIR : ExpressionIR
    {
        public ConditionalIR(ExpressionIR condition, ExpressionIR onTrue, ExpressionIR onFalse)
            : base(condition, onTrue, onFalse) { }

        public ExpressionIR Condition => Args[0];
        public ExpressionIR OnTrue => Args[1];
        public ExpressionIR OnFalse => Args[2];

        public override string ToString()
            => $"{Condition} ? {OnTrue} : {OnFalse}";
    }

    public class TypeOfIR : ExpressionIR
    {
        public TypeOfIR(TypeReferenceIR type)
            => TypeArgument = type;
        
        public TypeReferenceIR TypeArgument { get; }

        public override string ToString()
            => $"typeof({TypeArgument})";
    }

    public class ParenthesizedIR : ExpressionIR
    {
        public ParenthesizedIR(ExpressionIR expr)
            : base(expr) { }

        public ExpressionIR Expression => Args[0];

        public override string ToString()
            => $"({Expression}";
    }

    public class NewIR : ExpressionIR
    {
        public NewIR(TypeReferenceIR type, params ExpressionIR[] args)
            : base(args) => CreatedType = type;
        
        public TypeReferenceIR CreatedType { get;  }

        public override string ToString()
            => $"new {CreatedType}({string.Join(",", Args)})";
    }

    public class LambdaIR : ExpressionIR
    {
        public List<ReferenceIR> CapturedVariables { get; set; } = new List<ReferenceIR>();
        public List<ParameterDeclarationIR> Parameters { get; set; } = new List<ParameterDeclarationIR>();
        public StatementIR Body { get; set; }

        public override string ToString()
            => $"({string.Join(",", Parameters)}) => {Body}";
    }

    public class LiteralIR : ExpressionIR
    {
        public LiteralIR(string text, object value)
            => (Text, Value) = (text, value);
        
        public string Text { get; }
        public object Value { get; }

        public override string ToString()
            => Text;
    }

    public class AssignmentIR : ExpressionIR
    {
        public AssignmentIR(ExpressionIR lvalue, ExpressionIR rvalue)
            : base(lvalue, rvalue) { }
        
        public ExpressionIR LValue => Args[0];
        public ExpressionIR RValue => Args[1];

        public override string ToString()
            => $"{LValue} = {RValue}";
    }

    public class LetIR : ExpressionIR
    {
        public LetIR(VariableDeclarationIR varDecl, ExpressionIR expression)
            : base(expression) => Variable = varDecl;
        
        public VariableDeclarationIR Variable { get; }
        public ExpressionIR Expression => Args[0];
        public override IEnumerable<VariableDeclarationIR> GetDeclarations() => Enumerable.Repeat(Variable, 1);
        public override IEnumerable<ExpressionIR> GetExpressions() => new[] { Variable.InitialValue, Expression };

        public override string ToString()
            => $"{Variable.InitialValue} as var {Variable.Name} in {Expression}";
    }
}