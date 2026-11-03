// Renders the Cpu dialect's lowered nodes and lambdas (TKT-1097): loops, regions, arena arrays, C++ lambdas.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CppWriter.Policies;
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

    // --- borrowed reads (CppCpuOptions.BorrowReads) ------------------------------
    //
    // Arrays are immutable views into the arena, and generated code never rolls the arena back inside a body
    // (only hosts do, between rows, after copying results out), so a reference to an element lives as long
    // as the body. A const reference to a named value is sound only when neither name is written after the
    // binding; Mutated collects every definition the body writes anywhere (assignments, inline-region results
    // and exits, paired reductions), which is stronger than "after".

    private bool BorrowReads => UsesCpuLambdas && _w.Policies.Reads.BorrowsReads;

    private HashSet<object> _mutated;

    private HashSet<object> Mutated => _mutated ??= CollectMutated(_tir?.Body);

    private static HashSet<object> CollectMutated(TirNode body)
    {
        var written = new HashSet<object>();
        if (body == null)
            return written;
        foreach (var n in body.Descendants())
        {
            switch (n)
            {
                case TirAssign a:
                    foreach (var d in a.LValue.Descendants())
                        if (d is TirVariable v) written.Add(v.Def);
                        else if (d is TirParameter p) written.Add(p.Def);
                    break;
                case TirInlineExit exit:
                    written.Add(exit.Result);
                    break;
                case TirInlineRegion region:
                    written.Add(region.Result);
                    break;
                case TirMultiReduction pair:
                    written.Add(pair.First.Def);
                    written.Add(pair.Second.Def);
                    break;
            }
        }
        return written;
    }

    /// <summary>The declaration keyword for a lowered-loop parameter bound to <paramref name="element"/>:
    /// <c>const auto&amp;</c> when it reads an array element and the parameter is never written.</summary>
    private string ElementBinding(ParameterDef parameter, string element)
        => BorrowReads
            ? _w.Policies.Reads.ElementBinding(element.EndsWith("]") && element.Contains(".data["), Mutated.Contains(parameter))
            : "auto";

    /// <summary>The declared type of a local, made a const reference when the local copies a record or String
    /// that stays put for the whole body: an array element, or a name that is never written.</summary>
    private string BorrowedLocalType(string type, VariableDef def, TirNode value)
    {
        if (!BorrowReads || type == null || def == null || Mutated.Contains(def) || value == null)
            return type;
        if (type != "String" && !_w.StructNames.Contains(type))
            return type;
        if (_w.CppTypeNameOrNull(value.Type) != type)
            return type;
        var stable = value switch
        {
            TirParameter p when !Mutated.Contains(p.Def) => true,
            TirVariable v when !Mutated.Contains(v.Def) => true,
            TirCall c when IsBorrowedArrayRead(c) => true,
            _ => false,
        };
        return _w.Policies.Reads.LocalType(type, stable);
    }

    /// <summary>A conditional whose branches are a borrowed array read (an lvalue) and a value (a prvalue) has
    /// prvalue type, and MSVC materializes the lvalue into a temporary and copies it again. Writing the borrowed
    /// branch as <c>T(read)</c> makes both branches prvalues, so the result is built in place with one copy, as
    /// before borrowing. When the other branch is also an lvalue (a borrowed read or a name) the result stays
    /// an lvalue and nothing is copied until a value is needed.</summary>
    private void WriteConditionalBranch(TirConditional cond, TirNode branch, TirNode other)
    {
        var type = BorrowReads ? _w.CppTypeNameOrNull(cond.Type) : null;
        if (type != null && (type == "String" || _w.StructNames.Contains(type))
            && branch is TirCall read && IsBorrowedArrayRead(read) && _w.CppTypeNameOrNull(read.Type) == type
            && !(other is TirCall otherRead && IsBorrowedArrayRead(otherRead))
            && other is not (TirParameter or TirVariable))
        {
            Write($"{type}(");
            WriteNode(branch);
            Write(")");
            return;
        }
        WriteNode(branch);
    }

    private bool IsBorrowedArrayRead(TirCall call)
        => call.Name == "At" && call.Args.Count == 2 && _w.CppTypeNameOrNull(call.Args[0].Type)?.StartsWith("Array<") == true;

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
        Write($"{_w.Policies.Lambdas.Capture(LambdaSite.FunctionValue)}({ps.JoinStringsWithComma()}) ");
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
        Write($"({_w.Policies.Lambdas.Capture(LambdaSite.ArrayLiteral)}() -> plato::Array<{elem}> {{ {elem}* {p} = plato::arena_alloc<{elem}>({arr.Elements.Count}); ");
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
                Write(binding.Value is TirLambda ? "auto" : BorrowedLocalType(_w.CppTypeNameOrNull(binding.Type), binding.Def, binding.Value) ?? "auto");
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
        WriteLine($"{(BorrowReads ? _w.Policies.Reads.ReductionElementBinding : "auto")} {element} = {source}.data[{index}];");
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
                WriteLine($"{ElementBinding(lam.Parameters[p], elems[p])} {_loopParamNames[lam.Parameters[p]]} = {elems[p]};");
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
                    WriteLine($"{ElementBinding(sl.Parameters[0], prev)} {_loopParamNames[sl.Parameters[0]]} = {prev};");
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

        if (!_w.Policies.Loops.TryGet(ll.Kind, out var emitter))
            throw new CppUnsupportedException($"unknown lowered loop kind '{ll.Kind}'");
        var emission = new LoopEmission
        {
            Loop = ll,
            Writer = _w,
            SourceNames = srcNames,
            Count = n,
            Index = i,
            Buffer = a,
            Temp = t,
            SourceCount = count0,
            ArrayCount = n,
            ResultArray = a,
            Write = text => Write(text),
            WriteLine = text => WriteLine(text),
            WriteNode = WriteNode,
            StartBlock = () => WriteStartBlock(),
            EndBlock = () => WriteEndBlock(),
            ElementType = Elem,
            BaseElement = BaseElem,
            For = For,
            DeclareParameters = DeclareParams,
            WriteCallback = WriteFnValue,
            ElementBinding = ElementBinding,
            ParameterName = p => _loopParamNames[p],
        };
        emitter.Emit(emission);
        if (TirLoopLowerer.ProducesArray(ll.Kind) || emission.ResultArray != a)
            WriteLine($"auto {t} = plato::make_array({emission.ResultArray}, {emission.ArrayCount});");
        if (lam != null)
            foreach (var p in lam.Parameters)
                _loopParamNames.Remove(p);
        foreach (var sl in stageLams)
            foreach (var p in sl.Parameters)
                _loopParamNames.Remove(p);
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
