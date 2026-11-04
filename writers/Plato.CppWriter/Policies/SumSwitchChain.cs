// Reads a chain of case predicates on one sum value (what a Plato match lowers to) as the arms of a switch.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>One arm of a <see cref="SumSwitchChain"/>: the case and the expression chosen when the value is that case.</summary>
    public sealed record SumSwitchArm(SumCaseDef Case, TirNode Value);

    /// <summary>
    /// <c>IsA(s) ? x : IsB(s) ? y : z</c> where <c>s</c> is one parameter or variable of a sum type and each <c>Is</c> call
    /// is that type's own predicate: the arms in order and the expression for every other case.
    /// </summary>
    public sealed class SumSwitchChain
    {
        /// <summary>A switch pays off from two tested cases and a default; one test stays a conditional.</summary>
        public const int MinimumArms = 2;

        public string TypeName { get; }
        public TirNode Subject { get; }
        public IReadOnlyList<SumSwitchArm> Arms { get; }
        public TirNode Default { get; }

        private SumSwitchChain(string typeName, TirNode subject, IReadOnlyList<SumSwitchArm> arms, TirNode fallback)
            => (TypeName, Subject, Arms, Default) = (typeName, subject, arms, fallback);

        /// <summary>The chain <paramref name="value"/> spells, or null. <paramref name="casesOf"/> gives a sum type's cases by
        /// Plato type name (null for any other type); <paramref name="strip"/> removes coercions around a receiver.</summary>
        public static SumSwitchChain TryRead(TirNode value, Func<string, IReadOnlyList<SumCaseDef>> casesOf,
            Func<TirNode, string> typeNameOf, Func<TirNode, TirNode> strip)
        {
            var arms = new List<SumSwitchArm>();
            object subjectDef = null;
            TirNode subject = null;
            string typeName = null;
            IReadOnlyList<SumCaseDef> cases = null;
            var current = value;
            while (current is TirConditional c && c.Condition is TirCall call && call.Args.Count == 1)
            {
                var receiver = strip(call.Args[0]);
                var def = receiver is TirParameter p ? (object)p.Def : receiver is TirVariable v ? v.Def : null;
                if (def == null || (subjectDef != null && !ReferenceEquals(def, subjectDef)))
                    break;
                if (subjectDef == null)
                {
                    typeName = typeNameOf(receiver);
                    cases = typeName == null ? null : casesOf(typeName);
                    if (cases == null)
                        break;
                }
                var tested = cases.FirstOrDefault(k => k.PredicateName == call.Name);
                if (tested == null || arms.Any(a => a.Case == tested))
                    break;
                subjectDef = def;
                subject = receiver;
                arms.Add(new SumSwitchArm(tested, c.IfTrue));
                current = c.IfFalse;
            }
            return arms.Count >= MinimumArms ? new SumSwitchChain(typeName, subject, arms, current) : null;
        }
    }
}
