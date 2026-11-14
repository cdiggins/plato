// Native check of String as a view and of its constants (TKT-1159, TKT-1223). Built by StringLiteralTests with
// PLATO_ARENA_FAST 0 and 1; must exit 0.
#include <cstdio>
#include "plato_cpu.hpp"
#include <thread>
#include <vector>

#define CHECK(c) do { if (!(c)) { std::printf("FAIL %d: %s\n", __LINE__, #c); return 1; } } while (0)

// A String is a plain view, and so is a record of them: copied bit for bit, never destroyed.
struct Named { plato::String name; double x; };
static_assert(std::is_trivially_copyable<plato::String>::value && std::is_trivially_destructible<plato::String>::value, "String");
static_assert(std::is_trivially_copyable<Named>::value && std::is_trivially_destructible<Named>::value, "record of String");
static_assert(sizeof(plato::String) == 16, "a pointer and a length");
// The default String is empty, so a value-initialized record holds an empty one; that initializer is why String is
// not trivially default-constructible, and why arena_alloc builds such records unless PLATO_ARENA_UNINIT is on.
static_assert(!std::is_trivially_default_constructible<plato::String>::value, "String() is the empty String");
// A constant is built at compile time.
static_assert(PLATO_STR(u"abc", 3).size() == 3 && PLATO_STR(u"abc", 3)[1] == u'b' && plato::String(u"abc").size() == 3 && plato::String().empty(), "constexpr");

static long long builds() { return plato::plato_make_string_count; }

static plato::String site() { return PLATO_STR(u"caf\x00E9", 4); }
static plato::String site_nul() { return PLATO_STR(u"a\x0000" u"b", 3); }

// The string pool as the writer emits it (plato_strings.cpp): a namespace-scope table of views of literals.
static const plato::String pool[2] = {
    plato::String(u"pooled", 6), // 0
    plato::String(u"\x03A9", 1), // 1
};

