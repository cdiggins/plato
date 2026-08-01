using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    public class CSharpFunctionInfo : ITypeToCSharp
    {
        public CSharpFunctionInfo(FunctionInstance fi, TypeDef owner, ITypeToCSharp typeToCSharp)
        {
            Function = fi;
            OwnerType = owner;
            TypeToCSharp = typeToCSharp;

            ReturnType = this.ToCSharpType(fi.ReturnType);
            ParameterTypes = Function.ParameterTypes.Select(this.ToCSharpType).ToList();
            Generics = fi.TypeVariables;
            TypeGenerics = owner?.TypeParameters.Select(tp => tp.Name).ToList() ?? [];
            RebindReceiverTypeVariables();
            AllGenerics = Generics.Concat(TypeGenerics).Distinct().ToList();
            if (ParameterNames.Count != ParameterTypes.Count)
                throw new Exception("Parameter names and types must have the same length");
        }

        public List<string> AllGenerics { get; }

        /// <summary>
        /// A library function over a GENERIC concrete type arrives with its OWN free type
        /// variable where the type's parameter belongs: <c>Count(self: Array2D&lt;$T&gt;)</c>
        /// renders as <c>Integer Count&lt;_T0&gt;</c> on <c>Array2D&lt;T&gt;</c>, which is not
        /// valid C# for a property and does not discharge <c>Countable.Count</c>. When the
        /// receiver reads exactly <c>Owner&lt;a1..an&gt;</c> and some <c>ai</c> is one of this
        /// function's own type variables, that variable IS the owner's parameter in position i,
        /// so rebind it and drop it from the member's generic list. Positions that are not the
        /// function's own variables (Map's result element) are left alone.
        /// </summary>
        private void RebindReceiverTypeVariables()
        {
            if (OwnerType == null || Generics.Count == 0 || TypeGenerics.Count == 0 || ParameterTypes.Count == 0)
                return;

            var recv = ParameterTypes[0];
            var prefix = OwnerType.Name + "<";
            if (!recv.StartsWith(prefix) || !recv.EndsWith(">"))
                return;

            var args = recv.Substring(prefix.Length, recv.Length - prefix.Length - 1).Split(',').Select(a => a.Trim()).ToList();
            if (args.Count != TypeGenerics.Count)
                return;

            var rebinds = new Dictionary<string, string>();
            for (var i = 0; i < args.Count; i++)
                if (Generics.Contains(args[i]))
                    rebinds[args[i]] = TypeGenerics[i];
            if (rebinds.Count == 0)
                return;

            string Rebind(string t)
            {
                foreach (var kv in rebinds)
                    t = System.Text.RegularExpressions.Regex.Replace(t, $@"\b{System.Text.RegularExpressions.Regex.Escape(kv.Key)}\b", kv.Value);
                return t;
            }

            ReturnType = Rebind(ReturnType);
            ParameterTypes = ParameterTypes.Select(Rebind).ToList();
            Generics = Generics.Where(g => !rebinds.ContainsKey(g)).ToList();
        }


        public ITypeToCSharp TypeToCSharp { get; }
        public FunctionInstance Function { get; }
        public string Name => Function.Name;

        public string ReturnType { get; private set; }
        public TypeDef OwnerType { get; }
        public Symbol Body => Function.Implementation.Body;
        public bool IsImplicit => Function.IsImplicitCast;
        public IReadOnlyList<string> Generics { get; private set; }
        public IReadOnlyList<string> TypeGenerics { get; }
        public int NumParameters => ParameterNames.Count;
        public IReadOnlyList<string> ParameterNames => Function.ParameterNames;
        public string FirstParameterName => ParameterNames[0];
        public IReadOnlyList<string> ParameterTypes { get; private set; }
        public IEnumerable<string> MethodParameterNames => ParameterNames.Skip(1);
        public IEnumerable<string> MethodParameterTypes => ParameterTypes.Skip(1);
        public IEnumerable<string> Parameters => ParameterNames.Zip(ParameterTypes, (n, t) => $"{t} {n}");
        public IEnumerable<string> MethodParameters => MethodParameterNames.Zip(MethodParameterTypes, (n, t) => $"{t} {n}");
        public string GenericsString => Generics.Count > 0 ? $"<{Generics.JoinStringsWithComma()}>" : "";
        public string StaticParametersString => NumParameters > 0 ? $"({Parameters.JoinStringsWithComma()})" : EmitAsMethod ? "()" : "";
        public string ExtensionParametersString => NumParameters > 0 ? $"(this {Parameters.JoinStringsWithComma()})" : "";
        public string MethodParametersString => NumParameters > 1 ? $"({MethodParameters.JoinStringsWithComma()})" : IsProperty ? "" : "()";
        public string StaticSignature => $"{FunctionAnnotation}public static {ReturnType} {Name}{GenericsString}{StaticParametersString}{Constraints}";
        
        public string ExtensionGenericsString => AllGenerics.Count > 0 ? $"<{AllGenerics.JoinStringsWithComma()}>" : "";
        public string ExtensionSignature => $"{Annotation}public static {ReturnType} {Name}{ExtensionGenericsString}{ExtensionParametersString}{Constraints}";

        // TODO: this used to be a way to signal static methods.
        public bool IsStatic => ParameterNames.Count == 0 || ParameterNames[0] == "_";

        // --no-properties: a no-arg member that would emit as a property emits as a METHOD instead,
        // unless its name is on the struct surface (a field, primitive pseudo-field, or Count — see
        // CSharpWriter.StructSurfacePropertyNames, the uniform rendering rule).
        public bool NoProperties => (TypeToCSharp as CSharpTypeWriter)?.Writer?.NoProperties ?? false;
        // Interface DECLARATIONS are always methods (struct-surface obligations are satisfied by
        // explicit interface implementations forwarding to the property/field).
        public bool EmitAsMethod => NoProperties && !IsIndexer
            && ((Function.Kind == FunctionInstanceKind.InterfaceDeclared && OwnerType == null)
                || !((TypeToCSharp as CSharpTypeWriter) is CSharpTypeWriter tw
                     && (tw.Writer?.IsStructSurfaceProperty(tw.TypeDef?.Name, Name) ?? false)));

        public string StaticKeyword => IsStatic ? "static " : "";
        // A C# property cannot be generic, so a no-arg member that still carries a type
        // variable (Array2D.Count<T>) must render as a METHOD however the recipe is configured.
        public bool IsProperty => ParameterNames.Count <= 1 && !EmitAsMethod && Generics.Count == 0;
        public static string Annotation => "[MethodImpl(AggressiveInlining)] ";
        public string FunctionAnnotation => IsProperty ? "" : Annotation;
        public string Constraints => Function.ConstrainedTypeVariables.Select(ConstraintString).JoinStrings("");
        public string MethodSignature => $"{FunctionAnnotation}public {StaticKeyword}{ReturnType} {Name}{GenericsString}{MethodParametersString}{Constraints}";
        public string StaticArgsString => NumParameters > 0 ? $"({ParameterNames.JoinStringsWithComma()})" : "";
        public string MethodArgsString => NumParameters > 1 ? $"({ParameterNames.Skip(1).JoinStringsWithComma()})" : IsProperty ? "" : "()";
        public string IntrinsicsArgsString => NumParameters > 0 ? $"({ParameterNames.Skip(1).Prepend("this").JoinStringsWithComma()})" : "";
        public bool IsMethodOf(string typeName) => ParameterTypes.Count > 0 && ParameterTypes[0] == typeName;

        public bool IsMember => OwnerType != null;

        public string OperatorName
        {
            get
            {
                var op = ParameterNames.Count == 1 ? Operators.NameToUnaryOperator(Name) :
                    ParameterNames.Count == 2 ? Operators.NameToBinaryOperator(Name) : null;
                // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                if (op == "&&") op = "&";
                if (op == "||") op = "|";
                return op;
            }
        }

        public string ConstraintString(ConstrainedTypeVariable ctv)
        {
            if (ctv?.Constraint == null) return "";
            var constraint = ctv.Constraint;
            if (constraint.Args.Count == 1 || constraint.IsSelfConstrained)
            {
                Debug.Assert(constraint.Def.IsInterface());
                return $" where {ctv.Name} : {constraint.Def.Name}<{ctv.Name}>";
            }

            if (constraint.Args.Count == 0)
            {
                return $" where {ctv.Name} : {constraint.Def.Name}";
            }

            throw new NotImplementedException();
        }

        // C# indexers cannot be generic, so a generic `At<T>(T index)` (the `Index`-constrained
        // overload in collections-indexable) has no indexer companion - emitting one produced a
        // `this[_T0 index]` referencing a type parameter that is not in scope (CS0246).
        public bool IsIndexer => Name == "At" && Generics.Count == 0;
        public string IndexerSig => $"public {ReturnType} this[{MethodParameters.JoinStringsWithComma()}]";
        public string IndexerImpl => $"{IndexerSig} {{ {Annotation} get => {Name}{MethodArgsString}; }}";
        public string IndexerInterface => $"{ReturnType} this[{MethodParameters.JoinStringsWithComma()}] {{ get; }}";

        public string ImplicitImpl => $"{Annotation} public static implicit operator {ReturnType}{StaticParametersString} => {FirstParameterName}.{Name}{MethodArgsString};";

        public bool IsOperator => OperatorName != null;
        public string OperatorImpl => $"{Annotation} public static {ReturnType} operator {OperatorName}{StaticParametersString} => {FirstParameterName}.{Name}{MethodArgsString};";

        // C# requires the comparison operators in pairs (<= with >=, < with >). When the library
        // declares only one side (e.g. a concept with just LessThanOrEquals), the missing partner
        // is synthesized by swapping the operands: a >= b  ==  b <= a.
        public static readonly Dictionary<string, string> ComparisonPartners = new Dictionary<string, string>
        {
            { "<=", ">=" }, { ">=", "<=" }, { "<", ">" }, { ">", "<" },
        };

        public string PairedOperatorImpl
            => ComparisonPartners.TryGetValue(OperatorName ?? "", out var partner) && ParameterNames.Count == 2
                ? $"{Annotation} public static {ReturnType} operator {partner}{StaticParametersString} => {ParameterNames[1]}.{Name}({ParameterNames[0]});"
                : null;

        // The Plato function name whose operator pairs with this one (null when not a comparison).
        public string PartnerFunctionName
            => ComparisonPartners.TryGetValue(OperatorName ?? "", out var partner)
                ? Operators.BinaryOperatorToName(partner)
                : null;

        public string MethodInterface => $"{ReturnType} {Name}{MethodParametersString}" + (NumParameters > 1 || EmitAsMethod ? ";" : " { get; }");

        // --static-abstract: the interface declaration for a `_`-receiver (type-level) concept
        // member. The ignored receiver disappears exactly as `this` does for an instance member,
        // so the parameters are the tail. Always METHOD form, never a property: implementors emit
        // `public static T Name(...)`, and a `static abstract` property would demand `{ get; }`
        // on both sides. Mirrors System.Numerics.INumberBase<TSelf>.
        public string StaticAbstractInterface
            => $"static abstract {ReturnType} {Name}{GenericsString}({MethodParameters.JoinStringsWithComma()});";

        // TODO: should we not be taking into account the function type replacements?  
        public TypeDef GroundingOwner => TypeToCSharp.GroundingOwner;

        public string ToCSharpTypeName(TypeInstance ti)
        {
            return TypeToCSharp.ToCSharpTypeName(ti);
        }
    }
}