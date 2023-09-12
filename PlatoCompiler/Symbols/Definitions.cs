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

    public class FunctionDefinition : DefinitionSymbol, IFunction
    {
        public IReadOnlyList<ParameterDefinition> Parameters { get; }
        public int NumParameters => Parameters.Count;
        public ExpressionSymbol Body { get; }
        public string GetParameterName(int n) => Parameters[n].Name;
        public TypeExpressionSymbol GetParameterType(int n) => Parameters[n].Type;
        public TypeExpressionSymbol ReturnType => Type;
        public TypeDefinitionSymbol OwnerType { get; }
        public FunctionDefinition(string name, TypeDefinitionSymbol ownerType, TypeExpressionSymbol returnType, ExpressionSymbol body, params ParameterDefinition[] parameters)
            : base(returnType, name)
        {
            OwnerType = ownerType;
            Parameters = parameters;
            Body = body;
        }

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

        public string Name { get; }
        
        public SelfType Self { get; }

        public TypeDefinitionSymbol(TypeKind kind, string name)
        {
            Name = name;
            Kind = kind;

            if (Kind == TypeKind.Concept || Kind == TypeKind.Library)
            {
                Self = new SelfType(ToTypeExpression());
            }
        }

        public IEnumerable<TypeDefinitionSymbol> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Definition.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeExpressionSymbol> GetAllImplementedConcepts()
        {
            var r = new HashSet<TypeExpressionSymbol>(); 

            foreach (var tmp in Implements)
            {
                if (tmp == null)
                {
                    // TODO: move to semantic checker 
                    Debug.WriteLine("TODO: Implements should not have null types");
                    continue;
                }

                r.Add(tmp);
                if (tmp.Definition != null)
                    foreach (var tmp2 in tmp.Definition.GetAllImplementedConcepts())
                        r.Add(tmp2);
            }

            foreach (var tmp in Inherits)
            {
                r.Add(tmp);
                if (tmp.Definition != null)
                    foreach (var tmp2 in tmp.Definition.GetAllImplementedConcepts())
                        r.Add(tmp2);
            }

            return r;
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

    public class TypeParameterDefinition : TypeDefinitionSymbol
    {
        public TypeParameterDefinition(string name, TypeExpressionSymbol constraint)
            : base(TypeKind.TypeVariable, name)
            => Constraint = constraint;
        
        public TypeExpressionSymbol Constraint { get; }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Constraint };
    }

    public class SelfType : TypeParameterDefinition
    {
        public SelfType(TypeExpressionSymbol constraint)
            : base("Self", constraint)
        { }
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
            Function = new FunctionDefinition(Name, parentType, Type, null,
                new ParameterDefinition("self", parentType.ToTypeExpression()));
        }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FunctionGroupDefinition : DefinitionSymbol
    {
        public List<FunctionDefinition> Functions { get; }

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
                var sig = f.GetSignature();
                if (Functions.Count(f2 => f2.GetSignature().Equals(sig)) > 1)
                    throw new Exception($"More than one function has signature {sig}");
            }
        }

        public void Add(FunctionDefinition function)
            => Functions.Add(function);

        public string DebugString =>
            string.Join(";", Functions.Select(f => f?.GetSignature()));

        public override Reference ToReference()
            => new FunctionGroupReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => Functions;
    }
}