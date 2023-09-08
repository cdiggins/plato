using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public abstract class DefinitionSymbol : Symbol
    {
        public TypeExpressionSymbol Type { get; }

        public string Name { get; }
        public string UniqueName => Name + "_" + Id;

        protected DefinitionSymbol(TypeExpressionSymbol type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";

        public abstract Reference ToReference();
    }

    public class PredefinedDefinition : DefinitionSymbol
    {
        public PredefinedDefinition(TypeExpressionSymbol typeRef, string name)
            : base(typeRef, name)
        { }

        public override Reference ToReference()
            => new PredefinedReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => Enumerable.Empty<Symbol>();
    }

    public class FunctionDefinition : DefinitionSymbol
    {
        public IReadOnlyList<ParameterDefinition> Parameters { get; }
        public ExpressionSymbol Body { get; }
        public TypeExpressionSymbol ReturnType => Type;

        public FunctionDefinition(string name, TypeExpressionSymbol returnType, ExpressionSymbol body, params ParameterDefinition[] parameters)
            : base(returnType, name)
        {
            Parameters = parameters;
            Body = body;
        }

        public string Signature =>
            $"{Name}("
            + string.Join(",", Parameters.Select(p => $"{p.Name}:{p.Type}"))
            + $"):{Type};";

        public override Reference ToReference()
            => throw new NotSupportedException();

        public override IEnumerable<Symbol> GetChildSymbols()
            => Parameters.Cast<Symbol>().Append(ReturnType).Append(Body);
    }

    public class ParameterDefinition : DefinitionSymbol
    {
        public ParameterDefinition(string name, TypeExpressionSymbol type)
            : base(type, name)
        { }

        public override Reference ToReference()
            => new ParameterReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => new [] { Type };
    }

    public class VariableDefinition : DefinitionSymbol
    {
        public VariableDefinition(string name, TypeExpressionSymbol type)
            : base(type, name) { }

        public override Reference ToReference()
            => throw new NotSupportedException();

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Type };
    }

    public class TypeParameterDefinition : TypeDefinitionSymbol
    {
        public TypeParameterDefinition(string name, TypeExpressionSymbol constraint)
            : base(TypeKind.Variable, name)
            => Constraint = constraint;
        public TypeExpressionSymbol Constraint { get; }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Constraint };
    }

    public class TypeDefinitionSymbol : Symbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionDefinition> Functions => Enumerable.Empty<FunctionDefinition>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function));

        public List<MethodDefinition> Methods { get; } = new List<MethodDefinition>();
        public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();
        public List<TypeParameterDefinition> TypeParameters { get; } = new List<TypeParameterDefinition>();
        public List<TypeExpressionSymbol> Inherits { get; } = new List<TypeExpressionSymbol>();
        public List<TypeExpressionSymbol> Implements { get; } = new List<TypeExpressionSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();
        public string Name { get; }

        public TypeDefinitionSymbol(TypeKind kind, string name)
        {
            Name = name;
            Kind = kind;
        }

        public IEnumerable<TypeDefinitionSymbol> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Definition.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeExpressionSymbol> GetAllImplementedConcepts()
        {
            foreach (var tmp in Implements)
            {
                if (tmp == null)
                {
                    // TODO: move to semantic checker 
                    Debug.WriteLine("TODO: Implements should not have null types");
                    continue;
                }

                yield return tmp;

                if (tmp.Definition != null)
                    foreach (var tmp2 in tmp.Definition.GetAllImplementedConcepts())
                        yield return tmp2;
            }

            foreach (var tmp in Inherits)
            {
                if (tmp == null)
                {
                    // TODO: move to semantic checker 
                    Debug.WriteLine("TODO: Inherits should not have null types");
                    continue;
                }

                yield return tmp;
                if (tmp.Definition != null)
                    foreach (var tmp2 in tmp.Definition.GetAllImplementedConcepts())
                        yield return tmp2;
            }
        }

        public IEnumerable<MemberDefinition> Members => Enumerable.Empty<MemberDefinition>()
            .Concat(Methods).Concat(Fields);

        public IEnumerable<MethodDefinition> GetConceptMethods()
            => GetAllImplementedConcepts().SelectMany(c => c?.Definition?.Methods ?? Enumerable.Empty<MethodDefinition>());

        public TypeExpressionSymbol ToTypeExpression()
            => new TypeExpressionSymbol(this, TypeParameters.Select(tp => tp.ToTypeExpression()).ToArray());

        public override string ToString()
            => $"{Name}_{Id}:{Kind}";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Methods.Cast<Symbol>().Concat(Fields).Concat(TypeParameters).Concat(Inherits).Concat(Implements);
    }

    public abstract class MemberDefinition : DefinitionSymbol
    {
        protected MemberDefinition(TypeDefinitionSymbol parentType, TypeExpressionSymbol type, string name)
            : base(type, name)
        {
            ParentType = parentType;
        }

        public TypeDefinitionSymbol ParentType { get; }
        public FunctionDefinition Function { get; set; }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new Symbol[] { Function, Type };
    }

    public class MethodDefinition : MemberDefinition
    {
        public MethodDefinition(TypeDefinitionSymbol parentType, TypeExpressionSymbol type, string name)
            : base(parentType, type, name)
        { }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FieldDefinition : MemberDefinition
    {
        public FieldDefinition(TypeDefinitionSymbol parentType, TypeExpressionSymbol type, string name)
            : base(parentType, type, name)
        {
            Function = new FunctionDefinition(Name, Type, null,
                new ParameterDefinition("self", parentType.ToTypeExpression()));
        }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FunctionGroupDefinition : DefinitionSymbol
    {
        public List<FunctionDefinition> Functions { get; } = new List<FunctionDefinition>();

        public FunctionGroupDefinition(IEnumerable<FunctionDefinition> functions, string name)
            : base(PrimitiveTypeDefinitions.Function.ToTypeExpression(), name)
        {
            Functions = functions.ToList();
        }

        public void Validate()
        {
            foreach (var f in Functions)
            {
                if (f == null)
                    throw new Exception("Null function");
                if (f.Name != Name)
                    throw new Exception($"All members in group must have the name \"{Name}\" not \"{f.Name}\"");
                var sig = f.Signature;
                if (Functions.Count(f2 => f2.Signature.Equals(sig)) > 1)
                    throw new Exception($"More than one function has signature {sig}");
            }
        }

        public void Add(FunctionDefinition function)
            => Functions.Add(function);

        public string DebugString =>
            string.Join(";", Functions.Select(f => f?.Signature));

        public override Reference ToReference()
            => new FunctionGroupReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => Functions;
    }
}