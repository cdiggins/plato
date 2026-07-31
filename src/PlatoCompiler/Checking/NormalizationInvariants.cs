using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>A broken normalization invariant, with the offending symbol for diagnostics.</summary>
    public class NormalizationViolation
    {
        public string Code { get; }
        public string Message { get; }
        public Symbol Symbol { get; }

        public NormalizationViolation(string code, string message, Symbol symbol)
            => (Code, Message, Symbol) = (code, message, symbol);

        public override string ToString()
            => $"{Code}: {Message} (at {Symbol?.GetType().Name} #{Symbol?.Id})";
    }

    /// <summary>
    /// Verifies that a Symbol tree is in the canonical form guaranteed by <see cref="Normalizer"/>.
    /// The constrain pass assumes these invariants, so this verifier is the executable contract
    /// for "the normalization pass is correct": it must return zero violations on any normalized
    /// tree, and it is used by the tests to prove that over both synthetic inputs and the stdlib.
    ///
    ///   NORM1  no <see cref="Parenthesized"/> nodes remain
    ///   NORM2  no eta-expandable <see cref="FunctionGroupRefSymbol"/> remains in value position
    ///   NORM3  no single-element <see cref="MultiStatement"/> remains
    ///   NORM4  every <see cref="FunctionCall"/> callee is a callable form (group ref, lambda, or
    ///          a parameter/variable reference that may hold a function value)
    /// </summary>
    public static class NormalizationInvariants
    {
        public static IReadOnlyList<NormalizationViolation> Check(Symbol root)
        {
            var violations = new List<NormalizationViolation>();
            if (root == null)
                return violations;

            var tree = root.GetSymbolTree().ToList();

            // Callee group references (by identity) are the legitimate use of a group ref.
            var calleeGroupRefs = new HashSet<Symbol>(
                tree.OfType<FunctionCall>()
                    .Select(fc => fc.Function)
                    .OfType<FunctionGroupRefSymbol>());

            foreach (var s in tree)
            {
                switch (s)
                {
                    case Parenthesized _:
                        violations.Add(new NormalizationViolation(
                            "NORM1", "Parenthesized node was not stripped", s));
                        break;

                    case MultiStatement m when m.Symbols.Count == 1:
                        violations.Add(new NormalizationViolation(
                            "NORM3", "Single-element MultiStatement was not unwrapped", s));
                        break;

                    case FunctionGroupRefSymbol g when !calleeGroupRefs.Contains(g):
                        // Only eta-expandable groups (one shared arity >= 1) are required to be
                        // lambda-wrapped; zero-arity/mixed-arity groups are legitimately values.
                        if (IsEtaExpandable(g))
                            violations.Add(new NormalizationViolation(
                                "NORM2", $"Function-group '{g.Name}' used as a value was not eta-expanded", s));
                        break;

                    case FunctionCall fc when !IsCallable(fc.Function):
                        violations.Add(new NormalizationViolation(
                            "NORM4", $"FunctionCall callee is not a callable form: {fc.Function?.GetType().Name}", s));
                        break;
                }
            }

            return violations;
        }

        public static bool IsEtaExpandable(FunctionGroupRefSymbol g)
        {
            var fns = g.Def?.Functions;
            if (fns == null || fns.Count == 0)
                return false;
            var arity = fns[0].NumParameters;
            return arity >= 1 && fns.All(f => f.NumParameters == arity);
        }

        private static bool IsCallable(Expression callee)
            => callee is FunctionGroupRefSymbol   // ordinary named call / overload set
               || callee is Lambda                // immediately-applied lambda
               || callee is ParameterRefSymbol    // applying a function-typed parameter
               || callee is VariableRefSymbol     // applying a function-typed local
               || callee is KeywordRefSymbol;     // intrinsic keyword, e.g. `default` (nullary)
    }
}
