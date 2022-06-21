using Vim.Math3d;

namespace Vim.Geometry
{
    public interface IBounded
    {
        AABox Bounds { get; }
    }

    public static class Bounded
    {
        public static AABox UpdateBounds(this IBounded self, AABox box)
            => box.Merge(self.Bounds);
    }
}
