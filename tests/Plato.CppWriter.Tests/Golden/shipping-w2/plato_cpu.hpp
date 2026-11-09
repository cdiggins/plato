// plato_cpu.hpp: the CPU prelude of the Plato C++ writer, dialect cpu64i32 (TKT-1097).
// Mirrors the C# runtime (Intrinsics/*.cs of Ara3D.StepBrep) name for name: Number is double,
// Integer is int32_t, Boolean is bool, Character is char16_t, String is std::u16string, and an
// Array<T> is an immutable view into a thread-local bump arena. No exceptions, no RTTI, no iostream.
//
// The runtime is these fragments (plato_core.hpp, plato_arena.hpp, ...), included below in order inside
// namespace plato; its options are the PLATO_* macros of plato_config.hpp, each overridable with -D.
#pragma once
#define PLATO_ARENA_LIFETIMES 1
#include "plato_config.hpp"

#include <cassert>
#include <charconv>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <limits>
#include <new>
#include <string>
#include <type_traits>
#include <vector>
#if PLATO_SIMD
#include <immintrin.h>
#endif

#ifndef PLATO_ASSERT
#define PLATO_ASSERT(c) assert(c)
#endif

namespace plato
{
#include "plato_core.hpp"
#include "plato_arena.hpp"
#include "plato_array.hpp"
#include "plato_builders.hpp"
#include "plato_scalar.hpp"
#include "plato_simd.hpp"
#include "plato_string.hpp"
#include "plato_bytes.hpp"
#include "plato_helpers.hpp"
#include "plato_tostring.hpp"
#include "plato_json.hpp"

}
