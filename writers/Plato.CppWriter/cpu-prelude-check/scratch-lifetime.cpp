#include "plato_cpu.hpp"
#include <cstdio>
#include <thread>
#include <vector>
#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#endif

static std::vector<int> destroyed;
struct Tracked
{
    std::string text = std::string(256, 'x');
    int id = 0;
    ~Tracked() { destroyed.push_back(id); }
};

#define CHECK(c) do { if (!(c)) { std::printf("FAIL %d: %s\n", __LINE__, #c); return 1; } } while (0)
int main(int argc, char** argv)
{
#ifdef _WIN32
    SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
    _set_error_mode(_OUT_TO_STDERR);
    _set_abort_behavior(0, _CALL_REPORTFAULT);
#endif
    using namespace plato;
    if (argc == 2 && std::string(argv[1]) == "overflow")
    {
        arena_alloc_bytes(std::numeric_limits<size_t>::max());
        return 0;
    }
    if (argc == 2 && std::string(argv[1]) == "expired-mark")
    {
        auto mark = arena_mark();
        arena_reset();
        arena_rollback(mark);
        return 0;
    }
    auto* retained = arena_alloc<Tracked>(1);
    retained->id = 1;
    const auto outer = arena_mark();
    auto* row = arena_alloc<Tracked>(2);
    row[0].id = 2;
    row[1].id = 3;
    const auto inner = arena_mark();
    arena_alloc<unsigned char>(5 * 1024 * 1024);
    arena_alloc<Tracked>(1)->id = 4;
    arena_rollback(inner);
    CHECK(destroyed == std::vector<int>({4}));
    CHECK(arena_bytes() == inner.bytes && row[0].text.size() == 256);
    arena_rollback(outer);
    CHECK(destroyed == std::vector<int>({4, 3, 2}));
    CHECK(retained->text.size() == 256 && arena_bytes() == outer.bytes);
    arena_reset();
    CHECK(destroyed == std::vector<int>({4, 3, 2, 1}));
    CHECK(arena_state().cleanups.empty() && arena_bytes() == 0);
    for (int i = 0; i < 100; ++i)
    {
        auto mark = arena_mark();
        arena_alloc<Tracked>(1)->id = i;
        arena_rollback(mark);
        CHECK(destroyed.back() == i && arena_state().cleanups.empty());
    }
    const auto count = destroyed.size();
    std::thread worker([] { arena_alloc<Tracked>(1)->id = 500; });
    worker.join();
    CHECK(destroyed.size() == count + 1 && destroyed.back() == 500);
    std::puts("scratch lifetime gate passed");
    return 0;
}
