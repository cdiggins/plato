using Vim.Math3d;

namespace Vim.G3d
{
    public class G3dMaterial
    {
        public readonly G3D G3d;
        public readonly int Index;

        public G3dMaterial(G3D g3D, int index)
        {
            G3d = g3D;
            Index = index;
        }

        public Vector4 Color => G3d.MaterialColors[Index];
        public float Glossiness => G3d?.MaterialGlossiness[Index] ?? 0f;
        public float Smoothness => G3d?.MaterialSmoothness[Index] ?? 0f;
    }
}