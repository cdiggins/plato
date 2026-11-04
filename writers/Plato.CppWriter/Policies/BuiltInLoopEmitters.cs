// The lowered-loop kinds the Cpu dialect writes today, one emitter each.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;

namespace Ara3D.Geometry.CppWriter.Policies
{
    public static class BuiltInLoopEmitters
    {
        public static LoweredLoopEmitters Create() => LowerMoreLoopEmitters.AddTo(new LoweredLoopEmitters()
            .Add(new MapLoop())
            .Add(new MapRangeLoop())
            .Add(new ZipLoop())
            .Add(new ReduceLoop())
            .Add(new AllAnyLoop())
            .Add(new FlatMapLoop())
            .Add(new ReverseLoop())
            .Add(new WithNextLoop())
            .Add(new StrideLoop()));

        private sealed class MapLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Map", "MapIdx" };
            public void Emit(LoopEmission e)
            {
                e.WriteLine($"int32_t {e.Count} = {e.SourceCount};");
                e.FillArray(() =>
                {
                    var element = e.BaseElement();
                    return e.Loop.Kind == "MapIdx" ? new[] { element, e.Index } : new[] { element };
                });
            }
        }

        private sealed class MapRangeLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "MapRange" };
            public void Emit(LoopEmission e)
            {
                e.WriteLine($"int32_t {e.Count} = ({e.SourceNames[0]} > 0 ? {e.SourceNames[0]} : 0);");
                e.FillArray(() => new[] { e.Index });
            }
        }

        private sealed class ZipLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Zip2", "Zip3" };
            public void Emit(LoopEmission e)
            {
                e.WriteLine($"int32_t {e.Count} = {e.SourceCountOf(0)} < {e.SourceCountOf(1)} ? {e.SourceCountOf(0)} : {e.SourceCountOf(1)};");
                if (e.Loop.Kind == "Zip3")
                    e.WriteLine($"{e.Count} = {e.Count} < {e.SourceCountOf(2)} ? {e.Count} : {e.SourceCountOf(2)};");
                e.FillArray(() => e.SourceNames.Select(sn => $"{sn}.data[{e.Index}]").ToArray());
            }
        }

        private sealed class ReduceLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Reduce" };
            public void Emit(LoopEmission e)
            {
                var ll = e.Loop;
                var acc = TirLoopLowerer.IsRenderableConcrete(ll.Type) ? e.Writer.CppTypeNameOrNull(ll.Type) ?? "auto" : "auto";
                e.Write($"{acc} {e.Temp} = ");
                e.WriteNode(ll.Seed);
                e.WriteLine(";");
                e.WriteLine($"int32_t {e.Count} = {e.SourceCount};");
                e.For(e.Count, () =>
                {
                    var element = e.BaseElement();
                    e.DeclareParameters(new[] { e.Temp, element });
                    e.Write($"{e.Temp} = ");
                    e.WriteCallback(new[] { e.Temp, element });
                    e.WriteLine(";");
                });
            }
        }

        private sealed class AllAnyLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "All", "Any" };
            public void Emit(LoopEmission e)
            {
                var isAll = e.Loop.Kind == "All";
                e.WriteLine($"bool {e.Temp} = {(isAll ? "true" : "false")};");
                e.WriteLine($"int32_t {e.Count} = {e.SourceCount};");
                e.For(e.Count, () =>
                {
                    var element = e.BaseElement();
                    e.DeclareParameters(new[] { element });
                    e.Write(isAll ? "if (!(" : "if ((");
                    e.WriteCallback(new[] { element });
                    e.WriteLine("))");
                    e.StartBlock();
                    e.WriteLine($"{e.Temp} = {(isAll ? "false" : "true")};");
                    e.WriteLine("break;");
                    e.EndBlock();
                });
            }
        }

        // FlatMap needs the total length before it can allocate in a bump arena: with a proved fixed
        // width (exact output) the total is count * width; otherwise each part is kept and copied.
        private sealed class FlatMapLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "FlatMap" };
            public void Emit(LoopEmission e)
            {
                var ll = e.Loop;
                var elem = e.ElementType();
                var k = ll.Id;
                var n = e.Count;
                var i = e.Index;
                var j = $"_j{k}";
                var part = $"_p{k}";
                var total = $"_total{k}";
                var fa = $"_fa{k}";
                var off = $"_off{k}";
                e.WriteLine($"int32_t {n} = {e.SourceCount};");
                e.ArrayCount = total;
                e.ResultArray = fa;
                if (e.Writer.CpuOptions.ExactOutput
                    && ll.Fn is TirLambda exact && ll.SourceStages.Count == 0 && !ll.IndexSource
                    && TirExactOutput.TryGetWidth(ll, e.Writer.Compilation, out var width))
                {
                    e.WriteLine($"int32_t {total} = {n} * {width};");
                    e.WriteLine($"{elem}* {fa} = plato::arena_alloc<{elem}>({total});");
                    e.WriteLine($"int32_t {off} = 0;");
                    e.For(n, () =>
                    {
                        e.WriteLine($"{e.ElementBinding(exact.Parameters[0], $"{e.SourceNames[0]}.data[{i}]")} {e.ParameterName(exact.Parameters[0])} = {e.SourceNames[0]}.data[{i}];");
                        e.Write($"auto {part} = ");
                        e.WriteNode(exact.Body);
                        e.WriteLine(";");
                        e.WriteLine($"for (int32_t {j} = 0; {j} < {part}.count; {j}++) {fa}[{off}++] = {part}.data[{j}];");
                    });
                    return;
                }
                var parts = $"_parts{k}";
                e.WriteLine($"plato::Array<{elem}>* {parts} = plato::arena_alloc<plato::Array<{elem}>>({n});");
                e.WriteLine($"int32_t {total} = 0;");
                e.For(n, () =>
                {
                    var element = e.BaseElement();
                    e.DeclareParameters(new[] { element });
                    e.Write($"auto {part} = ");
                    e.WriteCallback(new[] { element });
                    e.WriteLine(";");
                    e.WriteLine($"{parts}[{i}] = {part};");
                    e.WriteLine($"{total} += {part}.count;");
                });
                e.WriteLine($"{elem}* {fa} = plato::arena_alloc<{elem}>({total});");
                e.WriteLine($"int32_t {off} = 0;");
                e.For(n, () =>
                    e.WriteLine($"for (int32_t {j} = 0; {j} < {parts}[{i}].count; {j}++) {fa}[{off}++] = {parts}[{i}].data[{j}];"));
            }
        }

        private sealed class ReverseLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Reverse" };
            public void Emit(LoopEmission e)
            {
                e.WriteLine($"int32_t {e.Count} = {e.SourceCountOf(0)};");
                e.WriteLine($"{e.ElementType()}* {e.Buffer} = plato::arena_alloc<{e.ElementType()}>({e.Count});");
                e.For(e.Count, () => e.WriteLine($"{e.Buffer}[{e.Index}] = {e.SourceNames[0]}.data[{e.Count} - 1 - {e.Index}];"));
            }
        }

        private sealed class WithNextLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "WithNext" };
            public void Emit(LoopEmission e)
            {
                e.WriteLine($"int32_t {e.Count} = {e.SourceCountOf(0)};");
                e.ArrayCount = $"_m{e.Loop.Id}";
                e.Write($"int32_t {e.ArrayCount} = (");
                e.WriteNode(e.Loop.IncludeFirst);
                e.WriteLine($") ? {e.Count} : ({e.Count} > 1 ? {e.Count} - 1 : 0);");
                e.FillArray(() => new[] { $"{e.SourceNames[0]}.data[{e.Index}]", $"{e.SourceNames[0]}.data[({e.Index} + 1) % {e.Count}]" });
            }
        }

        private sealed class StrideLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "MapPairs", "MapTriplets", "MapQuartets" };
            public void Emit(LoopEmission e)
            {
                var stride = e.Loop.Kind == "MapPairs" ? 2 : e.Loop.Kind == "MapTriplets" ? 3 : 4;
                e.WriteLine($"int32_t {e.Count} = {e.SourceCountOf(0)} / {stride};");
                e.FillArray(() => Enumerable.Range(0, stride)
                    .Select(o => o == 0 ? $"{e.SourceNames[0]}.data[{e.Index} * {stride}]" : $"{e.SourceNames[0]}.data[{e.Index} * {stride} + {o}]")
                    .ToArray());
            }
        }
    }
}
