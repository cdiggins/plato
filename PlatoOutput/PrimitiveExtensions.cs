using System;

namespace Ara3D
{
    public static class PrimitiveExtensions
    {
        public static T ChangePrecision<T>(this T self) => self;
        public static float ChangePrecision(this double self) => (float)self;
        public static string ChangePrecision(this string self) => self;
    }
}
