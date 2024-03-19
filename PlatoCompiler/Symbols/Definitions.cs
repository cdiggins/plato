using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.AST;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    public abstract class DefinitionSymbol : Symbol
    {
        public TypeExpression Type { get; }

        public override string Name { get; }

        protected DefinitionSymbol(TypeExpression type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";

        public abstract Reference ToReference();
    }

    public class PredefinedDefinition : DefinitionSymbol
    {
        public PredefinedDefinition(TypeExpression typeRef, string name)
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
        public Symbol Body { get; }
        public string GetParameterName(int n) => Parameters[n].Name;
        public TypeExpression GetParameterType(int n) => Parameters[n].Type;
        public TypeExpression ReturnType => Type;
        public TypeDefinition OwnerType { get; }
        public IEnumerable<TypeExpression> ParametersAndReturnType => Enumerable.Append(Parameters.Select(p => p.Type), ReturnType);

        public FunctionDefinition(string name, TypeDefinition ownerType, TypeExpression returnType, Symbol body, params ParameterDefinition[] parameters)
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

        public override string ToString()
            => this.GetSignature();
    }

    public class ParameterDefinition : DefinitionSymbol
    {
        public ParameterDefinition(string name, TypeExpression type, int index)
            : base(type, name)
        {
            Index = index;
        }

        public int Index { get; }

        public override Reference ToReference()
            => new ParameterReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => new [] { Type };
    }

    public class VariableDefinition : DefinitionSymbol
    {
        public VariableDefinition(string name, TypeExpression type, Expression value)
            : base(type, name)
        {
            Value = value;
        }

        public Expression Value { get; }

        public override Reference ToReference()
            => new VariableReference(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Type };
    }

    public class TypeDefinition : Symbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionDefinition> Functions => Enumerable.Empty<FunctionDefinition>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function))
            .Concat(CompilerGeneratedFunctions);

        public List<MethodDefinition> Methods { get; } = new List<MethodDefinition>();
        public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();
        public List<TypeParameterDefinition> TypeParameters { get; } = new List<TypeParameterDefinition>();
        public List<TypeExpression> Inherits { get; } = new List<TypeExpression>();
        public List<TypeExpression> Implements { get; } = new List<TypeExpression>();
        public List<FunctionDefinition> CompilerGeneratedFunctions { get; } = new List<FunctionDefinition>();

        public override string Name { get; }
       
        public SelfType Self { get; }

        public TypeDefinition(TypeKind kind, string name)
        {
            Name = name;
            Kind = kind;
            if (!this.IsTypeVariable())
                Self = new SelfType();
        }

        public IEnumerable<TypeDefinition> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Definition.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeExpression> GetAllImplementedConcepts()
        {
            var r = new HashSet<TypeExpression>(); 

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

        public TypeExpression ToTypeExpression()
            => new TypeExpression(this, TypeParameters.Select(tp => tp.ToTypeExpression()).ToArray());

        public override string ToString()
            => $"{Name}_{Id}:{Kind}";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Methods.Cast<Symbol>().Concat(Fields).Concat(TypeParameters).Concat(Inherits).Concat(Implements);

        public bool IsSelfConstrained()
        {
            if (Functions.Any(f => f.ParametersAndReturnType.Skip(1).Any(te => te.UsesSelfType()))) 
                return true;
            if (Inherits.Any(te => te.UsesSelfType()))
                return true;
            return Inherits.Any(i => i.Definition.IsSelfConstrained());
        }
    }

    public class TypeParameterDefinition : TypeDefinition
    {
        public IReadOnlyList<TypeExpression> Constraints { get; }

        public TypeParameterDefinition(string name, IReadOnlyList<TypeExpression> constraints)
            : base(TypeKind.TypeVariable, name)
        {
            Constraints = constraints ?? Array.Empty<TypeExpression>();
        }
    }

    public class TypeVariable : TypeDefinition
    {
        public TypeVariable(string name)
            : base(TypeKind.TypeVariable, name)
        { }
    }

    public class SelfType : TypeParameterDefinition
    {
        public SelfType()
            : base("Self", null)
        { }
    }

    public abstract class MemberDefinition : DefinitionSymbol
    {
        protected MemberDefinition(TypeDefinition parentType, TypeExpression type, string name)
            : base(type, name)
        {
            ParentType = parentType;
        }

        public TypeDefinition ParentType { get; }
        public FunctionDefinition Function { get; set; }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new Symbol[] { Function, Type };
    }

    public class MethodDefinition : MemberDefinition
    {
        public MethodDefinition(TypeDefinition parentType, TypeExpression type, string name)
            : base(parentType, type, name)
        { }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FieldDefinition : MemberDefinition
    {
        public FieldDefinition(TypeDefinition parentType, TypeExpression type, string name)
            : base(parentType, type, name)
        {
            Function = new FunctionDefinition(Name, parentType, Type, null,
                new ParameterDefinition("self", parentType.ToTypeExpression(), 0));
        }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FunctionGroupDefinition : DefinitionSymbol
    {
        public List<FunctionDefinition> Functions { get; }

        // NOTE: for now the "returnType" is always Any. 
        public FunctionGroupDefinition(TypeExpression returnType, IEnumerable<FunctionDefinition> functions, string name)
            : base(returnType, name)
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