using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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
    public Color Color1 = Color.red;
    public Color Color2 = Color.green;
    public float Thickness = 0.2f;
    public float Blend = 0.1f;
    public float Radius = 1f;
    public int FunctionIndex = 0;
    public Bitmap Bitmap;
    public float[] Distances;

    public static void UpdateMaterial(Material material, Bitmap bmp)
        => material.mainTexture = bmp.GetUpdatedTexture();

    public void UpdateMaterial(Bitmap bmp)
        => UpdateMaterial(GetComponent<Renderer>().sharedMaterial, bmp);

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    public static (float min, float max) UnitInterval = (0, 1);

    public static float Lerp((float min, float max) range, float amount)
        => Mathf.Lerp(range.min, range.max, amount);

    public static float InverseLerp((float min, float max) range, float amount)
        => Mathf.InverseLerp(range.min, range.max, amount);

    public static float Remap(float v, (float min, float max) input, (float min, float max) output)
        => Lerp(output, InverseLerp(input, v));

    public static float Clamp(float v, (float min, float max) range)
        => Mathf.Clamp(v, range.min, range.max);

    public static float ClampAndRemap(float v, (float min, float max) input, (float min, float max) output)
        => Remap(Clamp(v, input), input, output);

    public static float CircleSDF(float x, float y, float r = 1)
        => Mathf.Sqrt((x * x) + (y * y)) - r;

    public float Distance(int a, int b)
    {
        if (a < 0 || b < 0 || a >= Width || b >= Height) return float.PositiveInfinity;
        return Distances[b * Width + a];
    }

    public void SetDistance(int a, int b, float f)
    {
        if (a < 0 || b < 0 || a >= Width || b >= Height) return;
        Distances[b * Width + a] = Mathf.Min(Distances[b * Width + a], f);
    }

    public static void DrawSDF(Bitmap bmp, float thickness, float blend, float radius, Color color1, Color color2, Func<float, float, float> sdf)
    {

        for (var j=0; j < bmp.Width; j++)
        for (var i=0; i < bmp.Width; i++)
        {
            var x = Remap(i, (0, bmp.Width), (-2, +2));
            var y = Remap(j, (0, bmp.Width), (-2, +2));
            var distance = Mathf.Abs(sdf(x, y)) - thickness;
            var colorAmount = ClampAndRemap(distance, (0, blend), UnitInterval);
            var color = Color.Lerp(color1, color2, colorAmount);
            bmp.SetColor(i, j, color);
        }
    }

    public void DrawCurve(Bitmap bmp, float thickness, float blend, float radius, Color color1, Color color2, Func<float, float> curve)
    {
        for (var i = 0; i < Distances.Length; ++i)
            Distances[i] = float.PositiveInfinity;

        for (var i = 0; i < bmp.Width; i++)
        {
            var x = Remap(i, (0, bmp.Width), (-1, +1));
            var computedY = curve(x);

            for (var j = 0; j < bmp.Width; j++)
            {
                var localY = Remap(j, (0, bmp.Width), (-1, +1));
                var verticalDistance = Mathf.Abs(localY - computedY) - thickness;
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
            var colorAmount = ClampAndRemap(Distance(i, j), (0, blend), UnitInterval);
            var color = Color.Lerp(color1, color2, colorAmount);
            bmp.SetColor(i, j, color);
        }
    }

    public Func<float, float, float> GetSDF()
    {
        switch (FunctionIndex)
        {
            case 0: return (x, y) => CircleSDF(x, y, Radius);
            case 1: return (x, y) => Mathf.Abs(y - x * x);
            case 2: return (x, y) => Mathf.Abs(y - x * x * x);
            case 3: return (x, y) => Mathf.Abs(y - Mathf.Sqrt(x));
            case 4: return (x, y) => Mathf.Abs(y - Mathf.Sin(x));
            case 5: return (x, y) => Mathf.Abs(y - Mathf.Cos(x));
            case 6: return (x, y) => Mathf.Abs(y - Mathf.Tan(x));
            default: return (x, y) => CircleSDF(x, y, Radius);
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

//        DrawSDF(Bitmap, Thickness, Blend, Radius, Color1, Color2, GetSDF());

        DrawCurve(Bitmap, Thickness, Blend, Radius, Color1, Color2, GetCurve());

        yield return null;
        UpdateMaterial(Bitmap);
        yield return null;
    }

    public float RelativeTime(float time, float period)
    {
        return (time % period) / period;
    }

    public float SinEasing(float t)
        => (Mathf.Sin(t * Mathf.PI * 2) + 1) / 2;

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
            Distances = new float[Width * Height];
        }

        var hue = RelativeTime(Time.time, 12);
        var t = RelativeTime(Time.time, 3);
        Color1 = Color.HSVToRGB(hue, Saturation, Value);
        Color2 = Color.HSVToRGB((hue + 0.3f) % 1, Saturation, Value);
        Thickness = Lerp((0.001f, 0.5f), SinEasing(t));
        Radius = Lerp((0, 1.5f), SinEasing(t));
        StartCoroutine(UpdateMaterial());
    }
}