using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.TypeScriptWriter
{
    /// <summary>
    /// Computes the TypeScript signatures (methods, statics, interface members,
    /// prototype function expressions) for a Plato function instance.
    /// Analog of CSharpFunctionInfo.
    ///
    /// Design rules:
    /// - No property getters are generated: single-parameter functions become
    ///   zero-argument methods (v.Length()), mirroring the move to extension
    ///   methods on the C# side. Only declared fields remain properties.
    /// - Generic parameter constraints are not emitted (structural typing).
    /// </summary>
    public class TypeScriptFunctionInfo : ITypeToTypeScript
    {
        public TypeScriptFunctionInfo(FunctionInstance fi, TypeDef owner, ITypeToTypeScript typeToTypeScript)
        {
            Function = fi;
            OwnerType = owner;
            TypeToTypeScript = typeToTypeScript;

            ReturnType = this.ToTypeScriptType(fi.ReturnType);
            ParameterTypes = Function.ParameterTypes.Select(this.ToTypeScriptType).ToList();
            Generics = fi.TypeVariables;
            TypeGenerics = owner?.TypeParameters.Select(tp => tp.Name).ToList() ?? new List<string>();
            if (ParameterNames.Count != ParameterTypes.Count)
                throw new Exception("Parameter names and types must have the same length");
        }

        public ITypeToTypeScript TypeToTypeScript { get; }
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
            => ParameterNames.Zip(ParameterTypes, (n, t) => $"{TypeScriptTypeWriter.EscapeName(n)}: {t}");

        public IEnumerable<string> MethodParameters
            => MethodParameterNames.Zip(MethodParameterTypes, (n, t) => $"{TypeScriptTypeWriter.EscapeName(n)}: {t}");

        // TODO: this used to be a way to signal static methods.
        public bool IsStatic => ParameterNames.Count == 0 || ParameterNames[0] == "_";

        public bool IsIndexer => Name == "At";
        public bool IsOperator => OperatorName != null;

        /// <summary>
        /// Method-level generic parameters (excluding any that should be omitted,
        /// e.g. because they are supplied by the containing class).
        /// </summary>
        public string GenericsString(IEnumerable<string> exclude = null)
        {
            var generics = Generics.Where(g => exclude == null || !exclude.Contains(g)).ToList();
            return generics.Count > 0 ? $"<{generics.JoinStringsWithComma()}>" : "";
        }

        /// <summary>
        /// The signature of a member function. The first parameter becomes "this";
        /// single-parameter functions are zero-argument methods (never getters).
        /// </summary>
        public string MethodSignature(IEnumerable<string> excludeGenerics = null)
        {
            var prefix = IsStatic ? "static " : "";
            return $"{prefix}{Name}{GenericsString(excludeGenerics)}({MethodParameters.JoinStringsWithComma()}): {ReturnType}";
        }

        /// <summary>
        /// The declaration of the function inside an interface (or a global
        /// interface augmentation such as "interface Number").
        /// </summary>
        public string MethodInterface(IEnumerable<string> excludeGenerics = null)
            => $"{Name}{GenericsString(excludeGenerics)}({MethodParameters.JoinStringsWithComma()}): {ReturnType};";

        /// <summary>
        /// A function expression suitable for installing on a native prototype:
        /// the receiver is declared via an explicit "this" parameter.
        /// </summary>
        public string PrototypeFunction(string nativeType, IEnumerable<string> excludeGenerics = null)
        {
            var parameters = MethodParameters.Prepend($"this: {nativeType}");
            return $"function{GenericsString(excludeGenerics)}({parameters.JoinStringsWithComma()}): {ReturnType}";
        }

        /// <summary>
        /// The signature of a constant: a static getter on the Constants class.
        /// (Constants are field-like, so they keep property syntax.)
        /// </summary>
        public string ConstantSignature => $"static get {Name}(): {ReturnType}";

        /// <summary>
        /// The signature of a module-level function (all parameters explicit).
        /// </summary>
        public string FunctionSignature(IEnumerable<string> excludeGenerics = null)
            => $"export function {Name}{GenericsString(excludeGenerics)}({Parameters.JoinStringsWithComma()}): {ReturnType}";

        public string OperatorName
        {
            get
            {
                var op = ParameterNames.Count == 1 ? Operators.NameToUnaryOperator(Name) :
                    ParameterNames.Count == 2 ? Operators.NameToBinaryOperator(Name) : null;
                return op;
            }
        }

        public string ToTypeScriptTypeName(TypeInstance ti)
            => TypeToTypeScript.ToTypeScriptTypeName(ti);
    }
}
