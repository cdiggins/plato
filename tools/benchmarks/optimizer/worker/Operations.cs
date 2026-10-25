using Ara3D.Step.Mesher.Plato;
using Ara3D.StepBrep;

namespace Plato.OptimizerBenchmark;

public static class Operations
{
    public static readonly string[] Names = ["model", "projection-grids", "edges", "faces", "refinement", "assembly", "curve-eval", "surface-eval", "projection"];

    public static EdgeDiscretizations Edges(BrepModel model, StepPlatoTables tables, MeshingOptions options)
    {
        var edges = new EdgeDiscretization[model.Edges.Count];
        for (var i = 0; i < edges.Length; i++)
            edges[i] = tables.Discretized(model, new BrepEdgeIndex(i), options.Face.Discretization);
        return new(edges);
    }

    public static FaceMeshOutcome[] Faces(BrepModel model, StepPlatoTables tables, MeshingOptions options, EdgeDiscretizations edges)
    {
        var faces = new FaceMeshOutcome[model.Faces.Count];
        for (var i = 0; i < faces.Length; i++)
            faces[i] = tables.FaceOutcome(model, new BrepFaceIndex(i), edges, options.Face);
        return faces;
    }

    public static PipelineState Refine(BrepModel model, StepPlatoTables tables, MeshingOptions options,
        EdgeDiscretizations edges, IReadOnlyList<FaceMeshOutcome> faces)
    {
        var failures = new List<StepPlatoFailure>();
        var result = tables.Refined(model, options, edges, faces, failures);
        if (failures.Count != 0)
            throw new InvalidOperationException($"Refinement failed: {failures[0]}");
        return result;
    }

    public static ModelMesh Assemble(BrepModel model, PipelineState final)
    {
        var meshes = final.Outcomes.Select(f => f.Mesh).ToArray();
        return model.AssembleModelMesh(meshes, model.NumberMeshVertices(final.Discretizations, meshes));
    }

    public static ModelResult Model(BrepModel prepared, MeshingOptions options)
    {
        var model = prepared.PolygonTopology().Model.CacheProjectionGrids();
        var tables = StepPlatoTables.Of(model);
        var edges = Edges(model, tables, options);
        var faces = Faces(model, tables, options, edges);
        var final = Refine(model, tables, options, edges, faces);
        return new(model, final, Assemble(model, final));
    }

    public static Func<object> Bind(PreparedFixture fixture, string workload)
    {
        if (!fixture.Workloads.Contains(workload))
            throw new ArgumentException($"Workload {workload} is absent from fixture {fixture.Id}");
        var m = fixture.Cached;
        var options = StepPlatoMesher.Options(fixture.Chordal);
        var tables = workload is "edges" or "faces" or "refinement" ? StepPlatoTables.Of(m) : null;
        return workload switch
        {
            "model" => () => Model(fixture.Model, options),
            "projection-grids" => () => fixture.Model.CacheProjectionGrids(),
            "edges" => () => Edges(m, tables!, options),
            "faces" => () => Faces(m, tables!, options, fixture.Edges),
            "refinement" => () => Refine(m, tables!, options, fixture.Edges, fixture.Faces),
            "assembly" => () => Assemble(m, fixture.Final),
            "curve-eval" => () => CurveEval(m.Geometry, fixture.Curves),
            "surface-eval" => () => SurfaceEval(m.Geometry, fixture.Surfaces),
            "projection" => () => Project(m.Geometry, fixture.Surfaces),
            _ => throw new ArgumentException($"Unknown workload {workload}"),
        };
    }

    public static object BorrowedInputs(PreparedFixture fixture, string workload)
        => workload switch
        {
            "model" or "projection-grids" => fixture.Model,
            "edges" => fixture.Cached,
            "faces" => new { fixture.Cached, fixture.Edges },
            "refinement" => new { fixture.Cached, fixture.Edges, fixture.Faces },
            "assembly" => new { fixture.Cached, fixture.Final },
            "curve-eval" => new { fixture.Cached.Geometry, fixture.Curves },
            "surface-eval" or "projection" => new { fixture.Cached.Geometry, fixture.Surfaces },
            _ => throw new ArgumentException($"Unknown workload {workload}"),
        };

    static Point3D[] CurveEval(GeometrySet geometry, IReadOnlyList<CurveSample> samples)
    {
        var result = new Point3D[samples.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = geometry.Eval(samples[i].Curve, samples[i].Parameter);
        return result;
    }

    static Point3D[] SurfaceEval(GeometrySet geometry, IReadOnlyList<SurfaceSample> samples)
    {
        var result = new Point3D[samples.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = geometry.Eval(samples[i].Surface, samples[i].Parameter);
        return result;
    }

    static UvCoordinate[] Project(GeometrySet geometry, IReadOnlyList<SurfaceSample> samples)
    {
        var result = new UvCoordinate[samples.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = geometry.Project(samples[i].Surface, samples[i].Point);
        return result;
    }
}

public sealed record ModelResult(BrepModel Model, PipelineState Final, ModelMesh Mesh);
