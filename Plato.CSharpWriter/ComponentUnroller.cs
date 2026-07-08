using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter
{
    // ============================================================================================
    // Component-op unrolling / defunctionalization (docs/plato-roadmap.md Phase 3.1, --optimize).
    //
    // For IArrayLike types whose component count is statically known (Vector2/3/4/8, Point2D/3D,
    // Color, Angle, Time, ...), the generated component-wise machinery routes every operation
    // through a lambda, an IReadOnlyList wrapper (Components), and a CreateFromComponents
    // round-trip, e.g.:
    //
    //     public Vector3 Lerp(Vector3 b, Number t) => this.ZipComponents(b, (a1, b1) => a1.Lerp(b1, t));
    //
    // costing ~72 ns + 168-304 B of allocation per call (benchmark baseline 0.5). This transform
    // rewrites the *call sites* of the known HOFs at C#-emission time, beta-reducing the literal
    // lambda onto direct field accesses:
    //
    //     public Vector3 Lerp(Vector3 b, Number t) => new Vector3(this.X.Lerp(b.X, t), this.Y.Lerp(b.Y, t), this.Z.Lerp(b.Z, t));
    //
    // Implementation locus: emission-time specialization (not a Plato-level pass). Rationale:
    // the required static knowledge (concrete receiver type -> field list, incl. the primitive
    // pseudo-fields of the System.Numerics-backed types) already lives in the C# writer
    // (CSharpConcreteTypeWriter.PrimitiveFieldNames), the lambda argument is always a literal at
    // the generated-machinery call sites (core.library.plato), and working per-monomorphized-body
    // means the transform sees fully substituted parameter types. It also trivially serves both
    // --csharp-style modes, since both write bodies through CSharpFunctionBodyWriter.
    //
    // Recognized shapes (function-group name + arg count + literal lambda in last position, all
    // "vector" arguments must be plain parameter references whose monomorphized C# type is a
    // known fixed-arity IArrayLike type; anything else is LEFT UNCHANGED):
    //   MapComponents(v, f)                 -> new T(f[v.F0], f[v.F1], ...)
    //   ZipComponents(a, b[, c], f)         -> new T(f[a.F0, b.F0(, c.F0)], ...)
    //   AllZipComponents(a, b[, c], f)      -> f[a.F0, b.F0] && f[a.F1, b.F1] && ...
    //   AnyZipComponents(a, b[, c], f)      -> ... || ...
    //   AllComponents(v, p) / AnyComponent(v, p) -> p[v.F0] && / || ...
    //   Reduce(v, init, f)                  -> f[f[f[init, v.F0], v.F1], v.F2]   (left fold,
    //                                          matching IReadOnlyList.Reduce = Enumerable.Aggregate)
    //
    // Semantics preservation: the transform only removes the HOF plumbing; every scalar operation
    // (including known-buggy formulas such as MagnitudeSquared's divide-by-NumComponents, or the
    // wrong scalar FromOne) still calls the exact same scalar functions in the exact same
    // left-to-right component order, so the differential conformance suite (incl. KnownFailures)
    // must be bit-for-bit identical. All rewriting is guarded by CSharpWriter.Optimize; with the
    // flag off the writer output is byte-identical to before.
    // ============================================================================================

    /// <summary>Emission-only marker: direct component access "recv.Field", or
    /// "((CastTo)recv.Field)" when the raw field is not the Plato component type (the
    /// float-backed one-component primitives Number/Angle expose a raw "float Value" field;
    /// the cast reproduces the implicit float->Number conversion that the Components
    /// wrapper performed). Produced solely by ComponentUnroller; CSharpFunctionBodyWriter
    /// knows how to write it.</summary>
    public class ComponentAccessExpression : Expression
    {
        public Expression Receiver { get; }
        public string FieldName { get; }
        public string CastTo { get; }

        // Scalar erasure (--scalar=float) only: the primitive this component erases to when it
        // is a scalar-wrapper Plato type (either an erased struct field, or a wrapper-typed
        // pseudo-field explicitly cast down via CastTo="float"); null otherwise. Feeds
        // CSharpFunctionBodyWriter.ScalarPrimOf for method-call syntax decisions.
        public string ScalarComponentPrim { get; }

        public ComponentAccessExpression(Expression receiver, string fieldName, string castTo, string scalarComponentPrim = null)
            : base(receiver)
        {
            Receiver = receiver;
            FieldName = fieldName;
            CastTo = castTo;
            ScalarComponentPrim = scalarComponentPrim;
        }

        public override string Name => FieldName;
        public override string ToString() => $"{Receiver}.{FieldName}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new ComponentAccessExpression(Receiver?.Rewrite(f) as Expression, FieldName, CastTo, ScalarComponentPrim));
    }

    /// <summary>Emission-only marker: "new TypeName(args...)" where TypeName is a known concrete
    /// IArrayLike type (its component constructor is guaranteed by CreateFromComponents).</summary>
    public class ConstructorCallExpression : Expression
    {
        public string TypeName { get; }
        public IReadOnlyList<Expression> Args { get; }

        public ConstructorCallExpression(string typeName, params Expression[] args)
            : base(args)
        {
            TypeName = typeName;
            Args = args;
        }

        public override string Name => "new";
        public override string ToString() => $"new {TypeName}({string.Join(", ", Args)})";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new ConstructorCallExpression(TypeName, Args.Select(a => a?.Rewrite(f) as Expression).ToArray()));
    }

    /// <summary>Emission-only marker: "t0 &amp;&amp; t1 &amp;&amp; ..." (or "||"). Terms are
    /// guaranteed postfix-safe expressions (function calls / references), so no parentheses
    /// are required.</summary>
    public class BooleanChainExpression : Expression
    {
        public string Op { get; }
        public IReadOnlyList<Expression> Terms { get; }

        public BooleanChainExpression(string op, params Expression[] terms)
            : base(terms)
        {
            Op = op;
            Terms = terms;
        }

        public override string Name => Op;
        public override string ToString() => string.Join($" {Op} ", Terms);

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new BooleanChainExpression(Op, Terms.Select(t => t?.Rewrite(f) as Expression).ToArray()));
    }

    public static class ComponentUnroller
    {
        // HOF names that can possibly be unrolled; used for the cheap pre-scan so that the vast
        // majority of bodies are never rewritten (and thus provably unaffected by this pass).
        private static readonly HashSet<string> CandidateNames = new HashSet<string>
        {
            "MapComponents", "ZipComponents",
            "AllZipComponents", "AnyZipComponents",
            "AllComponents", "AnyComponent",
            "Reduce",
        };

        /// <summary>
        /// Builds the table of statically-known component fields: concrete non-generic IArrayLike
        /// types, keyed by type name. Primitive (System.Numerics-backed) types use the same
        /// pseudo-field list that generates their Components/CreateFromComponents scaffolding.
        /// </summary>
        public static Dictionary<string, IReadOnlyList<string>> BuildComponentFieldTable(Compiler.Compilation compilation)
        {
            var r = new Dictionary<string, IReadOnlyList<string>>();
            foreach (var ct in compilation.ConcreteTypes)
            {
                if (ct.TypeDef.TypeParameters.Count > 0)
                    continue;
                if (!ct.AllInterfaces.Any(i => i.Name == "IArrayLike"))
                    continue;

                var name = ct.Name;
                if (CSharpWriter.PrimitiveTypes.ContainsKey(name))
                {
                    // Mirrors CSharpConcreteTypeWriter: IArrayLike primitives must appear in
                    // PrimitiveFieldNames (the type writer throws otherwise).
                    if (CSharpConcreteTypeWriter.PrimitiveFieldNames.TryGetValue(name, out var pseudo))
                        r[name] = pseudo;
                    continue;
                }

                var fields = ct.TypeDef.Fields;
                if (fields.Count == 0)
                    continue;
                // Same constraint the type writer asserts: all components share one type.
                if (fields.Select(f => f.Type.Name).Distinct().Count() > 1)
                    continue;
                r[name] = fields.Select(f => f.Name).ToList();
            }
            return r;
        }

        /// <summary>
        /// Attempts to unroll all recognized component-HOF call sites in the body of the given
        /// function. Returns the rewritten body, or null if nothing was (safely) unrollable —
        /// in which case the caller must use the original body unchanged.
        /// </summary>
        public static Symbol TryUnroll(CSharpFunctionInfo fi, CSharpWriter writer)
        {
            if (!(fi.Body is Expression bodyExpr))
                return null;

            // Cheap pre-scan: only bodies that mention a candidate HOF with a literal lambda are
            // ever rewritten. Everything else keeps the exact original symbol tree.
            if (!bodyExpr.GetSymbolTree().OfType<FunctionCall>().Any(IsCandidateCall))
                return null;

            // Monomorphized C# type per parameter definition of this function instance.
            var paramTypes = new Dictionary<ParameterDef, string>();
            var impl = fi.Function.Implementation;
            for (var i = 0; i < impl.Parameters.Count && i < fi.ParameterTypes.Count; ++i)
                paramTypes[impl.Parameters[i]] = fi.ParameterTypes[i];

            var changed = false;
            var result = bodyExpr.Rewrite(sym =>
            {
                if (sym is FunctionCall fc)
                {
                    var u = TryUnrollCall(fc, paramTypes, writer);
                    if (u != null)
                    {
                        changed = true;
                        return u;
                    }
                }
                return sym;
            });

            if (!changed)
                return null;

            // All-or-nothing safety: if any lambda survives in the rewritten body, the
            // downstream capture rewriting (RewriteLambdasCapturingVars) would have to traverse
            // our emission-only marker nodes, which the compiler-side rewriter does not know.
            // Leave such bodies completely unchanged (conservative; coverage can grow later).
            if (result.GetSymbolTree().OfType<Lambda>().Any())
                return null;

            return result;
        }

        private static bool IsCandidateCall(FunctionCall fc)
            => fc.Function is FunctionGroupRefSymbol fgr
               && CandidateNames.Contains(fgr.Name)
               && fc.Args.Count >= 2
               && fc.Args[fc.Args.Count - 1] is Lambda;

        private static Expression TryUnrollCall(FunctionCall fc, Dictionary<ParameterDef, string> paramTypes, CSharpWriter writer)
        {
            if (!IsCandidateCall(fc))
                return null;

            var name = ((FunctionGroupRefSymbol)fc.Function).Name;
            var argc = fc.Args.Count;

            // shape: (kind, number of leading "vector" args, expected lambda arity)
            int vecArgCount;
            switch (name)
            {
                case "MapComponents" when argc == 2: vecArgCount = 1; break;
                case "ZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
                case "AllZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
                case "AnyZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
                case "AllComponents" when argc == 2: vecArgCount = 1; break;
                case "AnyComponent" when argc == 2: vecArgCount = 1; break;
                case "Reduce" when argc == 3: vecArgCount = 1; break;
                default: return null;
            }

            var lambda = (Lambda)fc.Args[argc - 1];
            var lambdaParams = lambda.Function.Parameters;
            var expectedLambdaArity = name == "Reduce" ? 2 : vecArgCount;
            if (lambdaParams.Count != expectedLambdaArity)
                return null;

            // The lambda body must be a plain expression whose root is postfix-safe (a call or a
            // reference), with no nested lambdas: after substitution it can be used verbatim as a
            // constructor argument, a &&/|| chain term, or a fold receiver, without parentheses.
            if (!(lambda.Function.Body is Expression lambdaBody))
                return null;
            if (!(lambdaBody is FunctionCall || lambdaBody is ParameterRefSymbol))
                return null;
            if (lambdaBody.GetSymbolTree().OfType<Lambda>().Any())
                return null;

            // All vector arguments must be plain parameter references (cheap + pure to duplicate
            // per component) of one and the same statically-known IArrayLike type.
            var vecArgs = new Expression[vecArgCount];
            string typeName = null;
            for (var i = 0; i < vecArgCount; ++i)
            {
                if (!(fc.Args[i] is ParameterRefSymbol pr) || !paramTypes.TryGetValue(pr.Def, out var t))
                    return null;
                if (typeName == null)
                    typeName = t;
                else if (typeName != t)
                    return null;
                vecArgs[i] = pr;
            }

            var fields = writer.GetComponentFields(typeName);
            if (fields == null || fields.Count == 0)
                return null;

            // The float-backed one-component primitives (Number, Angle) expose a raw
            // "float Value" field; accessing it directly needs an explicit cast back to the
            // Plato component type (see ComponentAccessExpression).
            var isPrimitive = CSharpWriter.PrimitiveTypes.TryGetValue(typeName, out var prim);
            var castTo = isPrimitive && prim == "float" ? "Number" : null;
            string scalarComponentPrim = null;
            if (writer.ScalarErase)
            {
                // Scalar erasure (--scalar=float): unrolled bodies live in "float-land".
                //  - Number/Angle expose a raw float Value field: use it uncast.
                //  - The other primitives' handwritten pseudo-fields (Vector3.X, Matrix4x4.M11,
                //    ...) return the WRAPPER Number, so cast them down to float.
                //  - Non-primitive IArrayLike structs have erased fields already; no cast.
                // Angle is not a scalar wrapper, but its Plato component type is Number, so its
                // components are scalar-valued in the erased output like everyone else's.
                castTo = isPrimitive && prim != "float" ? "float" : null;
                var compType = writer.GetComponentPlatoType(typeName);
                if (compType != null && CSharpWriter.ScalarPrimitives.TryGetValue(compType, out var compPrim))
                    scalarComponentPrim = compPrim;
            }

            switch (name)
            {
                case "MapComponents":
                case "ZipComponents":
                {
                    var args = fields
                        .Select(fd => Substitute(lambdaBody, lambdaParams,
                            vecArgs.Select(v => Component(v, fd, castTo, scalarComponentPrim)).ToArray()))
                        .ToArray();
                    return new ConstructorCallExpression(typeName, args);
                }

                case "AllZipComponents":
                case "AllComponents":
                case "AnyZipComponents":
                case "AnyComponent":
                {
                    var op = name.StartsWith("All") ? "&&" : "||";
                    var terms = fields
                        .Select(fd => Substitute(lambdaBody, lambdaParams,
                            vecArgs.Select(v => Component(v, fd, castTo, scalarComponentPrim)).ToArray()))
                        .ToArray();
                    return terms.Length == 1 ? terms[0] : new BooleanChainExpression(op, terms);
                }

                case "Reduce":
                {
                    // Left fold, matching IReadOnlyList.Reduce (Enumerable.Aggregate): the
                    // accumulator expression is substituted for the first lambda parameter and
                    // may appear in receiver position, so it must be postfix-safe too.
                    var init = fc.Args[1];
                    if (!(init is FunctionCall || init is ParameterRefSymbol || init is VariableRefSymbol
                          || init is FunctionGroupRefSymbol || init is Literal))
                        return null;
                    if (init.GetSymbolTree().OfType<Lambda>().Any())
                        return null;

                    var acc = init;
                    foreach (var fd in fields)
                        acc = Substitute(lambdaBody, lambdaParams, new[] { acc, Component(vecArgs[0], fd, castTo, scalarComponentPrim) });
                    return acc;
                }
            }

            return null;
        }

        private static Expression Component(Expression receiver, string fieldName, string castTo, string scalarComponentPrim)
            => new ComponentAccessExpression(receiver, fieldName, castTo, scalarComponentPrim);

        /// <summary>Beta reduction: returns a copy of the lambda body with each reference to a
        /// lambda parameter replaced by the corresponding expression. References to anything else
        /// (captured enclosing parameters, constants, ...) are left intact.</summary>
        private static Expression Substitute(Expression lambdaBody, IReadOnlyList<ParameterDef> parameters, IReadOnlyList<Expression> replacements)
            => (Expression)lambdaBody.Rewrite(sym =>
            {
                if (sym is ParameterRefSymbol pr)
                {
                    for (var i = 0; i < parameters.Count; ++i)
                        if (ReferenceEquals(pr.Def, parameters[i]))
                            return replacements[i];
                }
                return sym;
            });
    }
}
