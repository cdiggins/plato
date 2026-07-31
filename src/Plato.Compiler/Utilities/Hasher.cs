using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.Compiler.Utilities
{
    public static class Hasher
    {
        public static int Combine(int a, int b)
        {
            unchecked
            {
                return a * 397 ^ b;
            }
        }

        public static int Combine(params int[] codes)
            => codes.Aggregate(0, Combine);

        public static int Combine(IEnumerable<int> codes)
            => codes.Aggregate(0, Combine);

        public static int GetHashCode(object o)
            => o?.GetHashCode() ?? 0;

        public static int Hash(params object[] objects)
            => Combine(objects.Select(GetHashCode));

        public static int Hash<T>(IEnumerable<T> objects)
            => Combine(objects.Select(x => GetHashCode(x)));
    }
}