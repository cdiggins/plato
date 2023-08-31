using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class TypeFactory
    {
        public Dictionary<TypeDefinition, Type> DefinitionTypes { get; } = new Dictionary<TypeDefinition, Type>();
        public Dictionary<TypeExpression, Type> TypeExpressionTypes { get; } = new Dictionary<TypeExpression, Type>();

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

            TypeLookup = types
                .Where(t => t.IsType() || t.IsConcept())
                .ToDictionary(t => t.Name, t => CreateType(t));

            foreach (var c in Concepts)
            {
                ComputeFunctions(c);
            }

            foreach (var t in types)
            {
                if (!t.IsLibrary())
                    continue;

                foreach (var f in t.Functions)
                    CreateFunction(f);
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

        public Type GetType(TypeExpression typeExpression)
            => TypeExpressionTypes[typeExpression];

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

        public Type CreateFunctionType(IReadOnlyList<Type> paramTypes, Type returnType)
            => CreateType(PrimitiveTypeDefinitions.Function, paramTypes.Append(returnType).ToArray());

        public Type CreateType(FunctionDefinition function)
            => CreateFunctionType(function.Parameters.Select(p => CreateType(p.Type)).ToArray(),
                CreateType(function.Type));

        public Type FindType(string name)
            => TypeLookup[name];

        // TODO: this doesn't make sense: Why not look it up first? 

        public Type CreateType(TypeDefinition definition, params Type[] ps)
            => Register(new TypeReference(definition, ps, this));

        public Type CreateType(TypeExpression symbol)
        {
            if (TypeExpressionTypes.ContainsKey(symbol))
                return TypeExpressionTypes[symbol];
            var r = InternalCreateType(symbol);
            TypeExpressionTypes.Add(symbol, r);
            return r;
        }

        public Type InternalCreateType(TypeExpression symbol) 
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