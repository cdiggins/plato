using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

// https://gist.github.com/mattwarren/d17a0c356bd6fdb9f596bee6b9a5e63c
// https://fabiensanglard.net/postcard_pathtracer/index.html
// https://mattwarren.org/2019/03/01/Is-CSharp-a-low-level-language/
// https://fabiensanglard.net/postcard_pathtracer/formatted_full.html

namespace PathTracer
{
    public readonly struct Vector
    {
        public readonly float X, Y, Z;
        public Vector WithX(float x) => new(x, Y, Z);
        public Vector WithY(float y) => new(X, y, Z);
        public Vector WithZ(float z) => new(X, Y, z);
        public static implicit operator Vector(float v) => new(v, v, v);
        public Vector(float a, float b, float c = 0) => (X, Y, Z) = (a, b, c);
        public static Vector operator +(Vector q, Vector r) => new(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
        public static Vector operator *(Vector q, Vector r) => new(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
        public static float operator %(Vector q, Vector r) => q.X * r.X + q.Y * r.Y + q.Z * r.Z;
        public static Vector operator !(Vector q) => q * (1.0f / (float)Math.Sqrt(q % q));
        private const string format = ",10:N5"; // 5 decimal spaces, padded to 10 chars in total
        public override string ToString() => string.Format("{{ X:{0" + format + "}, Y:{1" + format + "}, Z:{2" + format + "} }}", X, Y, Z);
    }

    public static class Program
    {
        private static Random _random = new();
        public static Vector Vector(float a, float b, float c = 0) => new(a, b, c);
        public static float Min(float l, float r) => Math.Min(l, r);
        public static float randomVal() => (float)_random.NextDouble();
        public static float Fmodf(float x, float y) => x % y;
        public static float Fabsf(float x) => Math.Abs(x);
        public static float Sqrtf(float x) => (float)Math.Sqrt(x);
        public static float Powf(float x, float y) => (float)Math.Pow(x, y);
        public static float Cosf(float x) => (float)Math.Cos(x);
        public static float Sinf(float x) => (float)Math.Sin(x);

        // Rectangle CSG equation. Returns minimum signed distance from space carved by
        // lowerLeft vertex and opposite rectangle vertex upperRight.    
        public static float BoxTest(Vector position, Vector lowerLeft, Vector upperRight)
        {
            lowerLeft = position + (lowerLeft * -1.0f);
            upperRight += (position * -1.0f);
            return -Min(Min(Min(lowerLeft.X, upperRight.X), Min(lowerLeft.Y, upperRight.Y)), Min(lowerLeft.Z, upperRight.Z));
        }

        const int HIT_NONE = 0;
        const int HIT_LETTER = 1;
        const int HIT_WALL = 2;
        const int HIT_SUN = 3;

        static char[] letters =              // 15 two points lines
               ("5O5_" + "5W9W" + "5_9_" +        // P (without curve)
                "AOEO" + "COC_" + "A_E_" +        // I
                "IOQ_" + "I_QO" +               // X
                "UOY_" + "Y_]O" + "WW[W" +        // A
                "aOa_" + "aWeW" + "a_e_" + "cWiO") // R (without curve)
            .ToCharArray();

        // Two Curves (for P and R in PixaR) with hard-coded locations.
        public static Vector[] Curves = { Vector(-11, 6), Vector(11, 6) };

        // Sample the world using Signed Distance Fields.
        public static float QuerySDF(Vector position, out int hitType)
        {
            var distance = float.MaxValue; // 1e9;
            var f = position.WithZ(0);

            for (var i = 0; i < letters.Length; i += 4)
            {
                var begin = Vector(letters[i] - 79, letters[i + 1] - 79) * .5f;
                var e = Vector(letters[i + 2] - 79, letters[i + 3] - 79) * .5f + begin * -1f;
                var o = f + (begin + e * Min(-Min((begin + f * -1) % e / (e % e), 0), 1)) * -1;
                distance = Min(distance, o % o); // compare squared distance.
            }
            distance = Sqrtf(distance); // Get real distance, not square distance.

            for (var i = 1; i >= 0; i--)
            {
                var o = f + Curves[i] * -1;
                float temp;
                if (o.X > 0)
                {
                    temp = Fabsf(Sqrtf(o % o) - 2);
                }
                else
                {
                    o.WithY(o.Y > 0 ? o.Y - 2 : o.Y + 2);
                    temp = Sqrtf(o % o);
                }
                distance = Min(distance, temp);
            }
            distance = Powf(Powf(distance, 8) + Powf(position.Z, 8), 0.125f) - 0.5f;
            hitType = HIT_LETTER;

            var roomDist = Min(// Min(A,B) = Union with Constructive solid geometry
                               //-Min carves an empty space
                -Min(// Lower room
                    BoxTest(position, Vector(-30, -0.5f, -30), Vector(30, 18, 30)),
                    // Upper room
                    BoxTest(position, Vector(-25, 17, -25), Vector(25, 20, 25))
                ),
                BoxTest( // Ceiling "planks" spaced 8 units apart.
                    Vector(Fmodf(Fabsf(position.X), 8),
                        position.Y,
                        position.Z),
                    Vector(1.5f, 18.5f, -25),
                    Vector(6.5f, 20, 25)
                )
            );
            if (roomDist < distance) { distance = roomDist; hitType = HIT_WALL; };

            var sun = 19.9f - position.Y; // Everything above 19.9 is light source.
            if (sun < distance)
            {
                distance = sun;
                hitType = HIT_SUN;
            }

            return distance;
        }

        // Perform signed sphere marching
        // Returns hitType 0, 1, 2, or 3 and update hit position/normal
        static int RayMarching(Vector origin, Vector direction, ref Vector hitPos, ref Vector hitNorm)
        {
            var noHitCount = 0;
            float d; // distance from closest object in world.

            // Signed distance marching
            for (float total_d = 0; total_d < 100; total_d += d)
                if ((d = QuerySDF(hitPos = origin + direction * total_d, out var hitType)) < .01
                        || ++noHitCount > 99)
                {
                    hitNorm =
                       !Vector(QuerySDF(hitPos + Vector(.01f, 0), out _) - d,
                            QuerySDF(hitPos + Vector(0, .01f), out _) - d,
                            QuerySDF(hitPos + Vector(0, 0, .01f), out _) - d);
                    return hitType;
                }
            return 0;
        }

        static Vector Trace(Vector origin, Vector direction)
        {
            Vector sampledPosition = 0, normal = 0, color = 0, attenuation = 1;
            var lightDirection = !Vector(.6f, .6f, 1f); // Directional light

            for (var bounceCount = 3; bounceCount > 0; bounceCount--)
            {
                var hitType = RayMarching(origin, direction, ref sampledPosition, ref normal);
                if (hitType == HIT_NONE) break; // No hit. This is over, return color.
                if (hitType == HIT_LETTER)
                {
                    // Specular bounce on a letter. No color acc.
                    direction += normal * (normal % direction * -2);
                    origin = sampledPosition + direction * 0.1f;
                    attenuation *= 0.2f; // Attenuation via distance traveled.
                }
                if (hitType == HIT_WALL)
                {
                    // Wall hit uses color yellow?
                    var incidence = normal % lightDirection;
                    var p = 6.283185f * randomVal();
                    var c = randomVal();
                    var s = Sqrtf(1 - c);
                    var g = normal.Z < 0 ? -1f : 1f;
                    var u = -1 / (g + normal.Z);
                    var v = normal.X * normal.Y * u;
                    direction = Vector(v,
                                    g + normal.Y * normal.Y * u,
                                    -normal.Y) * (Cosf(p) * s)
                                +
                                Vector(1 + g * normal.X * normal.X * u,
                                    g * v,
                                    -g * normal.X) * (Sinf(p) * s) + normal * Sqrtf(c);
                    origin = sampledPosition + direction * .1f;
                    attenuation *= 0.2f;
                    if (incidence > 0 &&
                        RayMarching(sampledPosition + normal * .1f,
                                    lightDirection,
                                    ref sampledPosition,
                                    ref normal) == HIT_SUN)
                        color += attenuation * Vector(500, 400, 100) * incidence;
                }
                if (hitType == HIT_SUN)
                { //
                    color += attenuation * Vector(50, 80, 100); break; // Sun Color
                }
            }
            return color;
        }

        static void Main(string[] args)
        {
            //const int w = 960; //8; 
            //const int h = 540; //8; 
            const int w = 96 * 4; //8; 
            const int h = 54 * 4; //8; 

            const int samplesCount = 256; //8; 
            var position = Vector(-22, 5, 25);
            var goal = !(Vector(-3, 4, 0) + position * -1);
            var left = !Vector(goal.Z, 0, -goal.X) * (1.0f / w);

            var framework = Assembly
                      .GetEntryAssembly()?
                      .GetCustomAttribute<TargetFrameworkAttribute>()
                      ?.FrameworkName;
            Console.WriteLine($"C# Code - {framework ?? "Unknown"}");

            // Cross-product to get the up vector
            var up = Vector(
                goal.Y * left.Z - goal.Z * left.Y,
                goal.Z * left.X - goal.X * left.Z,
                goal.X * left.Y - goal.Y * left.X);

            var fileName = Path.Combine(Path.GetTempPath(), "output.ppm");
            Console.WriteLine($"Width = {w}, Height = {h}, Samples = {samplesCount}");
            Console.WriteLine($"Writing data to {fileName}", fileName);

            if (File.Exists(fileName))
                File.Delete(fileName);
            using var fileStream = File.Open(fileName, FileMode.CreateNew, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes($"P6 {w} {h} 255 ")); // trailing space!!!
            for (var y = (h - 1); y >= h / 4 * 3; y--)
            {
                for (var x = (w - 1); x >= w / 4 * 3; x--)
                {
                    Vector color = 0;
                    for (var p = samplesCount - 1; p >= 0; p--)
                    {
                        color += Trace(position, !(goal + left * (x - w / 2 + randomVal()) + up * (y - h / 2 + randomVal())));
                    }

                    // Reinhard tone mapping
                    color = color * (1.0f / samplesCount) + 14.0f / 241;
                    var o = color + 1;
                    color = Vector(color.X / o.X, color.Y / o.Y, color.Z / o.Z) * 255;

                    writer.Write(new[] { (byte)(int)color.X, (byte)(int)color.Y, (byte)(int)color.Z });
                }
            }
        }
    }
}
