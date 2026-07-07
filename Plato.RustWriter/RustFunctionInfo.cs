using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.RustWriter
{
    /// <summary>
    /// Computes the Rust signatures (methods, associated functions, trait items,
    /// free functions) for a Plato function instance.
    /// Analog of TypeScriptFunctionInfo.
    ///
    /// Design rules:
    /// - No getters are generated: single-parameter functions become
    ///   zero-argument methods (v.Length()); Rust has no properties anyway.
    /// - The receiver is "self" by value: every generated type is Copy.
    /// - Static functions (Plato first parameter "_") become associated
    ///   functions called as Type::Name(...).
    /// - Generic parameter bounds are not emitted except where hand-written
    ///   (the geometry library does not require them).
    /// </summary>
    public class RustFunctionInfo : ITypeToRust
    {
        public RustFunctionInfo(FunctionInstance fi, TypeDef owner, ITypeToRust typeToRust)
        {
            Function = fi;
            OwnerType = owner;
            TypeToRust = typeToRust;

            ReturnType = this.ToRustType(fi.ReturnType);
            ParameterTypes = Function.ParameterTypes.Select(this.ToRustType).ToList();
            Generics = fi.TypeVariables;
            TypeGenerics = owner?.TypeParameters.Select(tp => tp.Name).ToList() ?? new List<string>();
            if (ParameterNames.Count != ParameterTypes.Count)
                throw new Exception("Parameter names and types must have the same length");
        }

        public ITypeToRust TypeToRust { get; }
        public FunctionInstance Function { get; }
        public string Name => Function.Name;

        public string ReturnType { get; }
        public TypeDef OwnerType { get; }
        public Symbol Body => Function.Implementation.Body;
        public bool IsImplicit => Function.IsImplicitCast;
        public IReadOnlyList<string> Generics { get; }
        public IReadOnlyList<string> TypeGenerics { get; }
        public int NumParameters => ParameterNames.Count;
        public IReadOnlyList<string> ParameterNames => Function.ParameterNames;
        public string FirstParameterName => ParameterNames[0];
        public IReadOnlyList<string> ParameterTypes { get; }
        public IEnumerable<string> MethodParameterNames => ParameterNames.Skip(1);
        public IEnumerable<string> MethodParameterTypes => ParameterTypes.Skip(1);

        public IEnumerable<string> Parameters
            => ParameterNames.Zip(ParameterTypes, (n, t) => $"{RustTypeWriter.EscapeName(n)}: {t}");

        public IEnumerable<string> MethodParameters
            => MethodParameterNames.Zip(MethodParameterTypes, (n, t) => $"{RustTypeWriter.EscapeName(n)}: {t}");

        // The Plato convention: a first parameter named "_" marks a static function.
        public bool IsStatic => ParameterNames.Count == 0 || ParameterNames[0] == "_";

        public bool IsIndexer => Name == "At";
        public bool IsOperator => OperatorName != null;

        /// <summary>
        /// Method-level generic parameters (excluding any that should be omitted,
        /// e.g. because they are supplied by the containing impl block).
        /// </summary>
        public string GenericsString(IEnumerable<string> exclude = null)
        {
            var generics = Generics.Where(g => exclude == null || !exclude.Contains(g)).ToList();
            return generics.Count > 0 ? $"<{generics.Select(g => $"{g}: Copy").JoinStringsWithComma()}>" : "";
        }

        /// <summary>
        /// The signature of a member function inside an impl block. The first
        /// parameter becomes the "self" receiver (by value: everything is Copy);
        /// static functions become associated functions with no receiver.
        /// </summary>
        public string MethodSignature(bool isPub = true, IEnumerable<string> excludeGenerics = null)
        {
            var prefix = isPub ? "pub " : "";
            var parameters = IsStatic
                ? MethodParameters
                : MethodParameters.Prepend("self");
            return $"{prefix}fn {Name}{GenericsString(excludeGenerics)}({parameters.JoinStringsWithComma()}) -> {ReturnType}";
        }

        /// <summary>
        /// The declaration of the function inside a trait.
        /// </summary>
        public string TraitItem(IEnumerable<string> excludeGenerics = null)
            => $"fn {Name}{GenericsString(excludeGenerics)}({MethodParameters.Prepend("self").JoinStringsWithComma()}) -> {ReturnType};";

        /// <summary>
        /// The signature of a constant: a zero-argument function in the Constants module.
        /// </summary>
        public string ConstantSignature => $"pub fn {Name}() -> {ReturnType}";

        /// <summary>
        /// The signature of a module-level function (all parameters explicit).
        /// </summary>
        public string FunctionSignature(IEnumerable<string> excludeGenerics = null)
            => $"pub fn {Name}{GenericsString(excludeGenerics)}({Parameters.JoinStringsWithComma()}) -> {ReturnType}";

        public string OperatorName
        {
            get
            {
                var op = ParameterNames.Count == 1 ? Operators.NameToUnaryOperator(Name) :
                    ParameterNames.Count == 2 ? Operators.NameToBinaryOperator(Name) : null;
                return op;
            }
        }

        public string ToRustTypeName(TypeInstance ti)
            => TypeToRust.ToRustTypeName(ti);
    }
}
