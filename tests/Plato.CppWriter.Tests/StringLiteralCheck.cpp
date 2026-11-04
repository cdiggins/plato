// Native check of PLATO_STR (TKT-1159). Built by StringLiteralTests; must exit 0.
#include <cstdio>
#include "plato_cpu.hpp"
#include <thread>
#include <vector>

#define CHECK(c) do { if (!(c)) { std::printf("FAIL %d: %s\n", __LINE__, #c); return 1; } } while (0)

static long long builds() { return plato::plato_make_string_count; }

static plato::String site() { return PLATO_STR("caf\xC3\xA9", 5); }
static const plato::String& site_ref() { return PLATO_STR("a\0b", 3); }

int main()
{
    using namespace plato;
    CHECK(site() == u"café");                      // UTF-8 C3 A9 decodes to one code unit E9
    CHECK(site().size() == 4);
    const long long before = builds();
    for (int i = 0; i < 1000; ++i) CHECK(site().size() == 4);
    CHECK(builds() == before);                           // built once per site, never again
    CHECK(site_ref().size() == 3 && site_ref()[1] == 0); // embedded NUL kept
    CHECK(&site_ref() == &site_ref());                   // one object
    String copy = site(); copy.push_back(u'!');
    CHECK(site().size() == 4 && copy.size() == 5);       // a copy does not alias the static
    std::vector<std::thread> ts;
    std::vector<int> bad(8, 0);
    for (int t = 0; t < 8; ++t) ts.emplace_back([&, t] { for (int i = 0; i < 1000; ++i) if (PLATO_STR("shared", 6) != u"shared") bad[t] = 1; });
    for (auto& t : ts) t.join();
    for (int b : bad) CHECK(b == 0);
    std::printf("ok\n");
    return 0;
}
