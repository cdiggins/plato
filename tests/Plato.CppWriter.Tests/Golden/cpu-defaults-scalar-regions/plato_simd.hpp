// simd: AVX2 kernels for records of three or four doubles (option PLATO_SIMD; the build needs /arch:AVX2).
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.
//
// The writer's --cpp-simd-kernels switch writes the functions in Policies/SimdKernels.cs (Lerp and Add on a record of
// four doubles; Add, Subtract, Multiply, Lerp, Dot, Cross and Length on records of three) as `#if PLATO_SIMD` calls
// into these templates, with the generated scalar body in the `#else` branch, so one tree serves both builds.
// With PLATO_SIMD_FMA 0 every kernel performs the scalar body's operations lane by lane in the same order and returns
// the same bits. With PLATO_SIMD_FMA 1 (the default when PLATO_SIMD is on) a multiply and the add that follows it are
// fused with one rounding, which moves the last bit: that is allowed under the stated-tolerance regime of 2026-11-07
// (plato-brep docs/plans/cpp-simd-under-tolerance-2026-11-07.md) and checked by mesh_compare.py, not by identity.
#if PLATO_SIMD

    // A record of four doubles (HomogeneousPoint3D) or three (Vector3D, Point3D) as one 256-bit lane set. The record is
    // its consecutive double fields from the first; the static_asserts hold the writer's table to that layout.
    template <class P> inline __m256d simd_load4(const P& p)
    {
        static_assert(sizeof(P) == 4 * sizeof(double) && std::is_trivially_copyable<P>::value, "a record of four doubles");
        return _mm256_loadu_pd(reinterpret_cast<const double*>(&p));
    }
    template <class P> inline P simd_store4(__m256d v)
    {
        static_assert(sizeof(P) == 4 * sizeof(double), "a record of four doubles");
        P p;
        _mm256_storeu_pd(reinterpret_cast<double*>(&p), v);
        return p;
    }
    inline __m256i simd_mask3() { return _mm256_setr_epi64x(-1, -1, -1, 0); }
    template <class P> inline __m256d simd_load3(const P& p)
    {
        static_assert(sizeof(P) == 3 * sizeof(double) && std::is_trivially_copyable<P>::value, "a record of three doubles");
        return _mm256_maskload_pd(reinterpret_cast<const double*>(&p), simd_mask3());   // never reads past the record
    }
    template <class P> inline P simd_store3(__m256d v)
    {
        static_assert(sizeof(P) == 3 * sizeof(double), "a record of three doubles");
        P p;
        _mm256_maskstore_pd(reinterpret_cast<double*>(&p), simd_mask3(), v);
        return p;
    }

#if PLATO_SIMD_FMA
    inline __m256d simd_fmadd(__m256d a, __m256d b, __m256d c) { return _mm256_fmadd_pd(a, b, c); }   // a*b + c, one rounding
    inline __m256d simd_fmsub(__m256d a, __m256d b, __m256d c) { return _mm256_fmsub_pd(a, b, c); }   // a*b - c, one rounding
    inline double simd_fma(double a, double b, double c) { return std::fma(a, b, c); }
#else
    inline __m256d simd_fmadd(__m256d a, __m256d b, __m256d c) { return _mm256_add_pd(_mm256_mul_pd(a, b), c); }
    inline __m256d simd_fmsub(__m256d a, __m256d b, __m256d c) { return _mm256_sub_pd(_mm256_mul_pd(a, b), c); }
    inline double simd_fma(double a, double b, double c) { return a * b + c; }
#endif

    // Lerp(a, b, t) = a*(1-t) + b*t per component (bspline-evaluation.library.plato:50): the de Boor level step.
    template <class R, class A, class B> inline R simd_lerp4(const A& a, const B& b, double t)
    {
        const __m256d s = _mm256_mul_pd(simd_load4(a), _mm256_set1_pd(1.0 - t));
        return simd_store4<R>(simd_fmadd(simd_load4(b), _mm256_set1_pd(t), s));
    }
    template <class R, class A, class B> inline R simd_lerp3(const A& a, const B& b, double t)
    {
        const __m256d s = _mm256_mul_pd(simd_load3(a), _mm256_set1_pd(1.0 - t));
        return simd_store3<R>(simd_fmadd(simd_load3(b), _mm256_set1_pd(t), s));
    }
    template <class R, class A, class B> inline R simd_add4(const A& a, const B& b) { return simd_store4<R>(_mm256_add_pd(simd_load4(a), simd_load4(b))); }
    template <class R, class A, class B> inline R simd_add3(const A& a, const B& b) { return simd_store3<R>(_mm256_add_pd(simd_load3(a), simd_load3(b))); }
    template <class R, class A, class B> inline R simd_sub3(const A& a, const B& b) { return simd_store3<R>(_mm256_sub_pd(simd_load3(a), simd_load3(b))); }
    template <class R, class A> inline R simd_scale3(const A& a, double s) { return simd_store3<R>(_mm256_mul_pd(simd_load3(a), _mm256_set1_pd(s))); }

    // Dot is a reduction of three products: the lanes add nothing a scalar sum does not, so it is written as the scalar
    // body's sum (x*x' + y*y') + z*z', fused under PLATO_SIMD_FMA. Length is its square root.
    template <class A, class B> inline double simd_dot3(const A& a, const B& b)
    {
        static_assert(sizeof(A) == 3 * sizeof(double) && sizeof(B) == 3 * sizeof(double), "records of three doubles");
        const double* u = reinterpret_cast<const double*>(&a);
        const double* v = reinterpret_cast<const double*>(&b);
#if PLATO_SIMD_FMA
        return simd_fma(u[2], v[2], simd_fma(u[1], v[1], u[0] * v[0]));
#else
        return ((u[0] * v[0]) + (u[1] * v[1])) + (u[2] * v[2]);
#endif
    }
    template <class A> inline double simd_length3(const A& a) { return std::sqrt(simd_dot3(a, a)); }

    // Cross(a, b) = (a.y*b.z - a.z*b.y, a.z*b.x - a.x*b.z, a.x*b.y - a.y*b.x): two lane permutations each side, one
    // multiply and one fused multiply-subtract.
    template <class R, class A, class B> inline R simd_cross3(const A& a, const B& b)
    {
        const __m256d va = simd_load3(a), vb = simd_load3(b);
        const __m256d a_yzx = _mm256_permute4x64_pd(va, _MM_SHUFFLE(3, 0, 2, 1));
        const __m256d a_zxy = _mm256_permute4x64_pd(va, _MM_SHUFFLE(3, 1, 0, 2));
        const __m256d b_yzx = _mm256_permute4x64_pd(vb, _MM_SHUFFLE(3, 0, 2, 1));
        const __m256d b_zxy = _mm256_permute4x64_pd(vb, _MM_SHUFFLE(3, 1, 0, 2));
        return simd_store3<R>(simd_fmsub(a_yzx, b_zxy, _mm256_mul_pd(a_zxy, b_yzx)));
    }

#endif
