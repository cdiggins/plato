// Records and formats reasons why resolved calls could not be inlined.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Why <see cref="TirInliner.TryInlineCall"/> declined to inline a resolved call. Recorded
/// per (caller, callee) into an <see cref="InlineReport"/> when <c>--inline-report</c> is on, so the
/// "why didn't X inline?" question is answerable without ad-hoc tracing (M0 of the optimizer
/// completion plan).</summary>
public enum InlineRefusal
{
    NoCalleeBody,        // call.Callee?.Body == null (unresolved / handwritten-only member)
    NoGroundTir,         // no monomorphized TIR body for the receiver's concrete type
    StatementBody,       // callee body is a statement tree (block/return/if/loop)
    ReturnTypeMismatch,  // body value type != declared return type (and not a tuple lift)
    NotSelfContained,    // bare names / eta lambdas / non-emittable member calls / nested tuples
    InsideLambda,        // call site under a lambda; callee has a lambda or non-cheap args
    ArgTypeMismatch,     // a known arg type != the callee's resolved parameter type
    LambdaArgRefused,    // a lambda argument that is not all-uses-consuming / over budget / V1 recipe
    MultiUseCompound,    // a compound arg bound to a parameter used >1 time or under a lambda
    ArityMismatch,       // parameter count != argument count
    NullaryRefused,      // a nullary (Constants.X) call that failed the static-body rules
    SelfRecursive,       // callee resolves to the function being inlined into (would never settle)
    UnknownCallable,
    MutableCapture,
    CallableSignatureMismatch,
    ExpansionBudget,
}

/// <summary>Per-generation aggregation of inliner activity: every refusal keyed by
/// (caller, callee, reason), plus success totals. Printed as a table under <c>--inline-report</c>.
/// A pure diagnostic; has no effect on emitted C#.</summary>
public sealed class InlineReport
{
    private readonly HashSet<(string Owner, object Caller, object Origin, string Outcome)> _sites = new();

    public void RecordSite(string owner, object caller, TirNode node, string outcome)
        => _sites.Add((owner, caller, node.Origin ?? (object)node, outcome));
    private readonly Dictionary<(string Caller, string Callee, InlineRefusal Reason), int> _refusals
        = new Dictionary<(string, string, InlineRefusal), int>();
    private readonly Dictionary<InlineRefusal, int> _byReason = new Dictionary<InlineRefusal, int>();

    public int CallsInlined;
    public int BetaReductions;
    public int TupleConstructorRewrites;
    public int StatementCallsInlined;
    public int KnownLambdasInlined;

    public void Refuse(string caller, string callee, InlineRefusal reason)
    {
        var key = (caller ?? "?", callee ?? "?", reason);
        _refusals.TryGetValue(key, out var c);
        _refusals[key] = c + 1;
        _byReason.TryGetValue(reason, out var r);
        _byReason[reason] = r + 1;
    }

    public string ToTable()
    {
        var sb = new StringBuilder();
        sb.AppendLine("==== inline report ====");
        sb.AppendLine($"calls inlined:            {CallsInlined}");
        sb.AppendLine($"beta reductions:          {BetaReductions}");
        sb.AppendLine($"tuple ctor rewrites:      {TupleConstructorRewrites}");
        sb.AppendLine($"statement calls inlined:  {StatementCallsInlined}");
        sb.AppendLine($"known lambdas inlined:    {KnownLambdasInlined}");
        sb.AppendLine($"distinct source sites:    {_sites.Select(s => (s.Owner, s.Caller, s.Origin)).Distinct().Count()}");
        sb.AppendLine("unique source-site outcomes (a site can have multiple outcomes):");
        foreach (var group in _sites.GroupBy(s => s.Outcome).OrderBy(g => g.Key))
            sb.AppendLine($"  {group.Key}: {group.Count()}");
        sb.AppendLine();
        sb.AppendLine("refusals by reason:");
        foreach (var kv in _byReason.OrderByDescending(k => k.Value))
            sb.AppendLine($"  {kv.Key,-20} {kv.Value}");
        sb.AppendLine();
        sb.AppendLine("refusals by (caller -> callee -> reason):");
        foreach (var kv in _refusals.OrderByDescending(k => k.Value).ThenBy(k => k.Key.Caller))
            sb.AppendLine($"  {kv.Value,4}  {kv.Key.Caller} -> {kv.Key.Callee}  [{kv.Key.Reason}]");
        return sb.ToString();
    }
}
