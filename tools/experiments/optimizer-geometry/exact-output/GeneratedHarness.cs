using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry;

// Test inputs and measurement adapters only: every measured kernel is emitted from kernels.plato.
public static class GeometryHarness
{
    private static G3Point Point(int n) => new((Number)n, (Number)(n % 17), (Number)(-n));
    private static G3Triangle[] Triangles(int count)
        => Enumerable.Range(0, count).Select(i => new G3Triangle(Point(i), Point(i + 1), Point(i + 2))).ToArray();
    private static G3Quad[] Quads(int count)
        => Enumerable.Range(0, count).Select(i => new G3Quad(Point(i), Point(i + 1), Point(i + 2), Point(i + 3))).ToArray();
    private static G3QuadFace[] QuadFaces(int count)
        => Enumerable.Range(0, count).Select(i => new G3QuadFace(i, i + 1, i + 2, i + 3)).ToArray();
    private static G3TriangleFace[] TriangleFaces(int count)
        => Enumerable.Range(0, count).Select(i => new G3TriangleFace(i, i + 1, i + 2)).ToArray();

    private static long Consume(IReadOnlyList<G3Point> points)
        => points.Count == 0 ? 0 : points.Count + (long)(double)points[points.Count - 1].X;
    private static long Consume(IReadOnlyList<G3TriangleFace> faces)
        => faces.Count == 0 ? 0 : faces.Count + (long)faces[faces.Count - 1].C;

    public static Func<long> Create(string name, int count)
    {
        var triangles = Triangles(count);
        var quads = Quads(count);
        var quadFaces = QuadFaces(count);
        var triangleFaces = TriangleFaces(count);
        var vertices = Enumerable.Range(0, count + 3).Select(Point).ToArray();
        return name switch
        {
            "TriangleCorners" => () => Consume(GeneratedKernels.TriangleCorners(triangles)),
            "QuadCorners" => () => Consume(GeneratedKernels.QuadCorners(quads)),
            "SplitQuads" => () => Consume(GeneratedKernels.SplitQuads(quadFaces)),
            "GatherTriangles" => () => Consume(GeneratedKernels.GatherTriangles(triangleFaces, vertices)),
            "HelperCorners" => () => Consume(GeneratedKernels.HelperCorners(triangles)),
            _ => throw new ArgumentException(name)
        };
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
    }
    private static bool Equal(G3Point a, G3Point b)
        => BitConverter.DoubleToInt64Bits((double)a.X) == BitConverter.DoubleToInt64Bits((double)b.X)
            && BitConverter.DoubleToInt64Bits((double)a.Y) == BitConverter.DoubleToInt64Bits((double)b.Y)
            && BitConverter.DoubleToInt64Bits((double)a.Z) == BitConverter.DoubleToInt64Bits((double)b.Z);
    private static void EqualPoints(IReadOnlyList<G3Point> actual, IReadOnlyList<G3Point> expected)
    {
        Require(actual.Count == expected.Count, "point count");
        for (var i = 0; i < actual.Count; i++) Require(Equal(actual[i], expected[i]), $"point order/value {i}");
    }

    public static void Check()
    {
        foreach (var count in new[] { 0, 1, 7, 1003 })
        {
            var triangles = Triangles(count);
            EqualPoints(GeneratedKernels.TriangleCorners(triangles), triangles.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray());
            EqualPoints(GeneratedKernels.HelperCorners(triangles), triangles.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray());
            var quads = Quads(count);
            EqualPoints(GeneratedKernels.QuadCorners(quads), quads.SelectMany(q => new[] { q.A, q.B, q.C, q.D }).ToArray());
            var faces = QuadFaces(count);
            var split = GeneratedKernels.SplitQuads(faces);
            Require(split.Count == count * 2, "triangle count");
            for (var i = 0; i < count; i++)
            {
                Require(split[2 * i].A == faces[i].A && split[2 * i].B == faces[i].B && split[2 * i].C == faces[i].C, "first winding");
                Require(split[2 * i + 1].A == faces[i].A && split[2 * i + 1].B == faces[i].C && split[2 * i + 1].C == faces[i].D, "second winding");
            }
            var indexed = TriangleFaces(count);
            var vertices = Enumerable.Range(0, count + 3).Select(Point).ToArray();
            EqualPoints(GeneratedKernels.GatherTriangles(indexed, vertices), indexed.SelectMany(f => new[] { vertices[(int)f.A], vertices[(int)f.B], vertices[(int)f.C] }).ToArray());
        }
        var weird = new G3Point((Number)double.NaN, (Number)double.PositiveInfinity, (Number)(-0.0));
        var degenerate = new[] { new G3Triangle(weird, weird, weird), new G3Triangle(Point(0), Point(0), Point(0)) };
        EqualPoints(GeneratedKernels.TriangleCorners(degenerate), degenerate.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray());
        var repeated = GeneratedKernels.SplitQuads(new[] { new G3QuadFace(4, 4, 4, 4) });
        Require(repeated.Count == 2 && repeated.All(t => t.A == (Integer)4 && t.B == (Integer)4 && t.C == (Integer)4), "degenerate face preserved");
        foreach (var invalid in new[] { -1, 4 })
        {
            var failed = false;
            try { GeneratedKernels.GatherTriangles(new[] { new G3TriangleFace(0, invalid, 2) }, new[] { Point(0), Point(1), Point(2) }); }
            catch (ArgumentOutOfRangeException) { failed = true; }
            catch (IndexOutOfRangeException) { failed = true; }
            Require(failed, "invalid vertex fault");
        }
        var observed = new ObservedVertices();
        try
        {
            GeneratedKernels.GatherTriangles(new[] { new G3TriangleFace(0, 2, 1), new G3TriangleFace(1, 0, 0) }, observed);
            throw new Exception("missing ordered fault");
        }
        catch (InvalidOperationException)
        {
            Require(observed.Reads.SequenceEqual(new[] { 0, 2 }), "first fault/order");
        }
        Require(GeneratedKernels.GatherTriangles(Array.Empty<G3TriangleFace>(), null).Count == 0, "empty source avoids vertex read");
    }

    private sealed class ObservedVertices : IReadOnlyList<G3Point>
    {
        public readonly List<int> Reads = new();
        public int Count => 3;
        public G3Point this[int index]
        {
            get { Reads.Add(index); return index == 2 ? throw new InvalidOperationException("vertex fault") : Point(index); }
        }
        public IEnumerator<G3Point> GetEnumerator() => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
