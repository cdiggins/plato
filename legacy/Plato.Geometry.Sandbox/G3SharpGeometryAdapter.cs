using g3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vim.DotNetUtilities;

namespace Vim.Geometry
{
    public class G3SharpGeometryAdapter
    {
        public IMesh Source { get; private set; }

        // TODO: if the vertices and indices are the same, then this could be accelerated. 

        private readonly MemoizedFunction<IMesh, DMesh3> GeometryToDMesh
            = new MemoizedFunction<IMesh, DMesh3>((g) => g.ToG3Sharp());

        private readonly MemoizedFunction<DMesh3, Reducer> DMeshToReducer
            = new MemoizedFunction<DMesh3, Reducer>((m) => new Reducer(new DMesh3(m)));

        private readonly MemoizedFunction<DMesh3, DMeshAABBTree3> DMeshToAABBTree
            = new MemoizedFunction<DMesh3, DMeshAABBTree3>((m) => m.BuildDMeshAABBTree());

        private readonly MemoizedFunction<DMesh3, MeshProjectionTarget> MeshToProjectionTarget;

        public DMesh3 DMesh => GeometryToDMesh.Call(Source);
        public Reducer Reducer => DMeshToReducer.Call(DMesh);
        public DMeshAABBTree3 AABBTree => DMeshToAABBTree.Call(DMesh);
        public MeshProjectionTarget ProjectionTarget => MeshToProjectionTarget.Call(DMesh);

        public G3SharpGeometryAdapter()
            => MeshToProjectionTarget = new MemoizedFunction<DMesh3, MeshProjectionTarget>(
                (m) => new MeshProjectionTarget(m, AABBTree));

        public IMesh Reduce(IMesh g, int vertexCount, bool project = false)
        {
            Source = g;
            var reducer = Reducer;

            reducer.SetExternalConstraints(new MeshConstraints());
            MeshConstraintUtil.FixAllBoundaryEdges(reducer.Constraints, DMesh);

            // reducer.ProjectionMode = Reducer.TargetProjectionMode.
            reducer.PreserveBoundaryShape = true;
            if (project)
                reducer.SetProjectionTarget(ProjectionTarget);
            return reducer.Reduce(vertexCount, true).ToIGeometry();
        }
    }
}
