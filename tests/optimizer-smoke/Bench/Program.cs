extern alias opt;
extern alias unopt;

using System;
using System.Diagnostics;

using OTri = opt::Ara3D.Geometry.Triangle3D;
using OPt = opt::Ara3D.Geometry.Point3D;
using OXf = opt::Ara3D.Geometry.Transform3D;
using OMesh = opt::Ara3D.Geometry.TriangleMesh3D;
using OInt3 = opt::Ara3D.Geometry.Integer3;
using UTri = unopt::Ara3D.Geometry.Triangle3D;
using UPt = unopt::Ara3D.Geometry.Point3D;
using UXf = unopt::Ara3D.Geometry.Transform3D;
using UMesh = unopt::Ara3D.Geometry.TriangleMesh3D;
using UInt3 = unopt::Ara3D.Geometry.Integer3;

// M2 micro-benchmark: Triangle3D.Transform, the flagship collapse. The optimized library emits a
// straight-line `new Triangle3D(new Vector3(...).Transform(m), ...)`; the unoptimized library emits
// `this.Deform((p) => ...)` — a Func<Point3D,Point3D> delegate allocation plus three virtual
// .Invoke calls per triangle. Same arithmetic, same result; the delta is the delegate indirection.

const int Iterations = 50_000_000;
const int Warmup = 3;
const int Trials = 5;

static double BenchOpt()
{
    var t = OXf.Identity();
    var tri = new OTri(new OPt(1f, 2f, 3f), new OPt(4f, 5f, 6f), new OPt(7f, 8f, 9f));
    float acc = 0f;
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < Iterations; i++)
    {
        var r = tri.Transform(t);
        acc += r.A.X + r.B.Y + r.C.Z;
    }
    sw.Stop();
    if (acc == float.NegativeInfinity) Console.WriteLine("unreachable");
    return sw.Elapsed.TotalMilliseconds;
}

static double BenchUnopt()
{
    var t = UXf.Identity();
    var tri = new UTri(new UPt(1f, 2f, 3f), new UPt(4f, 5f, 6f), new UPt(7f, 8f, 9f));
    float acc = 0f;
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < Iterations; i++)
    {
        var r = tri.Transform(t);
        acc += r.A.X + r.B.Y + r.C.Z;
    }
    sw.Stop();
    if (acc == float.NegativeInfinity) Console.WriteLine("unreachable");
    return sw.Elapsed.TotalMilliseconds;
}

// Allocation-heavy path: TriangleMesh3D.Transform over a large point buffer. The optimized library
// emits a single loop filling a Point3D[] then `new TriangleMesh3D(buf, this.FaceIndices)`; the
// unoptimized library goes through Deform -> points.Map(delegate), a per-element Func invocation
// plus the combinator's own materialization.
const int MeshPoints = 200_000;
const int MeshReps = 200;

static double BenchOptMesh()
{
    var pts = new OPt[MeshPoints];
    for (var i = 0; i < MeshPoints; i++) pts[i] = new OPt(i, i * 0.5f, i * 0.25f);
    var mesh = new OMesh(pts, new OInt3[0]);
    var t = OXf.Identity();
    float acc = 0f;
    var sw = Stopwatch.StartNew();
    for (var r = 0; r < MeshReps; r++)
    {
        var p2 = mesh.Transform(t).Points;
        for (var j = 0; j < p2.Count; j++) acc += p2[j].X;
    }
    sw.Stop();
    if (acc == float.NegativeInfinity) Console.WriteLine("unreachable");
    return sw.Elapsed.TotalMilliseconds;
}

static double BenchUnoptMesh()
{
    var pts = new UPt[MeshPoints];
    for (var i = 0; i < MeshPoints; i++) pts[i] = new UPt(i, i * 0.5f, i * 0.25f);
    var mesh = new UMesh(pts, new UInt3[0]);
    var t = UXf.Identity();
    float acc = 0f;
    var sw = Stopwatch.StartNew();
    for (var r = 0; r < MeshReps; r++)
    {
        var p2 = mesh.Transform(t).Points;
        for (var j = 0; j < p2.Count; j++) acc += p2[j].X;
    }
    sw.Stop();
    if (acc == float.NegativeInfinity) Console.WriteLine("unreachable");
    return sw.Elapsed.TotalMilliseconds;
}

static double Best(Func<double> f)
{
    for (var i = 0; i < Warmup; i++) f();
    var best = double.MaxValue;
    for (var i = 0; i < Trials; i++) best = Math.Min(best, f());
    return best;
}

Console.WriteLine($"Triangle3D.Transform x {Iterations:N0} (best of {Trials}, ms)");
var un = Best(BenchUnopt);
var op = Best(BenchOpt);
Console.WriteLine($"  unoptimized (Deform + Func delegate): {un,8:N1} ms");
Console.WriteLine($"  optimized   (inlined, delegate-free): {op,8:N1} ms");
Console.WriteLine($"  speedup: {un / op:N2}x");

Console.WriteLine();
Console.WriteLine($"TriangleMesh3D.Transform: {MeshPoints:N0} points x {MeshReps} reps (best of {Trials}, ms)");
var unM = Best(BenchUnoptMesh);
var opM = Best(BenchOptMesh);
Console.WriteLine($"  unoptimized (Deform + Map delegate) : {unM,8:N1} ms");
Console.WriteLine($"  optimized   (fused loop, delegate-free): {opM,8:N1} ms");
Console.WriteLine($"  speedup: {unM / opM:N2}x");
