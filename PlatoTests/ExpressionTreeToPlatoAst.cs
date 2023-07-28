using System.Linq.Expressions;
using System.Reflection;
using Plato.Compiler;

namespace PlatoTests;

public static class ExpressionTreeToPlatoAst
{
    public static IEnumerable<AstNode> ToAst(this IEnumerable<Expression> expressions)
        => expressions.Select(ToAst);

    // https://stackoverflow.com/questions/2490244/default-value-of-a-type-at-runtime
    public static object? GetDefaultValue(this Type t)
        => t.IsValueType && Nullable.GetUnderlyingType(t) == null ? Activator.CreateInstance(t) : null;

    public static AstNode ToAst(this MethodBase mb)
        => AstConstant.Create(mb);

    public static AstNode ToAst(this Func<dynamic, dynamic, dynamic> f)
        => f.Method.ToAst();

    public static AstNode ToAst(this Func<dynamic, dynamic> f)
        => f.Method.ToAst();

    public static AstNode ToAst(this Func<dynamic> f)
        => f.Method.ToAst();

    public static Func<dynamic, dynamic> ToUnaryFunc(this ExpressionType type, Type fixedType = null)
    {
        switch (type)
        {
            case ExpressionType.ArrayLength:
                return xs => xs.Length;

            case ExpressionType.Negate:
                return x => unchecked(-x);

            case ExpressionType.UnaryPlus:
                return x => +x;

            case ExpressionType.NegateChecked:
                return x => checked(-x);

            case ExpressionType.Not:
                return x => !x;

            case ExpressionType.Decrement:
                return x => x - 1;

            case ExpressionType.Increment:
                return x => x + 1;

            case ExpressionType.Throw:
                return x => throw x;

            case ExpressionType.OnesComplement:
                return x => ~x;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static Func<dynamic, dynamic, dynamic> ToBinaryFunc(this ExpressionType type)
    {
        // Note: compound assignments are 
        switch (type)
        {
            case ExpressionType.Add:
                return (a, b) => unchecked(a + b);

            case ExpressionType.AddChecked:
                return (a, b) => a + b;

            case ExpressionType.And:
                return (a, b) => a & b;

            case ExpressionType.AndAlso:
                return (a, b) => a && b;

            case ExpressionType.ArrayIndex:
                return (array, index) => array[index];

            case ExpressionType.Coalesce:
                return (a, b) => a ?? b;

            case ExpressionType.Divide:
                return (a, b) => a / b;

            case ExpressionType.Equal:
                return (a, b) => a == b;

            case ExpressionType.ExclusiveOr:
                return (a, b) => a ^ b;

            case ExpressionType.GreaterThan:
                return (a, b) => a > b;
                
            case ExpressionType.GreaterThanOrEqual:
                return (a, b) => a >= b;

            case ExpressionType.LeftShift:
                return (a, b) => a << b;

            case ExpressionType.LessThan:
                return (a, b) => a < b;

            case ExpressionType.LessThanOrEqual:
                return (a, b) => a <= b;

            case ExpressionType.Modulo:
                return (a, b) => a ^ b;
                    
            case ExpressionType.Multiply:
                return (a, b) => unchecked(a * b);
                    
            case ExpressionType.MultiplyChecked:
                return (a, b) => a * b;
                
            case ExpressionType.NotEqual:
                return (a, b) => a != b;

            case ExpressionType.Or:
                return (a, b) => a | b;

            case ExpressionType.OrElse:
                return (a, b) => a || b;

            case ExpressionType.RightShift:
                return (a, b) => a >> b;
                    
            case ExpressionType.Subtract:
                return (a, b) => unchecked(a - b);

            case ExpressionType.SubtractChecked:
                return (a, b) => checked(a - b);

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static Func<dynamic, dynamic> PropertyGetter(this PropertyInfo pi)
        => x => pi.GetValue(x);

    public static Func<dynamic, dynamic> FieldGetter(this FieldInfo fi)
        => x => fi.GetValue(x);

    public static AstNode ToAst(this MemberExpression member)
    {
        if (member.Member is PropertyInfo pi)
        {
            return AstInvoke.Create(
                pi.PropertyGetter().ToAst(), 
                member.Expression.ToAst());
        }

        if (member.Member is FieldInfo fi)
        {
            return AstInvoke.Create(
                fi.FieldGetter().ToAst(), 
                member.Expression.ToAst());
        }

        throw new NotSupportedException($"Unsupported member type {member.Member}");
    }

    public static AstNode ToAst(this UnaryExpression unaryExpression)
    {
        var operand = unaryExpression.Operand.ToAst();

        if (unaryExpression.Method != null)
        {
            return AstInvoke.Create(unaryExpression.Method.ToAst(), operand);
        }

        var func = unaryExpression.NodeType.ToUnaryFunc();
        return AstInvoke.Create(func.ToAst(), operand);
    }

    public static AstNode ToAst(this BinaryExpression binaryExpression)
    {
        // Note: only supposed to be non null for Coalesce operations
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.binaryexpression.conversion?view=net-7.0
        if (binaryExpression.Conversion != null)
            throw new NotSupportedException();

        var args = new[]
        {
            binaryExpression.Left.ToAst(),
            binaryExpression.Right.ToAst()
        };

        if (binaryExpression.Method != null)
        {
            return AstInvoke.Create(binaryExpression.Method.ToAst(), args);
        }

        if (binaryExpression.NodeType == ExpressionType.Assign)
        {
            var lValue = args[0] as AstIdentifier;

            // TODO: there is probably special handling required when assigning to a field, property, indexer, or array index 

            if (lValue == null)
                throw new Exception(
                    $"Could not determine lvalue from {binaryExpression.Left} converted to {args[0]}");

            return AstAssign.Create(lValue.Text, args[1]);
        }

        var func = binaryExpression.NodeType.ToBinaryFunc();

        // TODO: I have a concern that the same function would create different nodes. 
        // This is fine for now, but should be fixed later. Same for unary expressions

        return AstInvoke.Create(func.ToAst(), args);
    }

    public static AstNode ToAst(this NewExpression expr)
    {
        var ci = expr.Constructor;
        var args = expr.Arguments.ToAst().ToArray();

        if (ci != null)
        {
            var f = ci.ToAst();
            return AstInvoke.Create(f, args);
        }

        if (args.Length != 0)
        {
            throw new NotSupportedException();
        }

        Func<dynamic> r = () => Activator.CreateInstance(expr.Type);
        return AstInvoke.Create(r.ToAst());
    }

    // TODO: this will require a proper type system support 
    public static AstNode ToAst(this Expression? expr)
    {
        if (expr == null)
        {
            return AstConstant.Null;
        }

        if (expr.CanReduce)
        {
            // Only reduces assignment operations. 
            expr = expr.Reduce();
        }

        switch (expr)
        {
            case BinaryExpression binaryExpression:
                return binaryExpression.ToAst();
                    
            case BlockExpression blockExpression:
                return AstBlock.Create(
                    blockExpression.Variables.Concat(blockExpression.Expressions).ToAst().ToArray());
                
            case ConditionalExpression conditionalExpression:
                return AstConditional.Create(
                    conditionalExpression.Test.ToAst(),
                    conditionalExpression.IfTrue.ToAst(),
                    conditionalExpression.IfFalse.ToAst());

            case ConstantExpression constantExpression:
                return AstConstant.Create(constantExpression.Value);
                
            case DebugInfoExpression debugInfoExpression:
                return AstConstant.Create(
                    $"Debug info {debugInfoExpression.StartLine}:{debugInfoExpression.StartColumn} to {debugInfoExpression.EndLine}:{debugInfoExpression.EndColumn}");

            case DefaultExpression defaultExpression:
                return AstConstant.Create(() => defaultExpression.Type.GetDefaultValue());
                
            case DynamicExpression dynamicExpression:
                throw new NotSupportedException("Dynamic expressions are not supported");
                
            case GotoExpression gotoExpression:
                switch (gotoExpression.Kind)
                {
                    case GotoExpressionKind.Goto:
                        throw new NotSupportedException("Dynamic expressions are not supported");

                    case GotoExpressionKind.Return:
                        break;
                        
                    case GotoExpressionKind.Break:
                        return new AstBreak();
                        
                    case GotoExpressionKind.Continue:
                        return new AstContinue();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;

            case IndexExpression indexExpression:
                return indexExpression.ToAst();

            case InvocationExpression invocationExpression:
                return AstInvoke.Create(
                    invocationExpression.Expression.ToAst(),
                    invocationExpression.Arguments.ToAst().ToArray());
                
            case LabelExpression labelExpression:
                throw new Exception($"Labels should be removed");
                
            case LambdaExpression lambdaExpression:
                throw new NotImplementedException();
                //return AstLambda.Create(lambdaExpression.Body.ToAst(), new AstParameterDeclaration(lambdaExpression.Parameters.ToAst().ToArray()));
                
            case ListInitExpression listInitExpression:
                throw new NotSupportedException();
                
            case LoopExpression loopExpression:
                return AstLoop.Create(AstConstant.Create(true), loopExpression.Body.ToAst());
                
            case MemberExpression memberExpression:
                return memberExpression.ToAst();
                    
            case MemberInitExpression memberInitExpression:
                throw new NotSupportedException();

            case MethodCallExpression methodCallExpression:
                return AstInvoke.Create(
                    methodCallExpression.Method.ToAst(),
                    methodCallExpression.Arguments.ToAst().ToArray());

            case NewArrayExpression newArrayExpression:
                throw new NotSupportedException();

            case NewExpression newExpression:
                return newExpression.ToAst();

            case ParameterExpression parameterExpression:
                return AstVarDef.Create(parameterExpression.Name, AstTypeNode.Create("object"));
                    
            case RuntimeVariablesExpression runtimeVariablesExpression:
                throw new NotSupportedException();
                
            case SwitchExpression switchExpression:
                throw new NotSupportedException();
                
            case TryExpression tryExpression:
                // TODO: convert each part into a lambda
                // tryExpression.
                throw new NotSupportedException();

            case TypeBinaryExpression typeBinaryExpression:
            {
                var typeOperand = typeBinaryExpression.TypeOperand;
                var exprOperand = typeBinaryExpression.Expression.ToAst();
                throw new NotImplementedException();
            }
                
            case UnaryExpression unaryExpression:
                return unaryExpression.ToAst();

            default:
                throw new ArgumentOutOfRangeException(nameof(expr));
        }

        throw new NotImplementedException();
    }
}