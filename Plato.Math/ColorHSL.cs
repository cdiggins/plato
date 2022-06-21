using System;
using Vim.Math3d;

namespace Vim.Desktop.Scripts
{
    public static class ColorExtensions
    {
        public static byte ToByte(this double d)
            => (byte)(d * 0xFF);

        public static byte ToByte(this float f)
            => (byte)(f * 0xFF);

        public static ColorRGBA ToColorRGBA(this ColorHDR hdr)
            => new ColorRGBA(hdr.R.ToByte(), hdr.G.ToByte(), hdr.B.ToByte(), hdr.A.ToByte());

        public static ColorRGB ToColorRGB(this ColorHDR hdr)
            => hdr.ToColorRGB();

        public static ColorRGB ToColorRGB(this ColorRGBA col)
            => new ColorRGB(col.R, col.G, col.B);

        public static ColorRGB ToColorRGB(this ColorHSL hsl)
            => hsl.ToColor().ToColorRGBA().ToColorRGB();

        public static ColorRGBA ToColorRGBA(this ColorRGB col)
            => ColorRGBA.Create(col.R, col.G, col.B, 0xFF);

        public static Random Random = new Random(123);

        public static ColorHSL CreateRandomColor()
            => new ColorHSL(Random.NextDouble() * 360.0, 1.0, 1.0);

    }
    public struct ColorHSL
    {
        public readonly double H; // Hue [0..360)
        public readonly double S; // Saturation [0..1]
        public readonly double L; // Lightness  [0..1]

        public ColorHSL(double h, double s, double l)
            => (H, S, L) = (h, s, l);

        public static byte ToByte(double d)
            => (byte)(d * 0xFF);

        public static ColorHDR CreateHDR(double r, double g, double b)
            => new((float)r, (float)g, (float)b, 1);

        public double V
            => (1.0 - Math.Abs(2.0 * L - 1.0)) * S;

        public double C
            => V * S;

        public ColorHDR ToColor()
        {
            // https://en.wikipedia.org/wiki/HSL_and_HSV
            var h1 = H / 60.0;
            var x = C * (1.0 - Math.Abs((h1 % 2.0) - 1.0));
            var m = L - C / 2.0;
            var cm = C + m;
            var xm = x + m;
            if (h1 < 0)
                return CreateHDR(m, m, m);
            if (h1 < 1)
                return CreateHDR(cm, xm, m);
            if (h1 < 2)
                return CreateHDR(xm, cm, m);
            if (h1 < 3)
                return CreateHDR(m, cm, xm);
            if (h1 < 4)
                return CreateHDR(m, xm, cm);
            if (h1 < 5)
                return CreateHDR(xm, m, cm);
            if (h1 <= 6)
                return CreateHDR(cm, m, xm);
            return CreateHDR(m, m, m);
        }
    }
}
