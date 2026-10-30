// Renders the Cpu dialect's lowered nodes and lambdas (TKT-1097): loops, regions, arena arrays, C++ lambdas.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter;

public partial class TirCppBodyWriter
{
    // True in the Cpu dialect: function values are C++ lambdas, not functor structs.
    private bool UsesCpuLambdas => CppTirPipeline.IsCpu(_w);

    // Loop and fused-stage lambda parameters rendered under a loop-local name.
    private readonly Dictionary<ParameterDef, string> _loopParamNames = new();

    // The return type of each enclosing C++ lambda; a null Cpp means it is deduced.
    private readonly Stack<(string Cpp, string Plato)> _lambdaReturns = new();

    private void WriteLambdaReturnValue(TirNode value)
    {
        var (cpp, plato) = _lambdaReturns.Peek();
        if (cpp == null)
            WriteNode(value);
        else
            WriteConvertedTo(value, cpp, plato);
    }

    // --- lambdas -------------------------------------------------------------

    private TypeExpression LambdaReturnType(TirLambda lam)
    {
        var fn = lam.Type;
        if (fn?.Def?.Name != null && fn.Def.Name.StartsWith("Function") && fn.TypeArgs != null && fn.TypeArgs.Count > 0)
        {
            var last = fn.TypeArgs[fn.TypeArgs.Count - 1];
            if (_w.CppTypeNameOrNull(last) != null)
                return last;
        }
        return lam.Body?.Type;
    }

    private void WriteCpuLambda(TirLambda lam)
    {
        var ps = lam.Parameters.Select(p => $"{ResolveLambdaParamCpp(lam, p) ?? "auto"} {CppWriter.EscapeName(p.Name)}");
        var retType = LambdaReturnType(lam);
        var ret = _w.CppTypeNameOrNull(retType);
        Write($"[=]({ps.JoinStringsWithComma()}) ");
        if (ret != null)
            Write($"-> {ret} ");
        _lambdaReturns.Push((ret, ret == null ? null : CppWriter.PlatoTypeName(retType)));
        if (IsStatementNode(lam.Body))
        {
            WriteLine();
            WriteStatement(Blockify(lam.Body));
        }
        else
        {
            Write("{ return ");
            WriteLambdaReturnValue(lam.Body);
            Write("; }");
        }
        _lambdaReturns.Pop();
    }

    // --- array literals ------------------------------------------------------

    private int _nextArrayId;

    private void WriteCpuArray(TirArray arr)
    {
        var elemType = arr.Type?.TypeArgs != null && arr.Type.TypeArgs.Count == 1 ? arr.Type.TypeArgs[0] : null;
        var elem = _w.CppTypeNameOrNull(elemType)
                   ?? (arr.Elements.Count > 0 ? _w.CppTypeNameOrNull(arr.Elements[0].Type) : null);
        if (elem == null)
            throw new CppUnsupportedException("array literal without a representable element type");
        if (arr.Elements.Count == 0)
        {
            Write($"plato::Array<{elem}>{{ nullptr, 0 }}");
            return;
        }
        var p = $"_lit{_nextArrayId++}";
        var elemPlato = elemType != null ? CppWriter.PlatoTypeName(elemType) : null;
        Write($"([&]() -> plato::Array<{elem}> {{ {elem}* {p} = plato::arena_alloc<{elem}>({arr.Elements.Count}); ");
        for (var i = 0; i < arr.Elements.Count; i++)
        {
            Write($"{p}[{i}] = ");
            WriteConvertedTo(arr.Elements[i], elem, elemPlato);
            Write("; ");
        }
        Write($"return plato::make_array({p}, {arr.Elements.Count}); }}())");
    }

    // --- statements ----------------------------------------------------------

    private void WriteCpuStatement(TirNode n)
    {
        switch (n)
        {
            case TirInlineRegion region:
                // The body is braced so the exit goto leaves its scope instead of jumping over initializations.
                WriteLine($"{_w.CppTypeName(region.Type)} {region.Result.Name}{{}};");
                WriteStatement(Blockify(region.Body));
                WriteLine($"{region.ExitLabel}: ;");
                return;
            case TirInlineExit exit:
                Write($"{exit.Result.Name} = ");
                WriteNode(exit.Value);
                WriteLine($"; goto {exit.Result.Name}_exit;");
                return;
            case TirInlineBinding binding:
                Write(binding.Value is TirLambda ? "auto" : _w.CppTypeNameOrNull(binding.Type) ?? "auto");
                Write($" {CppWriter.EscapeName(binding.Def.Name)} = ");
                WriteNode(binding.Value);
                WriteLine(";");
                return;
            case TirGuardedLoop loop:
                WriteLine("while (true)");
                WriteStartBlock();
                foreach (var statement in loop.Test.Statements)
                    WriteCpuStatementOrExpression(statement);
                Write("if (!(");
                WriteNode(loop.Test.Value);
                WriteLine(")) break;");
                WriteStatement(Blockify(loop.Body));
                WriteEndBlock();
                return;
            case TirLoweredLoop ll:
                WriteLoweredLoop(ll);
                return;
            case TirMultiReduction pair:
                WriteMultiReduction(pair);
                return;
        }
    }

