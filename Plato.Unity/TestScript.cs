using System;
using Plato.Geometry;
using Plato.Math;

namespace Plato.Unity
{
    public static class Extensions
    {
        public static IScene3D Arrange(this IArray<IMesh> mesh, Func<int, Vector3> transformFunc)
            => throw new NotImplementedException();

        public static float SampleFloat(int i, int n)
            => (float)i / (n - 1);

        public static Vector2 ToGridPosition(int row, int rows, int col, int cols)
            => (SampleFloat(col, cols), SampleFloat(row, rows));

        public static Vector2 ToGridPosition(int row, int rows, int col, int cols, float width, float height)
            => ToGridPosition(row, rows, col, cols) * (width, height);

        public static IScene3D CloneGrid(this IMesh mesh, int rows, int cols, float width, float height)
            => mesh.Repeat(rows * cols)
                .Arrange(i => ToGridPosition(i % cols, rows, i / rows, cols, width, height));

        public static IArray<(TX, TY)> CartesianProduct<TX, TY>(this IArray<TX> xs, IArray<TY> ys)
            => xs.Zip(ys, (x, y) => (x, y));

        public static IArray<(int, int)> CartesianProduct(this int numX, int numY) =>
            numX.Range().CartesianProduct(numY.Range());

        public 
    }

    public class TestScript : MonoBehaviour
    {

    }
}