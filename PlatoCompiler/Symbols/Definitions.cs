using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Symbols
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
            else if (ownerType.IsSum && ownerType.Cases.Any(c => c.Name == name))
            {
                // A synthesized per-case static factory (PathSegment2D.Move) — wave-2.
                FunctionType = FunctionType.SumFactory;
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
            else if (ownerType.IsInterface())
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new FunctionDef(Scope, Name, OwnerType, ReturnType, Body?.Rewrite(f), Parameters.ToArray()));
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new VariableDef(Scope, Name, Type, Value?.Rewrite(f) as Expression));
    }

    public class TypeDef : DefSymbol
    {
        public TypeKind Kind { get; }

        // Affine-type modifier (roadmap Phase 6): declared "unique type ...". Only the
        // intrinsic builder types (List, Buffer) may carry it (see UniqueTypes); unique
        // types are backed entirely by handwritten Plato.Intrinsics implementations and
        // are never emitted as generated structs.
        public bool IsUnique { get; set; }

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

        // Sum-type (tagged-union) cases when declared with the "= Case | Case | ...;" body
        // (wave-2, plato-232). Empty for ordinary product types; non-empty exactly when this
        // is a sum type (Kind stays ConcreteType). The cases carry the per-case fields; the
        // flattened struct fields (Kind tag + Case_Field) are synthesized by the C# writer.
        public List<SumCaseDef> Cases { get; } = new List<SumCaseDef>();
        public bool IsSum => Cases.Count > 0;

        public TypeDef(Scope scope, TypeKind kind, string name)
            : base(scope, null, name)
        {
            Kind = kind;
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
            => $"{Name}_{Id}";

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

        // plato-311 (Option A, dual-interface lowering): a concept method is "object safe" when
        // Self appears ONLY as the receiver (Parameters[0]) — never in a later parameter or in the
        // return type. Object-safe methods form the non-generic existential view interface
        // (`interface C`, "any C"); everything else stays reachable only through the F-bounded
        // generic form (`interface C<Self> : C where Self : C<Self>`, "some C"). A Self-RETURNING
        // method is excluded rather than rewritten to return the view — simplest choice that keeps
        // the emitted code straightforward (see tracker/decisions ADR for plato-311).
        //
        // A `_`-receiver (type-level/static) member isn't part of the instance view at all, so it
        // is never object-safe by this test (Plato.CSharpWriter already omits static members from
        // ordinary interface emission unless --static-abstract; this test agrees independently).
        public static bool IsObjectSafeMethod(MethodDef m)
            => IsObjectSafeFunction(m.Function);

        public static bool IsObjectSafeFunction(FunctionDef f)
        {
            if (f.Parameters.Count == 0 || f.Parameters[0].Name == "_")
                return false;
            return !f.ParametersAndReturnType.Skip(1).Any(te => te.UsesSelfType());
        }

        /// <summary>The object-safe surface DIRECTLY DECLARED on this concept (not inherited):
        /// instance methods where Self appears only as the receiver. This is what
        /// Plato.CSharpWriter lists in the concept's own non-generic view interface body — an
        /// inherited object-safe member reaches the view through interface inheritance from the
        /// base concept's OWN view instead of being redeclared here.</summary>
        public IEnumerable<MethodDef> ObjectSafeMethods()
            => Methods.Where(IsObjectSafeMethod);

        /// <summary>True when this concept — or any concept it inherits, transitively — has at
        /// least one object-safe member, i.e. it has a non-generic existential view (`interface
        /// C`) at all. Many concepts (marker/classification concepts like `Geometry3D`, and
        /// composite concepts like `Curve3D` that declare no methods of their own) carry their
        /// entire member surface through inheritance, so this must walk the full ancestry, not
        /// just <see cref="Methods"/>. A concept with no object-safe surface anywhere in its
        /// ancestry has NO view — using it in type position is a CHK308 diagnostic, not emission.</summary>
        public bool HasObjectSafeSurface()
            => GetSelfAndAllInheritedTypes().Any(t => t.Methods.Any(IsObjectSafeMethod));

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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }

    public class TypeParameterDef : TypeDef
    {
        public IReadOnlyList<TypeExpression> Constraints { get; }

        public TypeParameterDef(Scope scope, string name, IReadOnlyList<TypeExpression> constraints = null)
            : base(scope, TypeKind.TypeParameter, name)
        {
            Constraints = constraints ?? Array.Empty<TypeExpression>();
        }

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }

    public class TypeVariable : TypeDef
    {
        public TypeVariable(Scope scope, string name)
            : base(scope, TypeKind.TypeVariable, name)
        { }

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }

    public class SelfType : TypeDef
    {
        public SelfType()
            : base(null, TypeKind.SelfType, "Self")
        { }

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);

        public static SelfType Instance
            => new SelfType();
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }

    public class MethodDef : MemberDef
    {
        public MethodDef(Scope scope, TypeDef parentType, TypeExpression type, string name)
            : base(scope, parentType, type, name)
        { }

        public override RefSymbol ToReference()
            => throw new NotSupportedException();

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
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

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }

    /// <summary>One field of a sum-type case (wave-2, plato-232): a name and a resolved type.
    /// <see cref="FlatName"/> is the name it takes in the flattened tagged struct ("Case_Field").</summary>
    public class SumCaseField
    {
        public string Name { get; }
        public TypeExpression Type { get; }
        public string FlatName { get; }

        public SumCaseField(string caseName, string name, TypeExpression type)
        {
            Name = name;
            Type = type;
            FlatName = $"{caseName}_{name}";
        }
    }

    /// <summary>One case (variant) of a sum type (wave-2, plato-232): a name, a 0-based
    /// declaration-order <see cref="Tag"/>, and its (possibly empty) list of fields.</summary>
    public class SumCaseDef
    {
        public string Name { get; }
        public int Tag { get; }
        public IReadOnlyList<SumCaseField> Fields { get; }

        // The names this case takes in the emitted tagged struct.
        public string PredicateName => "Is" + Name;    // bool Is<Case>() => Kind == Kind_<Case>;
        public string TagConstName => "Kind_" + Name;  // public const int Kind_<Case> = <Tag>;

        public SumCaseDef(string name, int tag, IReadOnlyList<SumCaseField> fields)
        {
            Name = name;
            Tag = tag;
            Fields = fields ?? Array.Empty<SumCaseField>();
        }
    }
}