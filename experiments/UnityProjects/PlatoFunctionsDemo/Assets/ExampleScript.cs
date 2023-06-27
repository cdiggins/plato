using System;
using System.Collections;
using UnityEngine;
using Plato;
using Color = UnityEngine.Color;
using Time = UnityEngine.Time;

public class Bitmap
{
    public int Width { get; }
    public Texture2D Texture { get; }
    public byte[] Bytes { get; }

    public Bitmap(int width, int height)
    {
        Width = width;
        Texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Bytes = new byte[Width * Width * 4];
    }

    public Texture2D GetUpdatedTexture()
    {
        Texture.LoadRawTextureData(Bytes);
        Texture.Apply();
        return Texture;
    }

    public void SetColor(int x, int y, Color32 color)
    {
        var i = y * Width + x;
        Bytes[i * 4 + 0] = color.r;
        Bytes[i * 4 + 1] = color.g;
        Bytes[i * 4 + 2] = color.b;
        Bytes[i * 4 + 3] = color.a;
    }
}

public class ExampleScript : MonoBehaviour
{
    public int Width = 10;
    public int Height => Width;
    public float Saturation = 1f;
    public float Value = 1f;
    public UnityEngine.Color Color1 = UnityEngine.Color.red;
    public UnityEngine.Color Color2 = UnityEngine.Color.green;
    public double Thickness = 0.2f;
    public double Blend = 0.1f;
    public double Radius = 1f;
    public int FunctionIndex = 0;
    public Bitmap Bitmap;
    public double[] Distances;

    public static void UpdateMaterial(Material material, Bitmap bmp)
        => material.mainTexture = bmp.GetUpdatedTexture();

    public void UpdateMaterial(Bitmap bmp)  
        => UpdateMaterial(GetComponent<Renderer>().sharedMaterial, bmp);

    public double Distance(int a, int b)
    {
        if (a < 0 || b < 0 || a >= Width || b >= Height) return double.PositiveInfinity;
        return Distances[b * Width + a];
    }

    public void SetDistance(int a, int b, double d)
    {
        if (a < 0 || b < 0 || a >= Width || b >= Height) return;
        Distances[b * Width + a] = d.Min(Distances[b * Width + a]);
    }

    public static void DrawSDF(Bitmap bmp, double thickness, double blend, double radius, Color color1, Color color2, Func<double, double, double> sdf)
    {

        for (var j=0; j < bmp.Width; j++)
        for (var i=0; i < bmp.Width; i++)
        {
            var x = ClampAndRemap(i, (0, bmp.Width), (-2, +2));
            var y = ClampAndRemap(j, (0, bmp.Width), (-2, +2));
            var distance = Math.Abs(sdf(x, y)) - thickness;
            var colorAmount = ClampAndRemap(distance, (0, blend), 1);
            var color = Color.Lerp(color1, color2, (float)colorAmount);
            bmp.SetColor(i, j, color);
        }
    }

    public static double ClampAndRemap(double v, Interval input, Interval output)
        => v.Clamp(input).Remap(input, output);

    public void DrawCurve(Bitmap bmp, float thickness, float blend, float radius, Color color1, Color color2, Func<double, double> curve)
    {
        for (var i = 0; i < Distances.Length; ++i)
            Distances[i] = float.PositiveInfinity;

        for (var i = 0; i < bmp.Width; i++)
        {
            var x = IntervalOperations.Remap(i, (0, bmp.Width), (-1, +1));
            var computedY = curve(x);

            for (var j = 0; j < bmp.Width; j++)
            {
                var localY = IntervalOperations.Remap(j, (0, bmp.Width), (-1, +1));
                var verticalDistance = Math.Abs(localY - computedY) - thickness;
                SetDistance(i, j, verticalDistance);
            }
        }

        var e = 2f / Bitmap.Width;
        var e2 = e * Mathf.Sqrt(2);

        // Scan to the right each 
        for (var j = 0; j < bmp.Width; j++)
        for (var i = 0; i < bmp.Width; i++)
        {
            var d = Distance(i, j);
            SetDistance(i+1, j, d + e);
            SetDistance(i+1, j+1, d + e2);
            SetDistance(i+1, j-1, d + e2);
        }

        // Scan to the lft each 
        for (var j = 0; j < bmp.Width; j++)
        for (var i = bmp.Width-1; i >= 0; i--)
        {
            var d = Distance(i, j);
            SetDistance(i - 1, j, d + e);
            SetDistance(i - 1, j + 1, d + e2);
            SetDistance(i - 1, j - 1, d + e2);
        }

        for (var j = 0; j < bmp.Width; j++)
        for (var i = 0; i < bmp.Width; i++)
        {
            var colorAmount = ClampAndRemap(Distance(i, j), (0, blend), 1);
            var color = Color.Lerp(color1, color2, (float)colorAmount);
            bmp.SetColor(i, j, color);
        }
    }