int main()
{
    using namespace plato;

    // PLATO_STR: the units are the literal's, in static storage; nothing is built or allocated, at any evaluation.
    CHECK(site() == u"café" && site().size() == 4);
    const long long before = builds();
    const size_t bytes = arena_bytes();
    for (int i = 0; i < 1000; ++i) CHECK(site().size() == 4);
    CHECK(builds() == before && arena_bytes() == bytes);
    CHECK(site().data() == site().data());                    // one array per site
    CHECK(site_nul().size() == 3 && site_nul()[1] == 0 && site_nul()[2] == u'b');   // embedded NUL kept
    CHECK(PLATO_STR(u"", 0).empty() && PLATO_STR(u"", 0) == String() && String(u"").data() == nullptr);

    // make_string_n: the same text from UTF-8, in the arena, counted by the probe.
    const String made = make_string_n("caf\xC3\xA9", 5);
    CHECK(made == site() && made.data() != site().data() && builds() == before + 1 && arena_bytes() > bytes);
    CHECK(make_string_n("a\0b", 3) == site_nul() && make_string_n("", 0).empty());

    // Comparison and hashing read the units, never the pointer.
    const char16_t host[] = { u'c', u'a', u'f', 0xe9, u'!' };
    const String viewed(host, 4);
    CHECK(viewed == site() && !(viewed != site()) && HashString(viewed) == HashString(site()) && GetHashCode(viewed) == GetHashCode(made));
    CHECK(String(u"ab") < String(u"abc") && String(u"abc") > String(u"ab") && String(u"B") < String(u"a") && String(u"b") >= String(u"b") && String(u"a") <= String(u"b"));
    CHECK(String(u"ab").compare(String(u"ab")) == 0 && String(u"ab").compare(String(u"b")) < 0 && String().compare(String(u"a")) < 0 && String(u"a").compare(String()) > 0);
    CHECK(String(host, 5) != site() && String() == String(u"") && String() != String(u"a"));
    int32_t count = 0;
    for (char16_t c : site()) count += c != 0;
    CHECK(count == 4 && !site().empty() && *site().begin() == u'c' && site().end() - site().begin() == 4);

    // Substring and FromCharacters share the units; concatenation and the text forms allocate in the arena.
    const String whole = site() + String(u" au lait");
    CHECK(whole == u"café au lait" && whole.size() == 12);
    CHECK(Substring(whole, 5, 2).data() == whole.data() + 5 && Substring(whole, 5, 2) == u"au");
    const Array<char16_t> letters = make_array(u'x', u'y', u'z');
    CHECK(FromCharacters(letters).data() == letters.data && FromCharacters(letters) == u"xyz");
    CHECK((site() + String()).data() == site().data() && (String() + site()).data() == site().data() && (String() + String()).empty());
    const String copied = string_copy(site());
    CHECK(copied == site() && copied.data() != site().data() && string_copy(String()).empty());
    CHECK(ToString(true) == u"True" && ToString(false) == u"False" && ToString(u'q') == u"q" && ToString(-12) == u"-12" && ToString(0.5) == u"0.5");
    CHECK(ToText(make_bytes("ab\xff", 3)) == String(u"ab\x00FF", 3) && ToText(Bytes{}).empty());
    CHECK(StringFromUtf8("\xf0\x9f\x98\x80") == u"\U0001f600" && StringFromUtf8("\xf0\x9f\x98\x80").size() == 2 && StringFromUtf8("").empty());
    String built;
    string_append_char(&built, u'n'); string_append_cstr(&built, "=\xce\xa9"); string_append_int(&built, 7); string_append_float(&built, 1.5);
    CHECK(built == u"n=Ω" u"71.5");
    string_clear(&built);
    CHECK(built.empty());

    // A record of Strings in the arena: written before it is read, with or without the constructor having run.
    Named* rows = arena_alloc<Named>(3);
    for (int i = 0; i < 3; ++i) { rows[i].name = i == 0 ? site() : whole; rows[i].x = i; }
    Named moved = rows[2];
    CHECK(moved.name == whole && moved.name.data() == whole.data() && rows[0].name == site());
    // make_buffer value-initializes: every String empty.
    const Buffer<Named> blank = make_buffer<Named>(4);
    for (int i = 0; i < 4; ++i) CHECK(blank.items[i].name.empty() && blank.items[i].name.data() == nullptr && blank.items[i].x == 0);

    // The lifetime rule. Constants survive an arena reset: a literal, a PLATO_STR site, a pool entry, and a view of
    // host storage. An arena String does not: its units are reused by what is allocated next, which is why a host that
    // keeps one across a reset copies it out first.
    const String literal = u"literal";
    const std::u16string kept(whole.data(), whole.size());    // the host's deep copy
    arena_reset();
    CHECK(arena_bytes() == 0);
    char16_t* reused = string_alloc(64);
    for (int i = 0; i < 64; ++i) reused[i] = u'#';
    CHECK(reused == made.data() && made == u"####");          // the arena handed the first String's units out again
    CHECK(literal == u"literal" && site() == u"café" && site_nul().size() == 3 && pool[0] == u"pooled" && pool[1][0] == 0x3a9);
    CHECK(viewed == site() && ToString(true) == u"True");
    CHECK(String(kept.data(), kept.size()) == u"café au lait");

    // Constants are shared by threads; each thread's own Strings are in its own arena.
    std::vector<std::thread> ts;
    std::vector<int> bad(8, 0);
    for (int t = 0; t < 8; ++t)
        ts.emplace_back([&, t] {
            for (int i = 0; i < 1000; ++i)
            {
                if (PLATO_STR(u"shared", 6) != u"shared" || pool[0] != u"pooled") bad[t] = 1;
                if (PLATO_STR(u"shared", 6) + ToString(t) != String(u"shared") + ToString(t)) bad[t] = 1;
            }
            arena_reset();
        });
    for (auto& t : ts) t.join();
    for (int b : bad) CHECK(b == 0);
    std::printf("ok fast=%d\n", PLATO_ARENA_FAST);
    return 0;
}
