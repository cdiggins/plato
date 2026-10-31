// --switch-match (TKT-1148): a match on a closed sum is a chain of tag tests after elaboration
// (`s.IsA() ? x : s.IsB() ? y : ... : z`). Reaching the nth case costs n tests. With the switch on,
// a chain of at least MinTests tests on one subject is written as a C# switch expression on the
// tag, `s.Kind switch { T.Kind_A => x, T.Kind_B => y, _ => z }`, which the C# compiler emits as an
// IL switch and the JIT turns into a jump table. The subject is written once, so it is also
// evaluated once. The default output is unchanged.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    // Two tests are one ternary away from a switch and the JIT does not make a table of fewer than
    // four labels; three tests plus the default are the shortest chain worth rewriting.
    private const int MinSwitchTests = 3;

    private bool SwitchMatch => _tw.Writer.SwitchMatch;

    /// <summary>Writes the chain as a switch expression when it qualifies. <paramref name="writeArm"/>
    /// writes a result node the way the caller writes its conditional's branches.</summary>
    private bool TryWriteTagSwitch(TirConditional cond, Action<TirNode> writeArm)
    {
        if (!SwitchMatch || !TryCollectTagChain(cond, out var subject, out var sum, out var arms, out var tail))
            return false;
        var generic = sum.TypeParameters.Count > 0;
        var typeName = generic ? null : _tw.ToCSharpType(subject.Type);
        Write("(");
        WritePostfixReceiver(subject);
        Write(".Kind switch { ");
        foreach (var (caseDef, body) in arms)
        {
            Write(generic ? caseDef.Tag.ToString() : $"{typeName}.{caseDef.TagConstName}");
            Write(" => ");
            writeArm(body);
            Write(", ");
        }
        Write("_ => ");
        writeArm(tail);
        Write(" })");
        return true;
    }

    private static bool TryCollectTagChain(TirConditional cond, out TirNode subject, out TypeDef sum,
        out List<(SumCaseDef Case, TirNode Body)> arms, out TirNode tail)
    {
        subject = null;
        sum = null;
        arms = new List<(SumCaseDef, TirNode)>();
        tail = null;
        TirNode node = cond;
        while (node is TirConditional c && TagTest(c.Condition, out var testSubject, out var testSum, out var caseDef))
        {
            if (subject == null)
            {
                subject = testSubject;
                sum = testSum;
            }
            else if (!ReferenceEquals(sum, testSum) || !SameSubject(subject, testSubject))
                break;
            if (arms.Any(a => a.Case == caseDef))
                break;
            arms.Add((caseDef, c.IfTrue));
            node = c.IfFalse;
        }
        tail = node;
        return arms.Count >= MinSwitchTests && tail != null && arms.All(a => a.Body != null);
    }

    private static bool TagTest(TirNode test, out TirNode subject, out TypeDef sum, out SumCaseDef caseDef)
    {
        subject = null;
        sum = null;
        caseDef = null;
        if (TirRewrite.StripCoerce(test) is not TirCall { Callee: null, EmissionKind: EmissionKind.Property, Args.Count: 1 } call)
            return false;
        subject = call.Args[0];
        sum = TirRewrite.StripCoerce(subject)?.Type?.Def;
        if (sum == null || !sum.IsSum)
            return false;
        caseDef = sum.Cases.FirstOrDefault(c => c.PredicateName == call.Name);
        return caseDef != null;
    }

    /// <summary>The same value: one node, or equal trees of parameters, variables, literals and calls
    /// (Plato calls are pure, so a repeated call is the same value).</summary>
    private static bool SameSubject(TirNode a, TirNode b)
    {
        if (ReferenceEquals(a, b))
            return true;
        switch (a, b)
        {
            case (TirParameter p, TirParameter q):
                return p.Def != null && ReferenceEquals(p.Def, q.Def);
            case (TirVariable p, TirVariable q):
                return p.Def != null && ReferenceEquals(p.Def, q.Def);
            case (TirCoerce p, TirCoerce q):
                return Equals(p.ToType?.ToString(), q.ToType?.ToString()) && SameSubject(p.Inner, q.Inner);
            case (TirTypeRef p, TirTypeRef q):
                return p.TypeDef != null && ReferenceEquals(p.TypeDef, q.TypeDef);
            case (TirLiteral p, TirLiteral q):
                return p.LiteralType == q.LiteralType && Equals(p.Value, q.Value);
            case (TirCall p, TirCall q):
                return ReferenceEquals(p.Callee, q.Callee) && p.Name == q.Name && p.EmissionKind == q.EmissionKind
                    && p.Args.Count == q.Args.Count
                    && p.Args.Zip(q.Args, (x, y) => x != null && y != null && SameSubject(x, y)).All(t => t);
            default:
                return false;
        }
    }
}
