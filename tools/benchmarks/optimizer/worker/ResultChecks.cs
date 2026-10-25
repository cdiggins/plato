using Ara3D.Step.CorpusHarness;
using Ara3D.StepBrep;

namespace Plato.OptimizerBenchmark;

public static class ResultChecks
{
    static bool Finite(Point3D p)
        => double.IsFinite(p.X) && double.IsFinite(p.Y) && double.IsFinite(p.Z);

    public static Dictionary<string, bool> Of(PreparedFixture fixture, object result)
    {
        var checks = new Dictionary<string, bool> { ["output_present"] = result is not null };
        switch (result)
        {
            case Point3D[] points:
                checks["finite_points"] = points.Length > 0 && Enumerable.All(points, Finite);
                break;
            case UvCoordinate[] uv:
                checks["finite_parameters"] = uv.Length > 0 && Enumerable.All(uv, p => double.IsFinite(p.U) && double.IsFinite(p.V));
                break;
            case EdgeDiscretizations edges:
                checks["finite_edge_points"] = Enumerable.All(edges.ByEdge, e => Enumerable.All(e.Points, Finite));
                checks["edge_rows_present"] = edges.ByEdge.Count == fixture.Cached.Edges.Count;
                break;
            case FaceMeshOutcome[] faces:
                AddFaces(checks, fixture.Cached, faces, fixture.Chordal, fixture.Faces);
                break;
            case PipelineState state:
                AddFaces(checks, fixture.Cached, state.Outcomes, fixture.Chordal);
                break;
            case ModelResult model:
                AddFaces(checks, model.Model, model.Final.Outcomes, fixture.Chordal);
                AddMesh(checks, model.Model, model.Final.Outcomes, model.Mesh);
                break;
            case ModelMesh mesh:
                AddMesh(checks, fixture.Cached, fixture.Final.Outcomes, mesh);
                break;
            case BrepModel cached:
                checks["model_indices_in_range"] = cached.Law_IndicesInRange();
                checks["geometry_indices_in_range"] = cached.Geometry.IsEveryIndexInRange();
                break;
        }
        return checks;
    }

    static FaceRow[] Rows(BrepModel model, IReadOnlyList<FaceMeshOutcome> outcomes)
        => outcomes.Select((o, i) => new FaceRow(i, model.SurfaceName(new BrepFaceIndex(i)), model.Faces[i].Bounds.Count,
            0, false, 0, o.Mesh.Triangles.Count, o.Mesh.Quads.Count, FaceTiming.PathOf(o),
            new FaceStages(new double[FaceStages.All.Length]), 0, "", "", o)).ToArray();

    static void AddFaces(Dictionary<string, bool> checks, BrepModel model, IReadOnlyList<FaceMeshOutcome> faces, double chordal,
        IReadOnlyList<FaceMeshOutcome>? baseline = null)
    {
        checks["face_rows_present"] = faces.Count == model.Faces.Count;
        var actual = ContractCheck.Of(model, Rows(model, faces), chordal, false);
        if (baseline is null)
            checks["face_contract"] = actual.Holds;
        else
        {
            var expected = ContractCheck.Of(model, Rows(model, baseline), chordal, false);
            checks["first_pass_contract_matches_baseline"] = actual.Holds == expected.Holds
                && actual.OverChordal == expected.OverChordal && actual.WithFlipped == expected.WithFlipped
                && actual.WithDegenerate == expected.WithDegenerate && actual.Unexplained == expected.Unexplained;
        }
        checks["finite_face_positions"] = Enumerable.All(faces, f => Enumerable.All(f.Mesh.Positions, Finite));
    }

    public static object Diagnostics(PreparedFixture fixture, object result)
        => result is FaceMeshOutcome[] faces ? new
        {
            semantics = "First pass may request refinement; contract verdict must match the prepared baseline and outputs must agree across recipes.",
            baseline_contract = ContractCheck.Of(fixture.Cached, Rows(fixture.Cached, fixture.Faces), fixture.Chordal, false),
            actual_contract = ContractCheck.Of(fixture.Cached, Rows(fixture.Cached, faces), fixture.Chordal, false),
        } : (object)new { semantics = "Final meshing stages require strict contracts; geometry stages require finite deterministic outputs." };

    static void AddMesh(Dictionary<string, bool> checks, BrepModel model, IReadOnlyList<FaceMeshOutcome> faces, ModelMesh mesh)
    {
        checks["finite_mesh_positions"] = Enumerable.All(mesh.Positions, Finite);
        checks["mesh_elements_present"] = mesh.Triangles.Count + mesh.Quads.Count > 0;
        checks["mesh_oriented"] = MeshCheck.Of(mesh).Oriented;
        checks["closed_shells"] = BoundaryCheck.Of(model, mesh, Rows(model, faces), model.EdgesByFace()).ClosedShellsAreClosed;
        checks["mesh_indices_in_range"] = Enumerable.All(mesh.Triangles, t => Index(t.A) && Index(t.B) && Index(t.C))
            && Enumerable.All(mesh.Quads, q => Index(q.A) && Index(q.B) && Index(q.C) && Index(q.D));
        bool Index(VertexIndex index) => index.Value >= 0 && index.Value < mesh.Positions.Count;
    }
}
