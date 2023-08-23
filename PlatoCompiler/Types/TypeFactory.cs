using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
    public class TypeFactory
    {
        public Dictionary<TypeDefinition, Type> DefinitionTypes { get; } = new Dictionary<TypeDefinition, Type>();
        public Dictionary<Expression, Type> ExpressionTypes { get; } = new Dictionary<Expression, Type>();

        public List<TypeVariable> TypeVariables { get; } = new List<TypeVariable>();
        public IEnumerable<TypeReference> TypeReferences => AllTypes.OfType<TypeReference>();
        public HashSet<Type> AllTypes { get; } = new HashSet<Type>();
        public IReadOnlyList<Concept> Concepts { get; }
        public Dictionary<FunctionDefinition, TypedFunction> TypedFunctionLookup { get; } = new Dictionary<FunctionDefinition, TypedFunction>();
        public SelfType CurrentSelf { get; set; }
        public Dictionary<string, Type> TypeLookup { get; }

        public IEnumerable<FunctionDefinition> FunctionDefinitions => TypedFunctionLookup.Keys;
        public IEnumerable<TypedFunction> TypedFunctions => TypedFunctionLookup.Values;

        public TypeFactory(IReadOnlyList<TypeDefinition> types)
        {
            Concepts = types
                .Where(t => t.IsConcept())
                .Select(t => new Concept(t, this))
                .ToList();

            TypeLookup = types.Where(t => t.IsType() || t.IsConcept())
                .ToDictionary(t => t.Name, t => CreateType(t));

            foreach (var c in Concepts)
            {
                ComputeFunctions(c);
            }

            foreach (var f in FunctionDefinitions)
            {
                Resolve(f.Body);
                foreach (var x in f.Body.GetExpressionTree())
                    if (GetType(x) == null)
                        throw new Exception($"Could not find type for expression {x}");
            }

            // Note: interesting exercise in assuring that your hash-code/equals are implemented correctly. 
            foreach (var tr1 in TypeReferences)
            {
                foreach (var tr2 in TypeReferences)
                {
                    var refEquals = ReferenceEquals(tr1, tr2);
                    Debug.Assert(refEquals == ReferenceEquals(tr2, tr1), "Reference equals should be equivalent regardless of order");

                    var equals = tr1.Equals(tr2);
                    Debug.Assert(equals == tr2.Equals(tr1), "Equals should be equivalent regardless of order");

                    var s1 = tr1.ToString();
                    var s2 = tr2.ToString();
                    var strEquals = s1.Equals(s2);
                    Debug.Assert(equals == strEquals, "Value and string equivalency should be the same");

                    var h1 = s1.GetHashCode();
                    var h2 = s2.GetHashCode();
                    Debug.Assert(!equals || h1 == h2, "When two values are equal, they should have the same hash code");
                }
            }
        }

        public TypedFunction Register(FunctionDefinition fs, TypedFunction tf)
        {
            TypedFunctionLookup.Add(fs, tf);
            return tf;
        }

        public TypedFunction GetTypedFunction(FunctionDefinition fd)
            => TypedFunctionLookup[fd];

        public Type GetType(Expression expression)
            => ExpressionTypes[expression];

        public T Register<T>(T self) where T : Type
        {
            var typeVar = self as TypeVariable;
            var sym = self.Definition;
            if (sym != null)
            {
                DefinitionTypes[sym] = self;
            }

            if (typeVar != null)
            {
                Debug.Assert(!TypeVariables.Contains(typeVar));
                TypeVariables.Add(typeVar);
                Debug.Assert(!AllTypes.Contains(self));
                AllTypes.Add(self);
                return self;
            }

            AllTypes.Add(self);
            return self;
        }

        public SelfType CreateSelf()
            => Register(new SelfType(this));

        public Type CreateType(ParameterDefinition definition)
            => CreateType(definition.Type);

        public ConstrainedVariable CreateConstrainedVariable(TypeExpression concept, TypeDefinition definition)
            => Register(new ConstrainedVariable(CreateType(concept), definition, this));

        public AnyType CreateAny()
            => Register(new AnyType(this));

        public TypedFunction CreateFunction(FunctionDefinition definition)
            => Register(definition, new TypedFunction(definition, this));

        public void ComputeFunctions(Concept c)
        {
            CurrentSelf = c.Self;
            foreach (var f in c.Definition.Functions)
            {
                var f2 = CreateFunction(f);
                c.Functions.Add(f2);
            }
        }

        public Type GetReturnType(Type t)
        {
            if (t is TypeReference tr)
            {
                if (!tr.Type.Equals(PrimitiveTypeDefinitions.Function))
                {
                    throw new Exception($"Not a reference to a Function instead {tr.Type}");
                }

                if (tr.TypeArguments.Count > 0)
                {
                    return tr.TypeArguments[tr.TypeArguments.Count - 1];
                }

                // Note: this is a reference to an unbound type. It should never happen 
                // return CreateAny();
                throw new Exception("Unexpected number of type arguments");
            }

            if (t is TypeUnion tu)
            {
                var options = tu.Options.Select(GetReturnType).ToList();
                return new TypeUnion(options, this);
            }

            throw new Exception("Can only get return type of unions or functions");
        }

        public Type ResolveReturnType(Expression func)
        {
            var t = GetType(func);
            if (t == null)
                throw new Exception($"Invoked expression {func} was not assigned a type");

            return GetReturnType(t);
        }

        public Type CreateType(Lambda lambda)
        {
            return CreateType(lambda.Function);
        }

        public Type CreateType(Tuple tuple)
        {
            var args = tuple.Children.Select(Resolve).ToArray();
            return CreateType(PrimitiveTypeDefinitions.Tuple, args);
        }

        public Type CreateType(FunctionGroupReference functionGroup)
        {
            var options = functionGroup.Definition.Functions.Select(CreateType).ToArray();
            if (options.Length == 0)
                throw new Exception("Internal error");
            if (options.Length == 1)
                return options[0];
            return Register(new TypeUnion(options, this));
        }

        public Type CreateType(FunctionDefinition function)
        {
            var ret = function.Type;
            var args = function.Parameters.Select(p => p.Type).Append(ret).Select(CreateType).ToArray();
            return CreateType(PrimitiveTypeDefinitions.Function, args);
        }

        public Type FindType(string name)
        {
            return TypeLookup[name];
        }

        public Type Resolve(Expression expression)
        {
            if (expression == null)
                return null;

            Type r;
            if (ExpressionTypes.ContainsKey(expression))
                return ExpressionTypes[expression];

            foreach (var child in expression.Children)
                Resolve(child);

            switch (expression)
            {
                case Argument argument:
                    r = Resolve(argument.Expression);
                    break;

                case Assignment assignment:
                    r = Resolve(assignment.LValue);
                    break;

                case ConditionalExpression conditionalExpression:
                    r = new UnifiedType(
                        Resolve(conditionalExpression.IfTrue),
                        Resolve(conditionalExpression.IfFalse),
                        this);
                    break;

                case FunctionCall functionCall:
                    r = ResolveReturnType(functionCall.Function);
                    break;

                case FunctionGroupReference functionGroupReference:
                    r = CreateType(functionGroupReference);
                    break;

                case Lambda lambda:
                    r = CreateType(lambda);
                    break;

                case Literal literal:
                {
                    switch (literal.TypeEnum)
                    {
                        case LiteralTypesEnum.Integer:
                            r = FindType("Integer");
                            break;
                        case LiteralTypesEnum.Number:
                            r = FindType("Number");
                            break;
                        case LiteralTypesEnum.Boolean:
                            r = FindType("Boolean");
                            break;
                        case LiteralTypesEnum.String:
                            r = FindType("String");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }

                case ParameterReference parameterReference:
                    r = CreateType(parameterReference.Type);
                    break;

                case Parenthesized parenthesized:
                    r = Resolve(parenthesized.Expression);
                    break;

                case PredefinedReference predefinedReference:
                    r = CreateType(predefinedReference.Definition.Type);
                    break;

                case Reference reference:
                    r = CreateType(reference.Definition.Type);
                    break;

                case Tuple tuple:
                    r = CreateType(tuple);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            ExpressionTypes.Add(expression, r);
            return r;
        }

        public Type CreateType(TypeDefinition definition, params Type[] ps)
            => Register(new TypeReference(definition, ps, this));

        public Type CreateType(TypeExpression symbol)
        {
            if (symbol.Name == "Self")
            {
                Debug.Assert(CurrentSelf != null);
                Debug.Assert(symbol.TypeArgs.Count == 0);
                return CurrentSelf;
            }

            var def = symbol.Definition;
            if (def == null)
                throw new Exception("Missing type definition");
            var nArgs = symbol.TypeArgs.Count;
            var nParams = symbol.Definition.TypeParameters.Count;
            if (nArgs > nParams)
                throw new Exception($"Passed too many type arguments {nArgs} expected {nParams}");
            var list = new List<Type>();
            for (var i = 0; i < nParams; ++i)
            {
                var p = symbol.Definition.TypeParameters[i];
                var arg = i < nArgs
                    ? CreateType(symbol.TypeArgs[i])
                    : null;
                if (arg != null)
                    list.Add(arg);
                else if (p.Constraint != null)
                    list.Add(CreateConstrainedVariable(p.Constraint, p));
                else
                    list.Add(CreateAny());
            }

            return CreateType(def, list.ToArray());
        }
    }
}