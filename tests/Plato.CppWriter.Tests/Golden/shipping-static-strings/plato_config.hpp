// plato_config.hpp: the defaults of the runtime options of plato_cpu.hpp. CppWriter writes one of these from
// CppCpuOptions; every macro keeps a value given on the command line (cl /DPLATO_BORROW_READS=1).

// Smallest sizeof(T) of a record that ReadOnlyParameter<T> passes by const reference.
#ifndef PLATO_BORROW_MIN_BYTES
#define PLATO_BORROW_MIN_BYTES 256
#endif
// 1: ReadOnlyParameter also borrows every record that is not trivially copyable, whatever its size.
#ifndef PLATO_BORROW_NON_TRIVIAL
#define PLATO_BORROW_NON_TRIVIAL 0
#endif
// 1: array_at_readonly returns const T& (arrays are immutable arena views) instead of ReadOnlyParameter<T>.
#ifndef PLATO_BORROW_READS
#define PLATO_BORROW_READS 0
#endif

// 1: the cheaper arena allocation path (TKT-1158). The parts below default to this value and can be set one by one.
#ifndef PLATO_ARENA_FAST
#define PLATO_ARENA_FAST 0
#endif
// 1: the arena state is reached through a constant-initialized thread_local pointer, so no guard runs per call.
#ifndef PLATO_ARENA_TLS_PTR
#define PLATO_ARENA_TLS_PTR PLATO_ARENA_FAST
#endif
// 1: the peak is folded in when the byte count falls (rollback, reset) and when it is read, not on every allocation.
#ifndef PLATO_ARENA_LAZY_PEAK
#define PLATO_ARENA_LAZY_PEAK PLATO_ARENA_FAST
#endif
// 1: the bump fast path is force-inlined; the block-switching path is a separate noinline function.
#ifndef PLATO_ARENA_INLINE
#define PLATO_ARENA_INLINE PLATO_ARENA_FAST
#endif
// 1: arena_alloc does not default-construct trivially copyable elements (callers write before they read).
#ifndef PLATO_ARENA_UNINIT
#define PLATO_ARENA_UNINIT PLATO_ARENA_FAST
#endif

// 1: the AVX2 kernels of plato_simd.hpp replace the scalar bodies of the functions in Policies/SimdKernels.cs (the tree
// must have been written with --cpp-simd-kernels, and the build must pass /arch:AVX2). 0: the scalar bodies.
#ifndef PLATO_SIMD
#define PLATO_SIMD 0
#endif
// 1: the kernels fuse a multiply with the add that follows it (one rounding; last bits move; the stated-tolerance regime
// of 2026-11-07). 0: lane-wise operations in the scalar body's order, bit-identical to the scalar build.
#ifndef PLATO_SIMD_FMA
#define PLATO_SIMD_FMA PLATO_SIMD
#endif
