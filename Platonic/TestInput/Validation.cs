using System.Collections.Generic;
using Vim.LinqArray;

namespace Vim.G3d
{
    public enum G3dErrors
    {
        NodesCountMismatch,
        MaterialsCountMismatch,
        IndicesInvalidCount,
        IndicesOutOfRange,

        //Submeshes
        SubmeshesCountMismatch,
        SubmeshesIndesxOffsetInvalidIndex,
        SubmeshesIndexOffsetOutOfRange,
        SubmeshesNonPositive,
        SubmeshesMaterialOutOfRange,

        //Meshes
        MeshesSubmeshOffsetOutOfRange,
        MeshesSubmeshCountNonPositive,

        // Instances
        InstancesCountMismatch,
        InstancesParentOutOfRange,
        InstancesMeshOutOfRange
    }

    public static class Validation
    {
        public static IEnumerable<G3dErrors> Validate(G3D g3d)
        {
            var errors = new List<G3dErrors>();

            void Validate(bool value, G3dErrors error)
            {
                if (!value) errors.Add(error);
            }

            //Indices
            Validate(g3d.Indices.Count % 3 == 0, G3dErrors.IndicesInvalidCount);
            Validate(g3d.Indices.All(i => i >= 0 && i < g3d.NumVertices), G3dErrors.IndicesOutOfRange);
            //Triangle should have 3 distinct vertices
            //Assert.That(g3d.Indices.SubArrays(3).Select(face => face.ToEnumerable().Distinct().Count()).All(c => c == 3));

            //Submeshes
            Validate(g3d.NumSubmeshes >= g3d.NumMeshes, G3dErrors.SubmeshesCountMismatch);
            Validate(g3d.NumSubmeshes == g3d.SubmeshMaterials.Count, G3dErrors.SubmeshesCountMismatch);
            Validate(g3d.NumSubmeshes == g3d.SubmeshIndexOffsets.Count, G3dErrors.SubmeshesCountMismatch);
            Validate(g3d.SubmeshIndexOffsets.All(i => i % 3 == 0), G3dErrors.SubmeshesIndesxOffsetInvalidIndex);
            Validate(g3d.SubmeshIndexOffsets.All(i => i >= 0 && i < g3d.NumCorners),
                G3dErrors.SubmeshesIndexOffsetOutOfRange);
            Validate(g3d.SubmeshIndexCount.All(i => i > 0), G3dErrors.SubmeshesNonPositive);
            Validate(g3d.SubmeshMaterials.All(m => m < g3d.NumMaterials), G3dErrors.SubmeshesMaterialOutOfRange);

            //Mesh
            Validate(g3d.MeshSubmeshOffset.All(i => i >= 0 && i < g3d.NumSubmeshes),
                G3dErrors.MeshesSubmeshOffsetOutOfRange);
            Validate(g3d.MeshSubmeshCount.All(i => i > 0), G3dErrors.MeshesSubmeshCountNonPositive);

            //Instances
            Validate(g3d.NumInstances == g3d.InstanceParents.Count, G3dErrors.InstancesCountMismatch);
            Validate(g3d.NumInstances == g3d.InstanceMeshes.Count, G3dErrors.InstancesCountMismatch);
            Validate(g3d.NumInstances == g3d.InstanceTransforms.Count, G3dErrors.InstancesCountMismatch);
            Validate(g3d.NumInstances == g3d.InstanceFlags.Count, G3dErrors.InstancesCountMismatch);
            Validate(g3d.InstanceParents.All(i => i < g3d.NumInstances), G3dErrors.InstancesParentOutOfRange);
            Validate(g3d.InstanceMeshes.All(i => i < g3d.NumMeshes), G3dErrors.InstancesMeshOutOfRange);

            //Materials
            Validate(g3d.NumMaterials == g3d.MaterialColors.Count, G3dErrors.MaterialsCountMismatch);
            Validate(g3d.NumMaterials == g3d.MaterialGlossiness.Count, G3dErrors.MaterialsCountMismatch);
            Validate(g3d.NumMaterials == g3d.MaterialSmoothness.Count, G3dErrors.MaterialsCountMismatch);

            return errors;
        }
    }
}