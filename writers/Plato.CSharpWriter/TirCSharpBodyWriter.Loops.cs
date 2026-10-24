// Emits lowered array loops, including fused producers and lambda parameter bindings.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    // --- loop lowering emission (--loops; see TirLoopLowerer) --------------------------------

    private void WriteLoweredLoop(TirLoweredLoop ll)
    {
        var k = ll.Id;
        // Prefer the lowerer's authoritative result-element type (taken from the producing
        // function's own type); fall back to the call's zonked IArray<T> argument.
        var elem = ll.ElemType?.Name != null
            ? _tw.ToCSharpType(ll.ElemType)
            : ll.Type != null && ll.Type.TypeArgs.Count == 1 ? _tw.ToCSharpType(ll.Type.TypeArgs[0]) : null;
        var srcNames = new List<string>();
        var srcCounts = new List<string>();
        for (var s = 0; s < ll.Sources.Count; s++)
        {
            var name = ll.Sources.Count == 1 ? $"_s{k}" : $"_s{k}{(char)('a' + s)}";
            srcNames.Add(name);
            srcCounts.Add($"{name}.Count");
            Write($"var {name} = ");
            WriteNode(ll.Sources[s]);
            WriteLine(";");
        }
        var n = $"_n{k}";
        var i = $"_i{k}";
        var t = ll.TempName;
        // The array an array-producing kind fills; the temp the rest of the body reads is bound to
        // it as IReadOnlyList<T> below, the type every Array<T> value has in the emitted C#, so a
        // Reduce seeded with it, a FlatMap over it, or a conditional joining it with another list
        // types as before lowering (TKT-505: a T[] temp gave CS0266 and CS8917 on the STEP mesher).
        var a = $"_a{k}";
        // The loop index as the Plato Integer wrapper: a lambda parameter bound to the bare int
        // loses the wrapper's members (`i.ToNumber` is then a method group on int, TKT-505).
        var index = $"((Integer){i})";

        void For(string count, System.Action body)
        {
            WriteLine($"for (var {i} = 0; {i} < {count}; {i}++)");
            WriteStartBlock();
            body();
            WriteEndBlock();
        }

        // The function argument: a lambda literal's body is emitted INLINE with its parameters
        // declared as loop locals; a delegate-typed reference is invoked.
        var fn = ll.Fn == null ? null : TirRewrite.StripCoerce(ll.Fn);
        var lam = fn as TirLambda;
        if (lam != null)
            foreach (var p in lam.Parameters)
                _loopParamNames[p] = $"{CSharpTypeWriter.Identifier(p.Name)}_{k}";
        void DeclareParams(params string[] elems)
        {
            if (lam == null)
                return;
            for (var p = 0; p < lam.Parameters.Count && p < elems.Length; p++)
                WriteLine($"var {_loopParamNames[lam.Parameters[p]]} = {elems[p]};");
        }
        void WriteFnValue(params string[] elems)
        {
            if (lam != null)
            {
                _lambdaDepth++;
                WriteNode(lam.Body);
                _lambdaDepth--;
            }
            else
            {
                WriteNode(fn);
                Write("(");
                Write(string.Join(", ", elems));
                Write(")");
            }
        }

        // A fused producer chain (TKT-506): the element the body sees is the base element (the
        // index itself for an IndexSource, else the source list's _i-th item) passed through each
        // stage, every step bound to a local at the top of the loop body.
        var stages = ll.SourceStages.Select(TirRewrite.StripCoerce).ToList();
        var stageLams = stages.OfType<TirLambda>().ToList();
        for (var st = 0; st < stages.Count; st++)
            if (stages[st] is TirLambda sl)
                foreach (var p in sl.Parameters)
                    _loopParamNames[p] = $"{CSharpTypeWriter.Identifier(p.Name)}_{k}s{st}";
        var count0 = ll.IndexSource ? $"System.Math.Max(0, (int){srcNames[0]})" : srcCounts[0];
        string Elem()
        {
            var prev = ll.IndexSource ? index : $"{srcNames[0]}[{i}]";
            for (var st = 0; st < stages.Count; st++)
            {
                var e = $"_e{k}_{st}";
                if (stages[st] is TirLambda sl)
                {
                    WriteLine($"var {_loopParamNames[sl.Parameters[0]]} = {prev};");
                    Write($"var {e} = ");
                    _lambdaDepth++;
                    WriteNode(sl.Body);
                    _lambdaDepth--;
                    WriteLine(";");
                }
                else
                {
                    Write($"var {e} = ");
                    WriteNode(stages[st]);
                    WriteLine($"({prev});");
                }
                prev = e;
            }
            return prev;
        }

        switch (ll.Kind)
        {
            case "Map" or "MapIdx":
                WriteLine($"var {n} = {count0};");
                WriteLine($"var {a} = new {elem}[{n}];");
                For(n, () =>
                {
                    var e = Elem();
                    var elems = ll.Kind == "MapIdx" ? new[] { e, index } : new[] { e };
                    DeclareParams(elems);
                    Write($"{a}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            case "MapRange":
                // A count of zero or less is the empty array, as the eager intrinsic and the
                // interpreter have it; `new T[n]` would throw on a negative n (TKT-505).
                WriteLine($"var {n} = System.Math.Max(0, (int){srcNames[0]});");
                WriteLine($"var {a} = new {elem}[{n}];");
                For(n, () =>
                {
                    DeclareParams(index);
                    Write($"{a}[{i}] = ");
                    WriteFnValue(index);
                    WriteLine(";");
                });
                break;
            case "Zip2":
            case "Zip3":
                Write($"var {n} = System.Math.Min({srcCounts[0]}, {srcCounts[1]});");
                WriteLine(ll.Kind == "Zip3" ? $" {n} = System.Math.Min({n}, {srcCounts[2]});" : "");
                WriteLine($"var {a} = new {elem}[{n}];");
                For(n, () =>
                {
                    var elems = srcNames.Select(sn => $"{sn}[{i}]").ToArray();
                    DeclareParams(elems);
                    Write($"{a}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            case "Reduce":
                // The accumulator carries the call's type, not the seed's: a seed that is a lowered
                // list and a step that returns IReadOnlyList<T> must agree.
                var acc = TirLoopLowerer.IsRenderableConcrete(ll.Type) ? _tw.ToCSharpType(ll.Type) : "var";
                Write($"{acc} {t} = ");
                WriteNode(ll.Seed);
                WriteLine(";");
                WriteLine($"var {n} = {count0};");
                For(n, () =>
                {
                    var e = Elem();
                    DeclareParams(t, e);
                    Write($"{t} = ");
                    WriteFnValue(t, e);
                    WriteLine(";");
                });
                break;
            case "All":
            case "Any":
                var isAll = ll.Kind == "All";
                WriteLine($"var {t} = {(isAll ? "true" : "false")};");
                WriteLine($"var {n} = {count0};");
                For(n, () =>
                {
                    var e = Elem();
                    DeclareParams(e);
                    Write(isAll ? "if (!(bool)(" : "if ((bool)(");
                    WriteFnValue(e);
                    WriteLine("))");
                    WriteStartBlock();
                    WriteLine($"{t} = {(isAll ? "false" : "true")};");
                    WriteLine("break;");
                    WriteEndBlock();
                });
                break;
            case "FlatMap":
            {
                // One growable list for the whole chain; each element's list is appended in place
                // (the intrinsic FlatMap does the same over a materialized receiver).
                var list = $"_l{k}";
                var part = $"_p{k}";
                var j = $"_j{k}";
                WriteLine($"var {list} = new System.Collections.Generic.List<{elem}>();");
                WriteLine($"var {n} = {count0};");
                For(n, () =>
                {
                    var e = Elem();
                    DeclareParams(e);
                    if (TryWriteFlatMapConditional(ll, list, part, j, elem))
                        return;
                    Write($"var {part} = ");
                    WriteFnValue(e);
                    WriteLine(";");
                    WriteLine($"for (var {j} = 0; {j} < {part}.Count; {j}++)");
                    WriteStartBlock();
                    WriteLine($"{list}.Add({part}[{j}]);");
                    WriteEndBlock();
                });
                WriteLine($"IReadOnlyList<{elem}> {t} = {list};");
                break;
            }
            case "Reverse":
                WriteLine($"var {n} = {srcCounts[0]};");
                WriteLine($"var {a} = new {elem}[{n}];");
                For(n, () => WriteLine($"{a}[{i}] = {srcNames[0]}[{n} - 1 - {i}];"));
                break;
            case "WithNext":
            {
                WriteLine($"var {n} = {srcCounts[0]};");
                Write($"var _m{k} = (bool)(");
                WriteNode(ll.IncludeFirst);
                WriteLine($") ? {n} : ({n} > 1 ? {n} - 1 : 0);");
                WriteLine($"var {a} = new {elem}[_m{k}];");
                For($"_m{k}", () =>
                {
                    DeclareParams($"{srcNames[0]}[{i}]", $"{srcNames[0]}[({i} + 1) % {n}]");
                    Write($"{a}[{i}] = ");
                    WriteFnValue($"{srcNames[0]}[{i}]", $"{srcNames[0]}[({i} + 1) % {n}]");
                    WriteLine(";");
                });
                break;
            }
            case "MapPairs":
            case "MapTriplets":
            case "MapQuartets":
            {
                var stride = ll.Kind == "MapPairs" ? 2 : ll.Kind == "MapTriplets" ? 3 : 4;
                WriteLine($"var {n} = {srcCounts[0]} / {stride};");
                WriteLine($"var {a} = new {elem}[{n}];");
                For(n, () =>
                {
                    var elems = Enumerable.Range(0, stride)
                        .Select(o => o == 0 ? $"{srcNames[0]}[{i} * {stride}]" : $"{srcNames[0]}[{i} * {stride} + {o}]")
                        .ToArray();
                    DeclareParams(elems);
                    Write($"{a}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            }
            default:
                WriteLine($"/*unknown lowered loop kind: {ll.Kind}*/");
                break;
        }
        if (TirLoopLowerer.ProducesArray(ll.Kind))
            WriteLine($"IReadOnlyList<{elem}> {t} = {a};");
        if (lam != null)
            foreach (var p in lam.Parameters)
                _loopParamNames.Remove(p);
        foreach (var sl in stageLams)
            foreach (var p in sl.Parameters)
                _loopParamNames.Remove(p);

    }
}
