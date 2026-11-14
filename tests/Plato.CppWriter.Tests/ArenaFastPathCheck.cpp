// Native check of the arena fast path (TKT-1158). Built by ArenaFastPathTests with PLATO_ARENA_FAST 0 and 1; both must pass.
#include "plato_cpu.hpp"
#include <cstdio>
#include <thread>
#include <vector>

struct Rec { double x = 7, y = 8; };      // trivially copyable, not trivially default constructible
struct Named { plato::String name; double x; };
static_assert(std::is_trivially_copyable<Named>::value && std::is_trivially_destructible<Named>::value, "a record of Strings is plain data");
static int destroyed = 0;
struct Counted { int id = 0; ~Counted() { destroyed++; } };

#define CHECK(c) do { if (!(c)) { std::printf("FAIL %d: %s\n", __LINE__, #c); return 1; } } while (0)

static int on_thread()
{
    using namespace plato;
    double* p = arena_alloc<double>(3);
    CHECK(p != nullptr);
    CHECK(reinterpret_cast<uintptr_t>(p) % 16 == 0);
    CHECK(arena_bytes() == 32);
    return 0;
}

int main()
{
    using namespace plato;
    CHECK(arena_bytes() == 0 && arena_peak_bytes() == 0);

    // alignment, byte counts, zero and negative sizes
    CHECK(arena_alloc<double>(0) == nullptr && arena_alloc<double>(-1) == nullptr);
    double* a = arena_alloc<double>(3);
    CHECK(reinterpret_cast<uintptr_t>(a) % 16 == 0);
    CHECK(arena_bytes() == 32);
    a[0] = 1; a[1] = 2; a[2] = 3;

    // peak is exact across rollback: grow, mark, grow more, roll back, peak still the high-water mark
    const ArenaMark m = arena_mark();
    for (int i = 0; i < 1000; i++) arena_alloc<double>(5)[0] = i;
    const size_t high = arena_bytes();
    CHECK(high == 32 + 1000 * 48);
    arena_rollback(m);
    CHECK(arena_bytes() == 32);
    CHECK(arena_peak_bytes() == high);
    arena_alloc<double>(1);
    CHECK(arena_peak_bytes() == high);

    // blocks: allocations larger than a block, and many blocks, keep their contents
    std::vector<double*> ptrs;
    for (int i = 0; i < 5000; i++)
    {
        double* q = arena_alloc<double>(1024);
        q[0] = i; q[1023] = -i;
        ptrs.push_back(q);
    }
    double* big = arena_alloc<double>(1 << 22);
    big[0] = 5; big[(1 << 22) - 1] = 6;
    for (int i = 0; i < 5000; i++) CHECK(ptrs[i][0] == i && ptrs[i][1023] == -i);
    CHECK(a[0] == 1 && a[2] == 3 && big[0] == 5 && big[(1 << 22) - 1] == 6);
    const size_t high2 = arena_bytes();
    CHECK(arena_peak_bytes() == high2);

    // records: writing before reading works whether or not the constructor ran; a non-trivial type is always built and destroyed
    Rec* r = arena_alloc<Rec>(4);
    for (int i = 0; i < 4; i++) { r[i].x = i; r[i].y = -i; }
    for (int i = 0; i < 4; i++) CHECK(r[i].x == i && r[i].y == -i);
    // a record of Strings is such a record: String is a view, trivially copyable, so with PLATO_ARENA_UNINIT its
    // constructor does not run and every member is written before it is read
    Named* named = arena_alloc<Named>(4);
    for (int i = 0; i < 4; i++) { named[i].name = i % 2 ? String(u"odd") : String(); named[i].x = i; }
    for (int i = 0; i < 4; i++) CHECK(named[i].name == (i % 2 ? String(u"odd") : String()) && named[i].x == i);
    {
        const ArenaMark m2 = arena_mark();
        Counted* c = arena_alloc<Counted>(5);
        for (int i = 0; i < 5; i++) CHECK(c[i].id == 0);
        arena_rollback(m2);
        CHECK(destroyed == 5);
    }

    // reset keeps the peak and starts again from the first block
    arena_reset();
    CHECK(arena_bytes() == 0);
    CHECK(arena_peak_bytes() >= high2);
    double* z = arena_alloc<double>(2);
    CHECK(arena_bytes() == 16 && z != nullptr);

    // a second thread has its own state
    int rc = 0;
    std::thread t([&] { rc = on_thread(); });
    t.join();
    CHECK(rc == 0);
    CHECK(arena_bytes() == 16);

    // overflow of the size aborts: checked by the existing scratch-lifetime check
    std::printf("PASS fast=%d\n", PLATO_ARENA_FAST);
    return 0;
}