    private void WriteCpuStatementOrExpression(TirNode s)
    {
        if (s == null)
            return;
        if (IsStatementNode(s))
            WriteStatement(s);
        else
        {
            WriteNode(s);
            WriteLine(";");
        }
    }

    private void WriteMultiReduction(TirMultiReduction pair)
    {
        var prefix = $"_mc{pair.Id}_";
        var source = prefix + "source";
        var index = prefix + "index";
        var element = prefix + "element";
        Write($"auto {source} = ");
        WriteNode(pair.Source);
        WriteLine(";");
        foreach (var binding in new[] { pair.First, pair.Second })
        {
            Write($"{_w.CppTypeName(binding.Value.Type)} {CppWriter.EscapeName(binding.Def.Name)} = ");
            WriteNode(((TirCall)binding.Value).Args[1]);
            WriteLine(";");
        }
        WriteLine($"for (int32_t {index} = 0; {index} < {source}.count; {index}++)");
        WriteStartBlock();
        WriteLine($"auto {element} = {source}.data[{index}];");
        WriteReductionStep(pair.First, element);
        WriteReductionStep(pair.Second, element);
        WriteEndBlock();
    }

    private void WriteReductionStep(TirLet binding, string element)
    {
        var lambda = (TirLambda)((TirCall)binding.Value).Args[2];
        var accumulator = CppWriter.EscapeName(binding.Def.Name);
        var saved = new Dictionary<ParameterDef, string>();
        foreach (var parameter in lambda.Parameters)
            if (_loopParamNames.TryGetValue(parameter, out var previous))
                saved.Add(parameter, previous);
        _loopParamNames[lambda.Parameters[0]] = accumulator;
        _loopParamNames[lambda.Parameters[1]] = element;
        Write($"{accumulator} = ");
        WriteNode(lambda.Body);
        WriteLine(";");
        foreach (var parameter in lambda.Parameters)
            if (saved.TryGetValue(parameter, out var previous))
                _loopParamNames[parameter] = previous;
            else
                _loopParamNames.Remove(parameter);
    }

    // --- lowered loops -------------------------------------------------------

    private void WriteCallbackResult(TirLambda lambda, VariableDef result)
    {
        if (IsStatementNode(lambda.Body))
            WriteStatement(new TirInlineRegion(result, TirInlineCloner.RedirectReturns(lambda.Body, result)));
        else
        {
            Write($"{_w.CppTypeNameOrNull(result.Type) ?? "auto"} {result.Name} = ");
            WriteNode(lambda.Body);
            WriteLine(";");
        }
    }

