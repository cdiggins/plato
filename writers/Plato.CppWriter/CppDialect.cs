namespace Ara3D.Geometry.CppWriter
{
    /// <summary>
    /// The two C++ family targets. The emitted function bodies are IDENTICAL for both:
    /// the dialects differ only in the preamble (which supplies the vector types and the
    /// PLATO_FN function qualifier) and in the output file name.
    /// </summary>
    public enum CppDialect
    {
        /// <summary>Portable C++17. The preamble defines float2/float3/float4 itself.</summary>
        Cpp,

        /// <summary>CUDA C++ (nvcc). float2/float3/float4 and make_floatN come from cuda_runtime.h;
        /// every emitted function is qualified __host__ __device__.</summary>
        Cuda,
    }

    public static class CppDialectExtensions
    {
        public static string FileName(this CppDialect d)
            => d == CppDialect.Cuda ? "plato.cu" : "plato.hpp";

        public static string DisplayName(this CppDialect d)
            => d == CppDialect.Cuda ? "CUDA" : "C++";
    }
}
