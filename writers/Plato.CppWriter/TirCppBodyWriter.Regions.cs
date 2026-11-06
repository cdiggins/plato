// Scalar regions (TKT-1211, --cpp-scalar-regions): a call whose result holds no arena memory runs between an arena mark and a rollback.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter;

public partial class TirCppBodyWriter
{
    // The rule, decided per call site from declared types alone:
    //   (a) the callee's result holds no arena memory (CppWriter.HoldsNoArenaMemory), so nothing it returns can point
    //       into the span rolled back;
    //   (b) the enclosing result does hold arena memory (the function's, or the innermost lambda's), so a scalar
    //       function called from a scalar function is not wrapped again: the region on the outer call already covers
    //       it, and the regions sit at the outermost scalar-returning calls of the dynamic call tree;
    //   (c) some argument holds arena memory (an array, a record of arrays, a function value): a callee handed only
    //       scalars has little to build scratch from, and the small vector and comparison functions called in every
    //       loop stay free of a mark.
    // Excluded: a call to a function value (TirInvoke), a bodiless intrinsic, a prelude array template (Count, At,
    // Reduce, Contains, ...), a zero-argument call (a constant), and every call inside a lambda that is not written
    // directly as a call's argument (a lambda bound to a name or returned may escape; one that is an argument runs
    // inside its call, on this thread). The arguments are evaluated inside the region: they feed only the call.
    private bool ScalarRegions => UsesCpuLambdas && _w.CpuOptions.ScalarRegions;

    private int _nextRegionId;

    /// <summary>Per lambda being written: whether calls inside it may be wrapped (its result holds arena memory and it is a call's argument).</summary>
    private readonly Stack<bool> _regionScopes = new Stack<bool>();

    /// <summary>The lambdas written directly as a call's argument (see <see cref="WriteArgs"/>).</summary>
    private readonly HashSet<TirLambda> _argumentLambdas = new HashSet<TirLambda>(ReferenceEqualityComparer.Instance);

    private bool EnclosingResultHoldsArenaMemory
        => _regionScopes.Count > 0 ? _regionScopes.Peek() : !_w.HoldsNoArenaMemory(_tir.ZonkedReturnType ?? _tir.ReturnType);

    private bool LambdaAdmitsRegions(TirLambda lam, TypeExpression returnType)
        => ScalarRegions && _argumentLambdas.Contains(lam) && !_w.HoldsNoArenaMemory(returnType);

    private bool WrapsInRegion(TirCall call)
    {
        if (!ScalarRegions || call.Args.Count == 0 || !EnclosingResultHoldsArenaMemory)
            return false;
        if (call.Callee?.Body == null)
            return false;
        if (!_w.HoldsNoArenaMemory(call.Type ?? call.ReturnType))
            return false;
        var argTypes = call.Args.Select(a => _w.CppTypeNameOrNull(a.Type) ?? "").ToList();
        if (CppWriter.IsPreludeArrayTemplate(call.Name, argTypes))
            return false;
        return call.Args.Any(a => !_w.HoldsNoArenaMemory(a.Type));
    }

    /// <summary>Writes the call <paramref name="writeCall"/> produces, between a mark and a rollback when the rule admits it.</summary>
    private void WriteInRegion(TirCall call, Action writeCall)
    {
        if (!WrapsInRegion(call))
        {
            writeCall();
            return;
        }
        var id = _nextRegionId++;
        Write($"[&]() {{ const auto _rm{id} = plato::arena_mark(); auto _rv{id} = ");
        writeCall();
        Write($"; plato::arena_rollback(_rm{id}); return _rv{id}; }}()");
    }
}