    private void WriteLoweredLoop(TirLoweredLoop ll)
    {
        var k = ll.Id;
        var elemType = ll.ElemType?.Name != null ? ll.ElemType
            : ll.Type != null && ll.Type.TypeArgs.Count == 1 ? ll.Type.TypeArgs[0] : null;
        var elem = elemType == null ? null : _w.CppTypeNameOrNull(elemType);
        string Elem() => elem ?? throw new CppUnsupportedException($"loop {ll.Kind} without a representable element type");

        var srcNames = new List<string>();
        for (var s = 0; s < ll.Sources.Count; s++)
        {
            var name = ll.Sources.Count == 1 ? $"_s{k}" : $"_s{k}{(char)('a' + s)}";
            srcNames.Add(name);
            Write($"auto {name} = ");
            WriteNode(ll.Sources[s]);
            WriteLine(";");
        }
        string Count(int s) => $"{srcNames[s]}.count";
        var n = $"_n{k}";
        var i = $"_i{k}";
        var a = $"_a{k}";
        var t = ll.TempName;
        var arrayCount = n;

        void For(string count, System.Action body)
        {
            WriteLine($"for (int32_t {i} = 0; {i} < {count}; {i}++)");
            WriteStartBlock();
            body();
            WriteEndBlock();
        }

        var fn = ll.Fn == null ? null : TirRewrite.StripCoerce(ll.Fn);
        var lam = fn as TirLambda;
        if (lam != null)
            foreach (var p in lam.Parameters)
                _loopParamNames[p] = $"{CppWriter.EscapeName(p.Name)}_{k}";
        void DeclareParams(params string[] elems)
        {
            if (lam == null)
                return;
            for (var p = 0; p < lam.Parameters.Count && p < elems.Length; p++)
                WriteLine($"auto {_loopParamNames[lam.Parameters[p]]} = {elems[p]};");
            if (IsStatementNode(lam.Body))
                WriteCallbackResult(lam, new VariableDef(null, $"_callback{k}", lam.Type.TypeArgs.Last(), null));
        }
        void WriteFnValue(params string[] elems)
        {
            if (lam != null)
            {
                if (IsStatementNode(lam.Body))
                    Write($"_callback{k}");
                else
                    WriteNode(lam.Body);
            }
            else
            {
                Write("(");
                WriteNode(fn);
                Write(")(");
                Write(string.Join(", ", elems));
                Write(")");
            }
        }

        var stages = ll.SourceStages.Select(TirRewrite.StripCoerce).ToList();
        var stageLams = stages.OfType<TirLambda>().ToList();
        for (var st = 0; st < stages.Count; st++)
            if (stages[st] is TirLambda sl)
                foreach (var p in sl.Parameters)
                    _loopParamNames[p] = $"{CppWriter.EscapeName(p.Name)}_{k}s{st}";
        var count0 = ll.IndexSource ? $"({srcNames[0]} > 0 ? {srcNames[0]} : 0)" : Count(0);
        string BaseElem()
        {
            var prev = ll.IndexSource ? i : $"{srcNames[0]}.data[{i}]";
            for (var st = 0; st < stages.Count; st++)
            {
                var e = $"_e{k}_{st}";
                if (stages[st] is TirLambda sl)
                {
                    WriteLine($"auto {_loopParamNames[sl.Parameters[0]]} = {prev};");
                    WriteCallbackResult(sl, new VariableDef(null, e, sl.Type.TypeArgs.Last(), null));
                }
                else
                {
                    Write($"auto {e} = (");
                    WriteNode(stages[st]);
                    WriteLine($")({prev});");
                }
                prev = e;
            }
            return prev;
        }

        void FillArray(System.Func<string[]> elems)
        {
            WriteLine($"{Elem()}* {a} = plato::arena_alloc<{Elem()}>({arrayCount});");
            For(arrayCount, () =>
            {
                var es = elems();
                DeclareParams(es);
                Write($"{a}[{i}] = ");
                WriteFnValue(es);
                WriteLine(";");
            });
        }

        switch (ll.Kind)
        {
            case "Map" or "MapIdx":
                WriteLine($"int32_t {n} = {count0};");
                FillArray(() =>
                {
                    var e = BaseElem();
                    return ll.Kind == "MapIdx" ? new[] { e, i } : new[] { e };
                });
                break;
            case "MapRange":
                WriteLine($"int32_t {n} = ({srcNames[0]} > 0 ? {srcNames[0]} : 0);");
                FillArray(() => new[] { i });
                break;
            case "Zip2":
            case "Zip3":
                WriteLine($"int32_t {n} = {Count(0)} < {Count(1)} ? {Count(0)} : {Count(1)};");
                if (ll.Kind == "Zip3")
                    WriteLine($"{n} = {n} < {Count(2)} ? {n} : {Count(2)};");
                FillArray(() => srcNames.Select(sn => $"{sn}.data[{i}]").ToArray());
                break;
            case "Reduce":
            {
                var acc = TirLoopLowerer.IsRenderableConcrete(ll.Type) ? _w.CppTypeNameOrNull(ll.Type) ?? "auto" : "auto";
                Write($"{acc} {t} = ");
                WriteNode(ll.Seed);
                WriteLine(";");
                WriteLine($"int32_t {n} = {count0};");
                For(n, () =>
                {
                    var e = BaseElem();
                    DeclareParams(t, e);
                    Write($"{t} = ");
                    WriteFnValue(t, e);
                    WriteLine(";");
                });
                break;
            }
            case "All":
            case "Any":
            {
                var isAll = ll.Kind == "All";
                WriteLine($"bool {t} = {(isAll ? "true" : "false")};");
                WriteLine($"int32_t {n} = {count0};");
                For(n, () =>
                {
                    var e = BaseElem();
                    DeclareParams(e);
                    Write(isAll ? "if (!(" : "if ((");
                    WriteFnValue(e);
                    WriteLine("))");
                    WriteStartBlock();
                    WriteLine($"{t} = {(isAll ? "false" : "true")};");
                    WriteLine("break;");
                    WriteEndBlock();
                });
                break;
            }
            case "FlatMap":
                WriteCpuFlatMap(ll, srcNames, Elem(), count0, BaseElem, DeclareParams, WriteFnValue, For);
                arrayCount = $"_total{k}";
                break;
            case "Reverse":
                WriteLine($"int32_t {n} = {Count(0)};");
                WriteLine($"{Elem()}* {a} = plato::arena_alloc<{Elem()}>({n});");
                For(n, () => WriteLine($"{a}[{i}] = {srcNames[0]}.data[{n} - 1 - {i}];"));
                break;
            case "WithNext":
            {
                WriteLine($"int32_t {n} = {Count(0)};");
                arrayCount = $"_m{k}";
                Write($"int32_t {arrayCount} = (");
                WriteNode(ll.IncludeFirst);
                WriteLine($") ? {n} : ({n} > 1 ? {n} - 1 : 0);");
                FillArray(() => new[] { $"{srcNames[0]}.data[{i}]", $"{srcNames[0]}.data[({i} + 1) % {n}]" });
                break;
            }
            case "MapPairs":
            case "MapTriplets":
            case "MapQuartets":
            {
                var stride = ll.Kind == "MapPairs" ? 2 : ll.Kind == "MapTriplets" ? 3 : 4;
                WriteLine($"int32_t {n} = {Count(0)} / {stride};");
                FillArray(() => Enumerable.Range(0, stride)
                    .Select(o => o == 0 ? $"{srcNames[0]}.data[{i} * {stride}]" : $"{srcNames[0]}.data[{i} * {stride} + {o}]")
                    .ToArray());
                break;
            }
            default:
                throw new CppUnsupportedException($"unknown lowered loop kind '{ll.Kind}'");
        }
        if (TirLoopLowerer.ProducesArray(ll.Kind) || ll.Kind == "FlatMap")
            WriteLine($"auto {t} = plato::make_array({(ll.Kind == "FlatMap" ? $"_fa{k}" : a)}, {arrayCount});");
        if (lam != null)
            foreach (var p in lam.Parameters)
                _loopParamNames.Remove(p);
        foreach (var sl in stageLams)
            foreach (var p in sl.Parameters)
                _loopParamNames.Remove(p);
    }