    public Func<double, double, double> GetSDF()
    {
        switch (FunctionIndex)
        {
            case 0: return (x, y) => DistanceField2DOperations.CircleSDF((x, y), Radius);
            case 1: return (x, y) => Math.Abs(y - x * x);
            case 2: return (x, y) => Math.Abs(y - x * x * x);
            case 3: return (x, y) => Math.Abs(y - Math.Sqrt(x));
            case 4: return (x, y) => Math.Abs(y - Math.Sin(x));
            case 5: return (x, y) => Math.Abs(y - Math.Cos(x));
            case 6: return (x, y) => Math.Abs(y - Math.Tan(x));
            default: return (x, y) => DistanceField2DOperations.CircleSDF((x, y), Radius);
        }
    }

    public Func<float, float> GetCurve()
    {
        switch (FunctionIndex)
        {
            case 0: return x => x;
            case 1: return x => x * x;
            case 2: return x => x * x * x;
            case 3: return Mathf.Sqrt;
            case 4: return Mathf.Sin;
            case 5: return Mathf.Cos;
            case 6: return Mathf.Tan;
            default: return x => x;
        }
    }

    IEnumerator UpdateMaterial()
    {
        yield return null;

        DrawSDF(Bitmap, Thickness, Blend, Radius, Color1, Color2, GetSDF());

//        DrawCurve(Bitmap, Thickness, Blend, Radius, Color1, Color2, GetCurve());

        yield return null;
        UpdateMaterial(Bitmap);
        yield return null;
    }

    public float RelativeTime(float time, float period)
    {
        return (time % period) / period;
    }

    public double SinEasing(double t)
        => (Math.Sin(t * Math.PI * 2) + 1) / 2;

    /*

    //Find roots using Cardano's method. http://en.wikipedia.org/wiki/Cubic_function#Cardano.27s_method
    Vector2 SolveCubic2(Vector3 a)
    {
        var p = a.y - a.x * a.x / 3f;
        var p3 = p * p * p;
        var q = a.x * (2f * a.x * a.x - 9f * a.y) / 27f + a.z;
        var d = q * q + 4f * p3 / 27f;
        if (d > 0)
        {
            var x = (new Vector2(1, -1) * Mathf.Sqrt(d) - q) * 0.5f;
            return new Vector2(addv(Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), new Vector2(1f / 3f))) - a.x / 3f);
        }
        var v = Mathf.Acos(-Mathf.Sqrt(-27f / p3) * q * .5f) / 3f;
        var m = Mathf.Cos(v);
        var n = Mathf.Sin(v) * 1.732050808;
        return new Vector2(m + m, -n - m) * Mathf.Sqrt(-p / 3f) - a.x / 3f;
    }

    // How to solve the equation below can be seen on this image.
    // http://www.perbloksgaard.dk/research/DistanceToQuadraticBezier.jpg
    float CalculateDistanceToQuadraticBezier(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        b += Vector2.Lerp(new Vector2(1e-4, 1e-4), Vector2.zero, Mathf.Abs(Mathf.Sign(b * 2.0f - a - c)));
        var A = b - a;
        var B = c - b - A;
        var C = p - a;
        var D = A * 2.;
        var T = Mathf.Clamp((SolveCubic2(vec3(-3.* dot(A, B), dot(C, B) - 2.* dd(A), dot(C, A)) / -dd(B))), 0., 1.);
        return sqrt(min(dd(C - (D + B * T.x) * T.x), dd(C - (D + B * T.y) * T.y)));
    }
    */

    public void Update()
    {
        if (Width != Bitmap?.Width || Height != Bitmap?.Width)
        {
            Bitmap = new Bitmap(Width, Height);
            Distances = new double[Width * Height];
        }

        var hue = RelativeTime(Time.time, 12);
        var t = RelativeTime(Time.time, 3);
        Color1 = Color.HSVToRGB(hue, Saturation, Value);
        Color2 = Color.HSVToRGB((hue + 0.3f) % 1, Saturation, Value);
        Thickness = SinEasing(t).Lerp(0.001f, 0.5f);
        Radius = SinEasing(t).Lerp(0, 1.5f);
        StartCoroutine(UpdateMaterial());
    }
}