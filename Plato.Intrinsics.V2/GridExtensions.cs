using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
using Ara3D.Collections;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The forward stdlib's <c>Indexable2D</c> / <c>Indexable3D</c> concepts spell the grid
    /// extents <c>ColumnCount</c> / <c>RowCount</c> / <c>LayerCount</c>; the handwritten
    /// collection surface spells them <c>NumColumns</c> / <c>NumRows</c> / <c>NumLayers</c>.
    /// These adapters bind the concept names to the collection interfaces without adding a
    /// second spelling to the shared SDK.
    /// </summary>
    public static class GridExtensions
    {
        [MethodImpl(AggressiveInlining)]
        public static int ColumnCount<T>(this IReadOnlyList2D<T> self) => self.NumColumns;

        [MethodImpl(AggressiveInlining)]
        public static int RowCount<T>(this IReadOnlyList2D<T> self) => self.NumRows;

        [MethodImpl(AggressiveInlining)]
        public static int ColumnCount<T>(this IReadOnlyList3D<T> self) => self.NumColumns;

        [MethodImpl(AggressiveInlining)]
        public static int RowCount<T>(this IReadOnlyList3D<T> self) => self.NumRows;

        [MethodImpl(AggressiveInlining)]
        public static int LayerCount<T>(this IReadOnlyList3D<T> self) => self.NumLayers;
    }
}