    // FlatMap needs the total length before it can allocate in a bump arena: with a proved fixed
    // width (exact output) the total is count * width; otherwise each part is kept and copied.
    private void WriteCpuFlatMap(TirLoweredLoop ll, List<string> srcNames, string elem, string count0,
        System.Func<string> baseElem, System.Action<string[]> declareParams,
        System.Action<string[]> writeFnValue, System.Action<string, System.Action> forLoop)
    {
        var k = ll.Id;
        var n = $"_n{k}";
        var i = $"_i{k}";
        var j = $"_j{k}";
        var part = $"_p{k}";
        var total = $"_total{k}";
        var fa = $"_fa{k}";
        var off = $"_off{k}";
        WriteLine($"int32_t {n} = {count0};");
        if (_w.CpuOptions.ExactOutput
            && ll.Fn is TirLambda exact && ll.SourceStages.Count == 0 && !ll.IndexSource
            && TirExactOutput.TryGetWidth(ll, _w.Compilation, out var width))
        {
            WriteLine($"int32_t {total} = {n} * {width};");
            WriteLine($"{elem}* {fa} = plato::arena_alloc<{elem}>({total});");
            WriteLine($"int32_t {off} = 0;");
            forLoop(n, () =>
            {
                WriteLine($"auto {_loopParamNames[exact.Parameters[0]]} = {srcNames[0]}.data[{i}];");
                Write($"auto {part} = ");
                WriteNode(exact.Body);
                WriteLine(";");
                WriteLine($"for (int32_t {j} = 0; {j} < {part}.count; {j}++) {fa}[{off}++] = {part}.data[{j}];");
            });
            return;
        }
        var parts = $"_parts{k}";
        WriteLine($"plato::Array<{elem}>* {parts} = plato::arena_alloc<plato::Array<{elem}>>({n});");
        WriteLine($"int32_t {total} = 0;");
        forLoop(n, () =>
        {
            var e = baseElem();
            declareParams(new[] { e });
            Write($"auto {part} = ");
            writeFnValue(new[] { e });
            WriteLine(";");
            WriteLine($"{parts}[{i}] = {part};");
            WriteLine($"{total} += {part}.count;");
        });
        WriteLine($"{elem}* {fa} = plato::arena_alloc<{elem}>({total});");
        WriteLine($"int32_t {off} = 0;");
        forLoop(n, () =>
            WriteLine($"for (int32_t {j} = 0; {j} < {parts}[{i}].count; {j}++) {fa}[{off}++] = {parts}[{i}].data[{j}];"));
    }
}

public partial class TirCppBodyWriter
{
    // Scalar and native-primitive names follow the dialect's profile (double / int32_t in Cpu).
    private bool IsScalarT(string cpp) => UsesCpuLambdas ? _w.IsScalarCpp(cpp) : CppWriter.IsScalar(cpp);

    private bool TryNativePrimitive(string platoName, out string cpp)
    {
        if (!CppWriter.NativePrimitives.TryGetValue(platoName, out cpp))
            return false;
        if (UsesCpuLambdas)
            cpp = platoName == "Number" ? _w.NumberCpp : platoName == "Integer" ? _w.IntegerCpp : cpp;
        return true;
    }
}
