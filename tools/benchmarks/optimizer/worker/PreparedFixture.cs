using System.Reflection;
using System.Text;
using System.Text.Json;
using Ara3D.IO.StepParser;
using Ara3D.Step.Decode;
using Ara3D.Step.Entities;
using Ara3D.Step.Mesher.Plato;
using Ara3D.StepBrep;
using Ara3D.Utils;
using Ara3D.Step.CorpusHarness;

namespace Plato.OptimizerBenchmark;

public sealed record PreparedFixture(int Schema, string Id, string Source, string InputHash, string PreparationIdentity,
    double Chordal, BrepModel Model, BrepModel Cached, EdgeDiscretizations Edges,
    IReadOnlyList<FaceMeshOutcome> Faces, PipelineState Final, IReadOnlyList<CurveSample> Curves,
    IReadOnlyList<SurfaceSample> Surfaces, IReadOnlyList<string> Tags, IReadOnlyList<string> Workloads)
{
    public static string[] Witnesses()
        => typeof(BrepModel).Assembly.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .Where(m => m.ReturnType == typeof(BrepModel) && m.Name.StartsWith("Witness_", StringComparison.Ordinal)
                && Enumerable.All(m.GetParameters(), p => p.ParameterType == typeof(Number)))
            .Select(m => m.Name[8..] + string.Concat(m.GetParameters().Select(_ => ":0.3"))).Distinct().Order().ToArray();

    static BrepModel Witness(string spec)
    {
        var parts = spec.Split(':');
        var arguments = Enumerable.Skip(parts, 1).Select(p => (object)(Number)double.Parse(p, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var method = typeof(BrepModel).Assembly.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .Single(m => m.Name == $"Witness_{parts[0]}" && m.ReturnType == typeof(BrepModel)
                && m.GetParameters().Length == arguments.Length && Enumerable.All(m.GetParameters(), p => p.ParameterType == typeof(Number)));
        return (BrepModel)method.Invoke(null, arguments)!;
    }

    public static PreparedFixture Create(string source, string id, double chordal)
    {
        var identity = PortableJson.Hash(File.ReadAllBytes(typeof(BrepModel).Assembly.Location));
        var inputHash = PortableJson.Hash(source.StartsWith("witness:", StringComparison.Ordinal)
            ? Encoding.UTF8.GetBytes(source + "\n" + identity) : File.ReadAllBytes(source));
        var model = source.StartsWith("witness:", StringComparison.Ordinal) ? Witness(source[8..])
            : new StepModel(new StepDocument(new FilePath(source))).DecodeRaw().PrepareModel().Model;
        var cached = model.PolygonTopology().Model.CacheProjectionGrids();
        var options = StepPlatoMesher.Options(chordal);
        var tables = StepPlatoTables.Of(cached);
        var edges = Operations.Edges(cached, tables, options);
        var faces = Operations.Faces(cached, tables, options, edges);
        var final = Operations.Refine(cached, tables, options, edges, faces);
        var curves = new List<CurveSample>();
        for (var c = 0; c < cached.Geometry.Curves.Count; c++)
        {
            var index = new CurveIndex(c);
            if (!(bool)cached.Geometry.CanEvaluate(index)) continue;
            for (var i = 1; i <= 8; i++)
                curves.Add(new(index, cached.Geometry.Domain(index).SampleIn((Number)(i / 9.0))));
        }
        var surfaces = new List<SurfaceSample>();
        for (var s = 0; s < cached.Geometry.Surfaces.Count; s++)
        {
            var index = new SurfaceIndex(s);
            if (!(bool)cached.Geometry.CanEvaluate(index)) continue;
            for (var i = 1; i <= 8; i++)
            {
                var uv = cached.Geometry.Domain(index).SampleIn(new UvCoordinate(i / 9.0, (9 - i) / 9.0));
                var p = cached.Geometry.Eval(index, uv);
                surfaces.Add(new(index, uv, new Point3D(p.X + chordal, p.Y - chordal, p.Z + chordal)));
            }
        }
        var tags = cached.Geometry.Curves.Select(c => "curve:" + c.Name())
            .Concat(cached.Geometry.Surfaces.Select(s => "surface:" + s.Name()))
            .Concat(faces.Select(f => "path:" + FaceTiming.PathOf(f)))
            .Concat(new[] { "edges:" + SizeBin(cached.Edges.Count), "faces:" + SizeBin(cached.Faces.Count) })
            .Concat(SplineTags(cached.Geometry))
            .Distinct().Order().ToArray();
        var workloads = Operations.Names.Where(w => w switch
        {
            "curve-eval" => curves.Count > 0,
            "surface-eval" or "projection" => surfaces.Count > 0,
            "edges" => cached.Edges.Count > 0,
            "refinement" => final.Round.Value > 0,
            "projection-grids" => Enumerable.Any(cached.Geometry.ProjectionGrids, grids => grids.Count > 0),
            "faces" or "assembly" or "model" => cached.Faces.Count > 0,
            _ => true,
        }).ToArray();
        return new(1, id, source, inputHash, identity, chordal, model, cached, edges, faces, final, curves, surfaces, tags, workloads);
    }

    public static PreparedFixture Read(string path)
    {
        var fixture = JsonSerializer.Deserialize<PreparedFixture>(File.ReadAllBytes(path), PortableJson.Options)
            ?? throw new InvalidDataException("Empty fixture");
        if (fixture.Schema != 1) throw new InvalidDataException("Unsupported fixture schema");
        return fixture;
    }

    static string SizeBin(int count)
        => count switch { 0 => "empty", <= 16 => "tiny", <= 128 => "small", <= 1024 => "medium", _ => "large" };

    static IEnumerable<string> SplineTags(GeometrySet geometry)
    {
        foreach (var curve in geometry.Curves.Where(c => c.Kind == BrepCurve.Kind_BSpline))
        {
            yield return "spline-curve-degree:" + curve.BSpline_Data.Basis.Degree.Value;
            yield return WeightTag(curve.BSpline_Data.Weights);
        }
        foreach (var surface in geometry.Surfaces.Where(s => s.Kind == BrepSurface.Kind_BSpline))
        {
            yield return "spline-surface-degree-u:" + surface.BSpline_Data.U.Degree.Value;
            yield return "spline-surface-degree-v:" + surface.BSpline_Data.V.Degree.Value;
            yield return WeightTag(surface.BSpline_Data.Weights.Elements);
        }
    }

    static string WeightTag(IReadOnlyList<Number> weights)
        => "spline-weights:" + (Enumerable.Any(Enumerable.Skip(weights.Select(w => w.Value).Distinct(), 1)) ? "nonuniform" : "uniform-or-absent");
}

public sealed record CurveSample(CurveIndex Curve, Number Parameter);
public sealed record SurfaceSample(SurfaceIndex Surface, UvCoordinate Parameter, Point3D Point);
