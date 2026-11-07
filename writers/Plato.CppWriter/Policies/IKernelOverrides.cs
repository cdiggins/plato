// Kernel overrides: a generated function whose body a prelude kernel may replace behind a macro.
using System.Collections.Generic;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Decides whether an emitted function's definition is written twice, as a prelude kernel call under a macro and the
    /// generated body otherwise. <see cref="Call"/> returns the call expression (the kernel applied to the parameter
    /// names) or null when the function keeps its body alone.
    /// </summary>
    public interface IKernelOverrides
    {
        /// <summary>The preprocessor condition the kernel branch is written under (for example <c>PLATO_SIMD</c>).</summary>
        string Condition { get; }
        string Call(string name, IReadOnlyList<string> parameterTypes, string returnType, IReadOnlyList<string> parameterNames);
    }

    /// <summary>The current behaviour: every function keeps its generated body.</summary>
    public sealed class NoKernelOverrides : IKernelOverrides
    {
        public string Condition => null;
        public string Call(string name, IReadOnlyList<string> parameterTypes, string returnType, IReadOnlyList<string> parameterNames) => null;
    }

    /// <summary>
    /// --cpp-simd-kernels (2026-11-07, plato-brep docs/plans/cpp-simd-under-tolerance-2026-11-07.md): the arithmetic of
    /// records of four doubles (HomogeneousPoint3D: the de Boor level step Lerp and Add) and of three (Vector3D, Point3D:
    /// Add, Subtract, Multiply by a scalar, Lerp, Dot, Cross, Length) is also written as a call into the AVX2 kernels of
    /// prelude/plato_simd.hpp under <c>#if PLATO_SIMD</c>. The kernels are templates over the record types, so the table
    /// names only the Plato records whose layout is consecutive doubles; a record added here must keep that layout, which
    /// the kernels' static_asserts check at compile time.
    /// </summary>
    public sealed class SimdKernels : IKernelOverrides
    {
        private static readonly HashSet<string> Four = new HashSet<string> { "HomogeneousPoint3D" };
        private static readonly HashSet<string> Three = new HashSet<string> { "Vector3D", "Point3D" };

        public string Condition => "PLATO_SIMD";

        public string Call(string name, IReadOnlyList<string> parameterTypes, string returnType, IReadOnlyList<string> parameterNames)
        {
            var p = parameterNames;
            var t = parameterTypes;
            switch (t.Count)
            {
                case 1:
                    if (name == "Length" && Three.Contains(t[0]) && returnType == "double")
                        return $"plato::simd_length3({p[0]})";
                    return null;
                case 2:
                    if (Four.Contains(t[0]) && Four.Contains(t[1]) && Four.Contains(returnType) && name == "Add")
                        return $"plato::simd_add4<{returnType}>({p[0]}, {p[1]})";
                    if (Three.Contains(t[0]) && t[1] == "double" && Three.Contains(returnType) && name == "Multiply")
                        return $"plato::simd_scale3<{returnType}>({p[0]}, {p[1]})";
                    if (!Three.Contains(t[0]) || !Three.Contains(t[1]))
                        return null;
                    switch (name)
                    {
                        case "Add" when Three.Contains(returnType): return $"plato::simd_add3<{returnType}>({p[0]}, {p[1]})";
                        case "Subtract" when Three.Contains(returnType): return $"plato::simd_sub3<{returnType}>({p[0]}, {p[1]})";
                        case "Cross" when Three.Contains(returnType): return $"plato::simd_cross3<{returnType}>({p[0]}, {p[1]})";
                        case "Dot" when returnType == "double": return $"plato::simd_dot3({p[0]}, {p[1]})";
                        default: return null;
                    }
                case 3:
                    if (name != "Lerp" || t[2] != "double")
                        return null;
                    if (Four.Contains(t[0]) && Four.Contains(t[1]) && Four.Contains(returnType))
                        return $"plato::simd_lerp4<{returnType}>({p[0]}, {p[1]}, {p[2]})";
                    if (Three.Contains(t[0]) && Three.Contains(t[1]) && Three.Contains(returnType))
                        return $"plato::simd_lerp3<{returnType}>({p[0]}, {p[1]}, {p[2]})";
                    return null;
                default:
                    return null;
            }
        }
    }
}
