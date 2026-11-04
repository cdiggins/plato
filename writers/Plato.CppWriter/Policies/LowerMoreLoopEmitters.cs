// The loop kinds --lower-more adds (TKT-1186): Filter, and the grid builders Make2D, Make3D and Cartesian.
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.CppWriter.Policies
{
    public static class LowerMoreLoopEmitters
    {
        /// <summary>Adds the emitters of the four kinds; <see cref="BuiltInLoopEmitters.Create"/> calls it.</summary>
        public static LoweredLoopEmitters AddTo(LoweredLoopEmitters emitters) => emitters.Add(new FilterLoop()).Add(new GridLoop());

        // The kept elements go into a buffer sized for the whole source (every element may pass) and the result array
        // counts only the kept ones; a bump arena cannot shrink, so a sparse filter leaves the slack unused until the arena
        // rolls back. The C# loop trims with one array copy instead.
        private sealed class FilterLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Filter" };
            public void Emit(LoopEmission e)
            {
                RequireNoHoisting(e);
                var elem = e.ElementType();
                var kept = $"_m{e.Loop.Id}";
                e.ArrayCount = kept;
                e.WriteLine($"int32_t {e.Count} = {e.SourceCount};");
                e.WriteLine($"{elem}* {e.Buffer} = plato::arena_alloc<{elem}>({e.Count});");
                e.WriteLine($"int32_t {kept} = 0;");
                e.For(e.Count, () =>
                {
                    var element = e.BaseElement();
                    e.DeclareParameters(new[] { element });
                    e.Write("if (");
                    e.WriteCallback(new[] { element });
                    e.WriteLine(")");
                    e.StartBlock();
                    e.WriteLine($"{e.Buffer}[{kept}++] = {element};");
                    e.EndBlock();
                });
            }
        }

        // One element array for the whole grid, filled row-major by nested loops with the callback inline, then the
        // Array2D or Array3D record built once. A non-positive extent is the empty grid; the record keeps the extents given.
        private sealed class GridLoop : ILoweredLoopEmitter
        {
            public IReadOnlyList<string> Kinds { get; } = new[] { "Make2D", "Make3D", "Cartesian" };
            public void Emit(LoopEmission e)
            {
                RequireNoHoisting(e);
                var ll = e.Loop;
                var k = ll.Id;
                var cartesian = ll.Kind == "Cartesian";
                var three = ll.Kind == "Make3D";
                var elem = e.ElementType();
                string cols = $"_cols{k}", rows = $"_rows{k}", layers = $"_layers{k}";
                string col = $"_col{k}", row = $"_row{k}", layer = $"_layer{k}";
                e.WriteLine($"int32_t {cols} = {(cartesian ? e.SourceCountOf(0) : e.SourceNames[0])};");
                e.WriteLine($"int32_t {rows} = {(cartesian ? e.SourceCountOf(1) : e.SourceNames[1])};");
                if (three)
                    e.WriteLine($"int32_t {layers} = {e.SourceNames[2]};");
                e.WriteLine(three
                    ? $"int32_t {e.Count} = {cols} > 0 && {rows} > 0 && {layers} > 0 ? {cols} * {rows} * {layers} : 0;"
                    : $"int32_t {e.Count} = {cols} > 0 && {rows} > 0 ? {cols} * {rows} : 0;");
                e.WriteLine($"{elem}* {e.Buffer} = plato::arena_alloc<{elem}>({e.Count});");
                e.WriteLine($"int32_t {e.Index} = 0;");
                if (three)
                {
                    e.WriteLine($"for (int32_t {layer} = 0; {layer} < {layers}; {layer}++)");
                    e.StartBlock();
                }
                e.WriteLine($"for (int32_t {row} = 0; {row} < {rows}; {row}++)");
                e.StartBlock();
                e.WriteLine($"for (int32_t {col} = 0; {col} < {cols}; {col}++)");
                e.StartBlock();
                var cells = cartesian
                    ? new[] { $"{e.SourceNames[0]}.data[{col}]", $"{e.SourceNames[1]}.data[{row}]" }
                    : three ? new[] { col, row, layer } : new[] { col, row };
                e.DeclareParameters(cells);
                e.Write($"{e.Buffer}[{e.Index}++] = ");
                e.WriteCallback(cells);
                e.WriteLine(";");
                e.EndBlock();
                e.EndBlock();
                if (three)
                    e.EndBlock();
                var grid = e.Writer.CppTypeName(ll.Type);
                e.WriteLine(three
                    ? $"auto {e.Temp} = {grid}{{ plato::make_array({e.Buffer}, {e.Count}), {cols}, {rows}, {layers} }};"
                    : $"auto {e.Temp} = {grid}{{ plato::make_array({e.Buffer}, {e.Count}), {cols}, {rows} }};");
            }
        }

        // Loop-invariant callback work is hoisted by a pass the Cpu dialect does not run (hoist-invariants).
        private static void RequireNoHoisting(LoopEmission e)
        {
            if (e.Loop.Hoisted.Count > 0)
                throw new CppUnsupportedException($"loop {e.Loop.Kind} with hoisted invariants");
        }
    }
}
