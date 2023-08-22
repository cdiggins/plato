using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    public abstract class Definition : Symbol
    {
        public TypeExpression Type { get; }

        public string Name { get; }
        public string UniqueName => Name + "_" + Id;

        protected Definition(TypeExpression type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";

        public abstract Reference ToReference();
    }

    public class PredefinedDefinition : Definition
    {
        public PredefinedDefinition(TypeExpression typeRef, string name)
            : base(typeRef, name)
        { }

        public override Reference ToReference()
            => new PredefinedReference(this);
    }

    public class FunctionDefinition : Definition
    {
        public IReadOnlyList<ParameterDefinition> Parameters { get; }
        public Expression Body { get; }

        public FunctionDefinition(string name, TypeExpression returnType, Expression body, params ParameterDefinition[] parameters)
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
    }

    public class ParameterDefinition : Definition
    {
        public ParameterDefinition(string name, TypeExpression type)
            : base(type, name)
        { }

        public override Reference ToReference()
            => new ParameterReference(this);
    }

    public class VariableDefinition : Definition
    {
        public VariableDefinition(string name, TypeExpression type)
            : base(type, name) { }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class TypeParameterDefinition : TypeDefinition
    {
        public TypeParameterDefinition(string name, TypeExpression constraint)
            : base(TypeKind.Variable, name)
            => Constraint = constraint;
        public TypeExpression Constraint { get; }
    }

    public class TypeDefinition : Symbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionDefinition> Functions => Enumerable.Empty<FunctionDefinition>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function));

        public List<MethodDefinition> Methods { get; } = new List<MethodDefinition>();
        public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();
        public List<TypeParameterDefinition> TypeParameters { get; } = new List<TypeParameterDefinition>();
        public List<TypeExpression> Inherits { get; } = new List<TypeExpression>();
        public List<TypeExpression> Implements { get; } = new List<TypeExpression>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();
        public string Name { get; }

        public TypeDefinition(TypeKind kind, string name)
        {
            Name = name;
            Kind = kind;
        }

        public IEnumerable<TypeDefinition> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Definition.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeExpression> GetAllImplementedConcepts()
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

        public TypeExpression ToTypeExpression()
            => new TypeExpression(this);

        public override string ToString()
        {
            return $"{Name}_{Id}:{Kind}";
        }
    }

    public abstract class MemberDefinition : Definition
    {
        protected MemberDefinition(TypeDefinition parentType, TypeExpression type, string name)
            : base(type, name)
        {
            ParentType = parentType;
        }

        public TypeDefinition ParentType { get; }
        public FunctionDefinition Function { get; set; }
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
            // TODO: the implementation of field methods will be extra work. 
            Function = new FunctionDefinition(Name, Type, null,
                new ParameterDefinition("self", parentType.ToTypeExpression() as TypeExpression));
        }

        public override Reference ToReference()
            => throw new NotSupportedException();
    }

    public class FunctionGroupDefinition : Definition
    {
        public IReadOnlyList<FunctionDefinition> Functions { get; }

        public FunctionGroupDefinition(IReadOnlyList<FunctionDefinition> functions, string name)
            : base(PrimitiveTypeDefinitions.Function.ToTypeExpression() as TypeExpression, name)
        {
            Functions = functions;
            if (Functions.Count == 0) throw new Exception("Expected at least one function in group");

            foreach (var f in Functions)
            {
                if (f == null)
                    throw new Exception("Null function");
                if (f.Name != name)
                    throw new Exception($"All members in group must have the name \"{name}\" not \"{f.Name}\"");
                var sig = f.Signature;
                if (Functions.Count(f2 => f2.Signature.Equals(sig)) > 1)
                    throw new Exception($"More than one function has signature {sig}");
            }
        }

        public FunctionGroupDefinition Add(FunctionDefinition function)
            => new FunctionGroupDefinition(Functions.Append(function).ToList(), Name);

        public string DebugString =>
            string.Join(";", Functions.Select(f => f?.Signature));

        public override Reference ToReference()
            => new FunctionGroupReference(this);
    }
}