// What the sum refinement pass asks of a backend: a specialized copy of a function for known cases, and a memo.
using System;
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>One thing the refinement pass knows about a pure value of a function's parameters: <see cref="Key"/> is the
/// value's shape over the parameters (<c>&lt;P0&gt;</c>, <c>&lt;P1&gt;</c>, ... as <see cref="TirSumRefinement"/> spells
/// them), <see cref="Set"/> the cases it can be, one bit per <c>SumCaseDef.Tag</c>.</summary>
public readonly record struct SumFact(string Key, ulong Set);

/// <summary>
/// A backend that can emit a clone of a function specialized to <see cref="SumFact"/>s about its parameters, under a
/// name the pass then calls. The C++ writer implements it; a backend that does not leaves <c>PassContext.Backend</c>
/// without it and the pass never clones. Calls may come from several threads at once (the Cpu prepass).
/// </summary>
public interface ISumRefinementHost
{
    /// <summary>The name to call for <paramref name="callee"/> specialized to <paramref name="facts"/> (sorted by key),
    /// emitted later from <paramref name="calleeTir"/> under <paramref name="receiverTypeName"/>; null when the backend
    /// refuses (no emission owner for the function, or the clone budget is spent).</summary>
    string RequestClone(FunctionDef callee, string receiverTypeName, TirFunction calleeTir, IReadOnlyList<SumFact> facts);

    /// <summary>A generation-wide memo for the pass's analyses (reachable match subjects per body). The compute may
    /// run more than once for one key under contention; results must be pure.</summary>
    T Memo<T>(object key, Func<T> compute);
}
