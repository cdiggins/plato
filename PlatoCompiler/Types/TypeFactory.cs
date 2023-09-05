using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// The type factory creates types from type definition and expression symbols.
    /// It also creates Typed Functions from all function definition symbols.
    /// </summary>
    public class TypeFactory
    {
        public Dictionary<TypeExpressionSymbol, Type> TypeExpressionTypes { get; } = new Dictionary<TypeExpressionSymbol, Type>();
        public Dictionary<TypeDefinitionSymbol, Type> TypeDefinitionTypes { get; } = new Dictionary<TypeDefinitionSymbol, Type>();
        public Dictionary<FunctionDefinition, TypedFunction> TypedFunctions { get; } = new Dictionary<FunctionDefinition, TypedFunction>();

        public Dictionary<TypeDefinitionSymbol, Concept> Concepts { get; }
        public Dictionary<string, TypeReference> NamesToTypes { get; }
        public SelfType CurrentSelf { get; set; }

        public HashSet<Type> AllTypes { get; } = new HashSet<Type>();
        public IEnumerable<TypeReference> TypeReferences => AllTypes.OfType<TypeReference>();
        public IEnumerable<TypeVariable> TypeVariables => AllTypes.OfType<TypeVariable>();

        public TypeFactory(IReadOnlyList<TypeDefinitionSymbol> typeDefinitions)
        {
            // Create the concept wrapper classes 
            Concepts = typeDefinitions
                .Where(t => t.IsConcept())
                .ToDictionary(t => t, CreateConcept);

            // Create type reference from type names
            NamesToTypes = typeDefinitions
                .Where(t => t.IsConcreteType() || t.IsConcept())
                .ToDictionary(t => t.Name, t => CreateTypeFromDefinition(t));

            // Create a lookup, so that we can find a type from just a definition
            TypeDefinitionTypes = NamesToTypes.Values
                .ToDictionary(tr => tr.Definition, tr => tr as Type);

            // Resolve all type expression and functions 
            foreach (var t in typeDefinitions)
            {
                // Set the current definition of self 
                if (t.IsConcept())
                {
                    var concept = Concepts[t];
                    CurrentSelf = concept.Self;
                }

                // Create all of the type expressions
                foreach (var symbol in t.GetSymbolTree().OfType<TypeExpressionSymbol>())
                {
                    CreateType(symbol);
                }

                // Create all of the typed functions
                foreach (var symbol in t.GetSymbolTree().OfType<FunctionDefinition>())
                {
                    CreateTypedFunction(symbol);
                }
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

        private Concept CreateConcept(TypeDefinitionSymbol td)
        {
            if (!td.IsConcept())
                throw new Exception("Expected concept type definition");
            var inherits = td.Inherits.Select(CreateType).ToList();
            var selfType = CreateSelf();
            return new Concept(td, selfType, inherits, this);
        }

        public TypedFunction GetTypedFunction(FunctionDefinition fd)
            => TypedFunctions[fd];

        public Type GetType(TypeExpressionSymbol typeExpression)
        {
            if (TypeExpressionTypes.ContainsKey(typeExpression))
                return TypeExpressionTypes[typeExpression];

            if (typeExpression.TypeArgs.Count == 0)
                return TypeDefinitionTypes[typeExpression.Definition];

            throw new Exception($"Unrecognized type expression {typeExpression}");
        }

        public Type FindType(string name)
            => NamesToTypes[name];

        public AnyType CreateAny()
            => Register(new AnyType(this));

        public Type CreateTuple(IReadOnlyList<Type> args)
            => Register(new TypeReference(PrimitiveTypeDefinitions.Tuple, args, this));
        
        // Private methods

        private T Register<T>(T self) where T : Type
        {   
            AllTypes.Add(self);
            return self;
        }

        private SelfType CreateSelf()
            => Register(new SelfType(this));

        private ConstrainedVariable CreateConstrainedVariable(TypeExpressionSymbol concept, TypeDefinitionSymbol definition)
            => Register(new ConstrainedVariable(CreateType(concept), definition, this));

        private TypeReference CreateFunctionType(FunctionDefinition fd)
            => CreateFunctionType(fd.Parameters.Select(p => GetType(p.Type)).ToList(),
                GetType(fd.ReturnType));

        private TypeReference CreateFunctionType(IReadOnlyList<Type> paramTypes, Type returnType)
            => CreateTypeFromDefinition(PrimitiveTypeDefinitions.Function, paramTypes.Append(returnType).ToArray());

        private TypeReference CreateTypeFromDefinition(TypeDefinitionSymbol definition, params Type[] ps)
            => Register(new TypeReference(definition, ps, this));

        private TypedFunction CreateTypedFunction(FunctionDefinition fd)
        {
            var t = CreateFunctionType(fd);
            var tf = new TypedFunction(fd, t);
            TypedFunctions.Add(fd, tf);
            return tf;
        }

        private Type CreateType(TypeExpressionSymbol symbol)
        {
            if (TypeExpressionTypes.ContainsKey(symbol))
                return TypeExpressionTypes[symbol];
            var r = InternalCreateType(symbol);
            TypeExpressionTypes.Add(symbol, r);
            return Register(r);
        }

        private Type InternalCreateType(TypeExpressionSymbol symbol) 
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

            return CreateTypeFromDefinition(def, list.ToArray());
        }
    }
}