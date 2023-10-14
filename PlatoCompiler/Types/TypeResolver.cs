namespace Plato.Compiler.Types
{
    /*
    /// <summary>
    /// This class is primarily responsible for assigning types to expressions.
    /// </summary>
    public class TypeResolver
    {
        public bool Success { get; }
        public string Message { get; private set; }
        public Compiler Compiler { get; }
        public Dictionary<Expression, IType> Types => Compiler.ExpressionTypes;
        public FunctionDefinition Function { get; }
        public List<FunctionCallResolver> FunctionCalls { get; } = new List<FunctionCallResolver>();

        public TypeResolver(
            Compiler compiler,  
            FunctionDefinition function)
        {
            Compiler = compiler;

            Function = function;

            try
            {
                if (function != null)
                {
                    Resolve(Function.Body);
                }

                Success = true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                Success = false;
            }
        }

        public void Fail(string reason)
            => Message = reason;


        public TypeExpression Unify(TypeExpression typeA, TypeExpression typeB)
        {
            // TODO: Choose between the best, and if not successful, fail. 
            return typeA; 
        }

        public TypeExpression ResolveFunctionCall(FunctionCall fc)
        {
            var fx = fc.Function;
            var argTypes = fc.Args.Select(Resolve).ToList();

            if (fx is FunctionGroupReference fgr)
            {
                var fcr = new FunctionCallResolver(Compiler, fgr, argTypes);
                FunctionCalls.Add(fcr);
                return fcr.ResultType;
            }

            return CreateAny();
        }

        public TypeExpression Resolve(Expression expression)
        {
            if (expression == null)
                return null;

            TypeExpression r = null;

            switch (expression)
            {
                case Argument argument:
                    r = Resolve(argument.Expression);
                    break;

                case Assignment assignment:
                    r = Resolve(assignment.LValue);
                    break;

                case ConditionalExpression conditionalExpression:
                    r = Unify(
                        Resolve(conditionalExpression.IfTrue),
                        Resolve(conditionalExpression.IfFalse));
                    break;

                case FunctionCall functionCall:
                    r = ResolveFunctionCall(functionCall);
                    break;

                case FunctionGroupReference functionGroupReference:
                    r = Compiler.GetTypeExpression(PrimitiveTypeDefinitions.Function.Name);
                    break;

                case Lambda lambda:
                    r = CreateType(lambda);
                    break;

                case Literal literal:
                    switch (literal.TypeEnum)
                    {
                        case LiteralTypesEnum.Integer:
                            r = Compiler.GetTypeExpression("Integer");
                            break;
                        case LiteralTypesEnum.Number:
                            r = Compiler.GetTypeExpression("Number");
                            break;
                        case LiteralTypesEnum.Boolean:
                            r = Compiler.GetTypeExpression("Boolean");
                            break;
                        case LiteralTypesEnum.String:
                            r = Compiler.GetTypeExpression("String");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case ParameterReference parameterReference:
                    r = parameterReference.Type;
                    break;

                case Parenthesized parenthesized:
                    r = Resolve(parenthesized.Expression);
                    break;

                case PredefinedReference predefinedReference:
                    r = predefinedReference.Definition.Type;
                    break;

                case Reference reference:
                    r = reference.Definition.Type;
                    break;

                case Tuple tuple:
                    r = CreateType(tuple);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            if (r == null)
                throw new Exception($"No type determined for {expression}");
            Types.Add(expression, r);
            return r;
        }

        public TypeExpression CreateAny()
        {
            return new TypeExpression(Compiler.GetTypeDefinition("Any"));
        }

        public TypeExpression CreateType(Lambda lambda)
        {
            return CreateType(lambda.Function);
        }

        public TypeExpression CreateType(FunctionDefinition function)
        {
            var args = function.Parameters.Select(p => p.Type).Append(function.ReturnType).ToArray();
            // TODO: maybe choose one of the numbered functions
            return new TypeExpression(Compiler.GetTypeDefinition("Function"), args);
        }

        public TypeExpression CreateType(Tuple tuple)
        {
            var args = tuple.Children.Select(Resolve).ToArray();
            // TODO: maybe choose one of the numbered tuples
            return new TypeExpression(Compiler.GetTypeDefinition("Tuple"), args);
        }
    }
    */
}