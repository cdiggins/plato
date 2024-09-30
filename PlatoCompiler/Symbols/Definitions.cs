using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Plato.AST;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    public abstract class DefSymbol : Symbol
    {
        // NOTE: type definitions, have no type expression. It gets circular and confusing. 
        public TypeExpression Type { get; }
        public override string Name { get; }
        public Scope Scope { get; }

        protected DefSymbol(Scope scope, TypeExpression type, string name)
        {
            Scope = scope;
            Type = type;
            Name = name;
        }

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";

        public abstract RefSymbol ToReference();
    }

    public class FunctionDef : DefSymbol, IFunction
    {
        public FunctionType FunctionType { get; }
        public IReadOnlyList<ParameterDef> Parameters { get; }
        public int NumParameters => Parameters.Count;
        public Symbol Body { get; }
        public string GetParameterName(int n) => Parameters[n].Name;
        public TypeExpression GetParameterType(int n) => Parameters[n].Type;
        public TypeExpression ReturnType => Type;
        public TypeDef OwnerType { get; }
        public IEnumerable<TypeExpression> ParametersAndReturnType => Parameters.Select(p => p.Type).Append(ReturnType);
        public IReadOnlyList<ParameterOrVariableRefSymbol> CapturedSymbols { get; }

        public FunctionDef(Scope scope, string name, TypeDef ownerType, TypeExpression returnType, Symbol body, params ParameterDef[] parameters)
            : base(scope, returnType, name)
        {
            if (ownerType == null)
            {
                FunctionType = FunctionType.Lambda;
            }
            else if (name == ownerType.Name)
            {
                FunctionType = FunctionType.Constructor;
            }
            else if (name == "Cast" && parameters.Length == 1)
            {
                 FunctionType = FunctionType.Cast;
            }
            else if (parameters.Length == 1 && ownerType.IsConcrete())
            {
                FunctionType = FunctionType.Field;
            }
            else if (body == null && ownerType.IsLibrary())
            {
                FunctionType = FunctionType.Intrinsic;
            }
            else if (ownerType.IsConcept())
            {
                FunctionType = FunctionType.Concept;
            }
            else if (ownerType.IsLibrary())
            {
                FunctionType = FunctionType.Library;
            }
            else
            {
                throw new Exception("Unknown function type");
            }

            OwnerType = ownerType;
            Parameters = parameters;
            Body = body;

            CapturedSymbols = ComputeCapturedSymbols().ToList();
        }

        public IEnumerable<ParameterOrVariableRefSymbol> ComputeCapturedSymbols()
        {
            var defs = new HashSet<DefSymbol>(GetAllDefs());
            foreach (var reference in GetAllRefs().OfType<ParameterOrVariableRefSymbol>())
            {
                var def = reference.Def;
                Debug.Assert(def != null);
                if (!defs.Contains(def))
                    yield return reference;
            }
        }

        public IEnumerable<DefSymbol> GetAllDefs()
            => this.GetSymbolTree().OfType<DefSymbol>();

        public IEnumerable<RefSymbol> GetAllRefs()
            => this.GetSymbolTree().OfType<RefSymbol>();

        public override RefSymbol ToReference()
            => throw new NotSupportedException();

        public override IEnumerable<Symbol> GetChildSymbols()
            => Parameters.Cast<Symbol>().Append(ReturnType).Append(Body);

        public override string ToString()
            => this.GetSignature();
    }

    public class ParameterDef : DefSymbol
    {
        public ParameterDef(Scope scope, string name, TypeExpression type, int index)
            : base(scope, type, name)
        {
            Index = index;
        }

        public int Index { get; }

        public override RefSymbol ToReference()
            => new ParameterRefSymbol(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => new [] { Type };
    }

    public class VariableDef : DefSymbol
    {
        public VariableDef(Scope scope, string name, TypeExpression type, Expression value)
            : base(scope, type, name)
        {
            Value = value;
        }

        public Expression Value { get; }

        public override RefSymbol ToReference()
            => new VariableRefSymbol(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Type };
    }

    public class TypeDef : DefSymbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionDef> Functions => Enumerable.Empty<FunctionDef>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function))
            .Concat(CompilerGeneratedFunctions);

        public List<MethodDef> Methods { get; } = new List<MethodDef>();
        public List<FieldDef> Fields { get; } = new List<FieldDef>();
        public List<TypeParameterDef> TypeParameters { get; } = new List<TypeParameterDef>();
        public List<TypeExpression> Inherits { get; } = new List<TypeExpression>();
        public List<TypeExpression> Implements { get; } = new List<TypeExpression>();
        public List<FunctionDef> CompilerGeneratedFunctions { get; } = new List<FunctionDef>();

        public SelfType Self { get; }

        public TypeDef(Scope scope, TypeKind kind, string name)
            : base(scope, null, name)
        {
            Kind = kind;
            if (!this.IsTypeVariable())
                Self = new SelfType(scope);
        }

        public IEnumerable<TypeDef> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Def.GetSelfAndAllInheritedTypes()).Append(this);

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
                if (tmp.Def != null)
                    foreach (var tmp2 in tmp.Def.GetAllImplementedConcepts())
                        r.Add(tmp2);
            }

            foreach (var tmp in Inherits)
            {
                r.Add(tmp);
                if (tmp.Def != null)
                    foreach (var tmp2 in tmp.Def.GetAllImplementedConcepts())
                        r.Add(tmp2);
            }

            return r;
        }

        public IEnumerable<MemberDef> Members => Enumerable.Empty<MemberDef>()
            .Concat(Methods).Concat(Fields);

        public IEnumerable<MethodDef> GetConceptMethods()
            => GetAllImplementedConcepts().SelectMany(c => c?.Def?.Methods ?? Enumerable.Empty<MethodDef>());

        public TypeExpression ToTypeExpression()
            => new TypeExpression(this, TypeParameters.Select(tp => tp.ToTypeExpression()).ToArray());

        public override string ToString()
            => $"{Name}_{Id}:{Kind}";

        public override RefSymbol ToReference()
            => new TypeRefSymbol(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => Methods.Cast<Symbol>().Concat(Fields).Concat(TypeParameters).Concat(Inherits).Concat(Implements);

        public bool IsSelfConstrained()
        {
            if (Functions.Any(f => f.ParametersAndReturnType.Skip(1).Any(te => te.UsesSelfType()))) 
                return true;
            if (Inherits.Any(te => te.UsesSelfType()))
                return true;
            return Inherits.Any(i => i.Def.IsSelfConstrained());
        }

        public int DepthTo(TypeDef other)
        {
            if (this.ToString() == other.ToString())
                return 0;
            foreach (var i in Inherits)
            {
                if (i.Def.DepthTo(other) >= 0)
                    return 1 + i.Def.DepthTo(other);
            }
            foreach (var i in Implements)
            {
                if (i.Def.DepthTo(other) >= 0)
                    return 1 + i.Def.DepthTo(other);
            }
            return -1;
        }
    }

    public class TypeParameterDef : TypeDef
    {
        public IReadOnlyList<TypeExpression> Constraints { get; }

        public TypeParameterDef(Scope scope, string name, IReadOnlyList<TypeExpression> constraints)
            : base(scope, TypeKind.TypeVariable, name)
        {
            Constraints = constraints ?? Array.Empty<TypeExpression>();
        }
    }

    public class TypeVariable : TypeDef
    {
        public TypeVariable(Scope scope, string name)
            : base(scope, TypeKind.TypeVariable, name)
        { }
    }

    public class SelfType : TypeParameterDef
    {
        public SelfType(Scope scope)
            : base(scope, "Self", null)
        { }
    }

    public abstract class MemberDef : DefSymbol
    {
        protected MemberDef(Scope scope, TypeDef parentType, TypeExpression type, string name)
            : base(scope, type, name)
        {
            ParentType = parentType;
        }

        public TypeDef ParentType { get; }
        public FunctionDef Function { get; set; }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new Symbol[] { Function, Type };
    }

    public class MethodDef : MemberDef
    {
        public MethodDef(Scope scope, TypeDef parentType, TypeExpression type, string name)
            : base(scope, parentType, type, name)
        { }

        public override RefSymbol ToReference()
            => throw new NotSupportedException();
    }

    public class FieldDef : MemberDef
    {
        public FieldDef(Scope scope, TypeDef parentType, TypeExpression type, string name)
            : base(scope, parentType, type, name)
        {
            Function = new FunctionDef(scope, name, parentType, Type, null, new ParameterDef(scope, "self", parentType.ToTypeExpression(), 0));
        }

        public override RefSymbol ToReference()
            => throw new NotSupportedException();
    }

    public class FunctionGroupDef : DefSymbol
    {
        public List<FunctionDef> Functions { get; }

        // NOTE: for now the "returnType" is always Any. 
        public FunctionGroupDef(Scope scope, TypeExpression returnType, IEnumerable<FunctionDef> functions, string name)
            : base(scope, returnType, name)
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

        public void Add(FunctionDef function)
            => Functions.Add(function);

        public string DebugString =>
            string.Join(";", Functions.Select(f => f?.GetSignature()));

        public override RefSymbol ToReference()
            => new FunctionGroupRefSymbol(this);

        public override IEnumerable<Symbol> GetChildSymbols()
            => Functions;
    }
}