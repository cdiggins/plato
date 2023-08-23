using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
    public static class Hasher
    {
        public static int Combine(int a, int b)
        {
            unchecked
            {
                return a * 397 ^ b;
            }
        }

        public static int Combine(params int[] codes)
            => codes.Aggregate(0, Combine);

        public static int Combine(IEnumerable<int> codes)
            => codes.Aggregate(0, Combine);

        public static int GetHashCode(object o)
            => o?.GetHashCode() ?? 0;

        public static int Hash(params object[] objects)
            => Combine(objects.Select(GetHashCode));

        public static int Hash<T>(IEnumerable<T> objects)
            => Combine(objects.Select(x => GetHashCode(x)));
    }

    public abstract class Type
    {
        public string Name { get; }
        public TypeDefinition Definition { get; }

        // ReSharper disable once UnusedParameter.Local
        protected Type(string name, TypeDefinition definition, TypeFactory _)
        {
            Name = name;
            Definition = definition;
        }

        public override string ToString()
            => $"{Name}";

        public override bool Equals(object obj)
            => obj != null
               && GetType() == obj.GetType()
               && obj is Type other
               && Name == other.Name;

        public override int GetHashCode()
            => Hasher.Hash(Name);
    }

    public class TypeVariable : Type
    {
        public int Id { get; } = Symbol.NextId++;

        public TypeVariable(string name, TypeDefinition definition, TypeFactory factory)
            : base(name, definition, factory)
        { }

        public override string ToString()
            => $"`{Name}_{Id}";

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is TypeVariable other
               && Id == other.Id;

        public override int GetHashCode()
            => Hasher.Hash(base.GetHashCode(), Id);
    }

    public class SelfType : TypeVariable
    {
        public SelfType(TypeFactory factory)
            : base("Self", null, factory)
        { }
    }

    public class AnyType : TypeVariable
    {
        public AnyType(TypeFactory factory)
            : base("Any", null, factory)
        { }
    }

    public class ConstrainedVariable : TypeVariable
    {
        public Type Constraint { get; }

        public ConstrainedVariable(Type constraint, TypeDefinition definition, TypeFactory factory)
            : base(constraint.Name, definition, factory)
        {
            Constraint = constraint;
        }

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is ConstrainedVariable other
               && Equals(Constraint, other.Constraint);

        public override int GetHashCode()
            => Hasher.Combine(base.GetHashCode(), Constraint.GetHashCode());
    }

    public class TypeReference : Type
    {
        public IReadOnlyList<Type> TypeArguments { get; }
        public int DefId => Type.Id;
        public TypeDefinition Type => (TypeDefinition)Definition;

        public TypeReference(TypeDefinition definition, IReadOnlyList<Type> args, TypeFactory factory)
            : base(definition.Name, definition, factory)
        {
            TypeArguments = args;
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeArguments);
            return $"{Name}_{DefId}<{tps}>";
        }

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is TypeReference other
               && DefId == other.DefId
               && TypeArguments.SequenceEqual(other.TypeArguments);

        public override int GetHashCode()
            => Hasher.Combine(base.GetHashCode(), DefId, Hasher.Hash(TypeArguments));
    }

    // A function signature generates types (and constraints)
    // It is like a function. 
    public class TypedFunctionSignature
    {
        public IReadOnlyList<Type> Parameters { get; }
        public Type ReturnType { get; }
        public string Name { get; }

        public TypedFunctionSignature(FunctionDefinition f, TypeFactory factory)
        {
            Name = f.Name;
            Parameters = f.Parameters.Select(factory.CreateType).ToList();
            ReturnType = factory.CreateType(f.Type);
        }

        public override string ToString()
            => $"{Name}({string.Join(", ", Parameters)}): {ReturnType}";

        public override bool Equals(object obj)
            => obj != null
               && obj is TypedFunctionSignature other
               && GetType() == other.GetType()
               && Name == other.Name
               && Equals(ReturnType, other.ReturnType)
               && Parameters.SequenceEqual(other.Parameters);

        public override int GetHashCode()
            => Hasher.Hash(Parameters.Cast<object>().Append(Name).Append(ReturnType).ToArray());
    }

    public class UnifiedType : Type
    {
        public Type TypeA { get; }
        public Type TypeB { get; }
        public UnifiedType(Type typeA, Type typeB, TypeFactory factory)
            : base("$unified_type", null, factory)
        {
            TypeA = typeA;
            TypeB = typeB;
        }   
    }

    public class TypeUnion : Type
    {
        public IReadOnlyList<Type> Options { get; }
        public TypeUnion(IReadOnlyList<Type> options, TypeFactory factory)
            : base("$one_of", null, null)
        {
            Options = options;
        }
    }

    public class TypedFunction
    {
        public TypedFunctionSignature Signature { get; }
        public TypedFunction(FunctionDefinition f, TypeFactory factory)
            => Signature = new TypedFunctionSignature(f, factory);
        public override string ToString()
            => Signature.ToString();
    }

    public class Concept
    {
        public IReadOnlyList<Type> InheritedTypes { get; }
        public List<TypedFunction> Functions { get; } = new List<TypedFunction>();
        public SelfType Self { get; }
        public TypeDefinition Definition { get; }
        public string Name => Definition.Name;

        public Concept(TypeDefinition definition, TypeFactory factory)
        {
            if (!definition.IsConcept())
                throw new Exception("Expected a concept");
            Definition = definition;
            Self = factory.CreateSelf();
            InheritedTypes = definition.Inherits.Select(factory.CreateType).ToList();
        }
    }

    public class TypeFactory
    {
        public Dictionary<TypeDefinition, Type> DefinitionTypes { get; } = new Dictionary<TypeDefinition, Type>();
        public Dictionary<Expression, Type> ExpressionTypes { get; } = new Dictionary<Expression, Type>();

        public List<TypeVariable> TypeVariables { get; } = new List<TypeVariable>();
        public IEnumerable<TypeReference> TypeReferences => AllTypes.OfType<TypeReference>();
        public HashSet<Type> AllTypes { get; } = new HashSet<Type>();
        public IReadOnlyList<Concept> Concepts { get; }
        public Dictionary<FunctionDefinition, TypedFunction> Functions { get; } = new Dictionary<FunctionDefinition, TypedFunction>();
        public SelfType CurrentSelf { get; set; }
        public Dictionary<string, Type> TypeLookup { get; }
        public TypeFactory(IReadOnlyList<TypeDefinition> types)
        {
            Concepts = types
                .Where(t => t.IsConcept())
                .Select(t => new Concept(t, this))
                .ToList();

            TypeLookup = types.ToDictionary(t => t.Name, t => CreateType(t));

            foreach (var c in Concepts)
            {
                ComputeFunctions(c);
            }

            // TODO:
            // Foreach function work out the expression types. 

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
            Functions.Add(fs, tf);
            return tf;
        }

        public T Register<T>(T self) where T : Type
        {
            var typeVar = self as TypeVariable;
            var typeRef = self as TypeReference;
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

            if (typeRef != null)
            {
                AllTypes.Add(self);
                return self;
            }

            throw new Exception($"{self} is neither a type reference or a type variable");
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

        public Type ResolveReturnType(Expression func)
        {
            // TODO: note, I think this could generate a new type variable in some cases. If we don't know what the thing  is. 
            throw new NotImplementedException();
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
            Type r;
            if (ExpressionTypes.ContainsKey(expression))
                return ExpressionTypes[expression];

            foreach (var child in expression.Children)
                Resolve(child);

            switch (expression)
            {
                case Argument argument:
                    r = Resolve(argument);
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
                    r = ResolveReturnType(functionCall);
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