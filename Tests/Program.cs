using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace PlatoTest
{
    [SimpleJob(warmupCount:5, launchCount:2, targetCount: 10)]
    public class Benchmarks
    {
        [Benchmark]
        public float[] Torus()
            => Extensions.Torus(500, 100, 1, 0.2f).ToTriMesh().FaceNormals().ToFloatArray();
    }

    public static class Program
    { 
        public static void Main()
        {
            var _ = BenchmarkRunner.Run<Benchmarks>();
        }
    }

}