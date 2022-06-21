using Vim.G3d;

namespace Vim.Geometry
{
    public static class Serialization
    {
        public static IMesh ReadG3D(string filePath)
            => G3D.Read(filePath).ToIMesh();

        public static G3D ToG3d(this IMesh mesh)
            => mesh is G3D r ? r : mesh.Attributes.ToG3d();

        public static void WriteG3d(this IMesh mesh, string filePath)
            => mesh.ToG3d().Write(filePath);

        public static void WriteObj(this IMesh mesh, string filePath)
            => mesh.ToG3d().WriteObj(filePath);

        public static void WritePly(this IMesh mesh, string filePath)
            => mesh.ToG3d().WritePly(filePath);
    }
}
