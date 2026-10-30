// Hand-written check of plato_cpu.hpp (TKT-1097 chunk A). Expected values are what .NET gives.
#include "plato_cpu.hpp"
#include <cstdio>

using namespace plato;

static int failures = 0;
#define CHECK(c) do { if (!(c)) { std::printf("FAIL line %d: %s\n", __LINE__, #c); ++failures; } } while (0)

struct Pt { double X; double Y; };
inline void from_json(const Json& j, Pt& p) { from_json(j["X"], p.X); from_json(j["y"], p.Y); }  // "y": case-insensitive read
inline Json to_json(const Pt& p) { Json j = Json::Object(); j.set("X", to_json(p.X)); j.set("Y", to_json(p.Y)); return j; }

int main()
{
    // Arena
    arena_reset();
    double* a = arena_alloc<double>(10);
    CHECK(reinterpret_cast<uintptr_t>(a) % 16 == 0);
    CHECK(arena_bytes() == 80);
    int32_t* b = arena_alloc<int32_t>(3);
    CHECK(reinterpret_cast<uintptr_t>(b) % 16 == 0 && arena_bytes() == 96);
    size_t peak = arena_peak_bytes();
    arena_reset();
    CHECK(arena_bytes() == 0 && arena_peak_bytes() == peak);
    CHECK(arena_alloc<double>(1) == a);   // reset reuses the memory
    arena_reset();
    unsigned char* big = arena_alloc<unsigned char>(5 * 1024 * 1024);   // bigger than the first block
    CHECK(big != nullptr);
    arena_reset();

    // Range, Map, Reduce, FlatMap, Zip
    Array<int32_t> r = Range(5);
    CHECK(ToCount(r) == 5 && At(r, 4) == 4);
    CHECK(ToCount(Range(-3)) == 0);
    Array<double> sq = Map(r, [](int32_t i) { return i * 1.5; });
    CHECK(ToCount(sq) == 5 && At(sq, 3) == 4.5);
    CHECK(Reduce(r, 0, [](int32_t acc, int32_t x) { return acc + x * x; }) == 30);
    Array<int32_t> fm = FlatMap(r, [](int32_t i) { return Range(i); });   // 0 + 01 + 012 + 0123 = 10 items
    CHECK(ToCount(fm) == 10 && At(fm, 9) == 3 && At(fm, 1) == 0 && At(fm, 3) == 0);
    Array<double> z = Zip(r, sq, [](int32_t i, double d) { return i + d; });
    CHECK(ToCount(z) == 5 && At(z, 2) == 5.0);
    Array<double> lit = make_array(1.0, 2.0, 3.0);
    Array<double> cat = Concatenate(lit, Reverse(lit));
    CHECK(ToCount(cat) == 6 && At(cat, 2) == 3.0 && At(cat, 3) == 3.0 && At(cat, 5) == 1.0);
    CHECK(ToCount(Append(lit, 4.0)) == 4 && At(Prepend(lit, 0.0), 0) == 0.0);
    CHECK(IndexOf(lit, 2.0) == 1 && !Contains(lit, 9.0) && ToCount(Skip(Take(lit, 2), 1)) == 1);
    CHECK(lit == make_array(1.0, 2.0, 3.0));
    const double raw[] = { 7.0, 8.0 };
    CHECK(At(make_array(raw, 2), 1) == 8.0);
    PlatoList<int32_t> l = EmptyList(r);
    for (int i = 0; i < 20; i++) l = Add(l, i);
    CHECK(ToCount(l) == 20 && At(Freeze(l), 19) == 19);
    PlatoBuffer<double> buf = make_buffer<double>(3);
    buf = Set(buf, 1, 2.5);
    CHECK(At(Freeze(buf), 0) == 0.0 && At(buf, 1) == 2.5);

    // Modulo, Round, Floor, Hash, ToInteger against .NET
    CHECK(Modulo(-7.5, 2.0) == -1.5 && Modulo(7.5, -2.0) == 1.5);   // -7.5 % 2 = -1.5 in C#
    CHECK(Modulo(-7, 3) == -1 && Modulo(7, -3) == 1);
    CHECK(Floor(-0.5) == -1.0 && Floor(2.7) == 2.0);
    CHECK(ToRound(0.5, 0) == 0.0 && ToRound(1.5, 0) == 2.0 && ToRound(2.5, 0) == 2.0 && ToRound(-2.5, 0) == -2.0);   // ToEven
    CHECK(ToRound(2.675, 2) == 2.68);   // 2.675 * 100 is exactly 267.5 in double; ToEven gives 268 (as Math.Round does)
    CHECK(ToRound(1.005, 2) == 1.0 && ToRound(0.125, 2) == 0.12);
    CHECK(ToInteger(3.99) == 3 && ToInteger(-3.99) == -3 && ToInteger(1e20) == INT32_MAX && ToInteger(std::nan("")) == 0);
    CHECK(ShiftLeft(1, 33) == 2 && ShiftRight(-8, 1) == -4);
    CHECK(Hash(0.0) == Hash(-0.0) && Hash(1.0) == 1072693248);   // 0x3FF0000000000000 -> 0x3FF00000 ^ 0
    CHECK(Atan2Radians(1.0, 1.0) > 0.785398 && Atan2Radians(1.0, 1.0) < 0.785399);

    // ToString
    CHECK(ToString(1.5) == "1.5" && ToString(100.0) == "100" && ToString(0.1) == "0.1");
    CHECK(ToString(1e15) == "1E+15" && ToString(123456789012345.0) == "123456789012345");
    CHECK(ToString(0.0001) == "0.0001" && ToString(0.00001) == "1E-05");
    CHECK(ToString(-1.2345e-7) == "-1.2345E-07" && ToString(1.2345678901234568e17) == "1.2345678901234568E+17");
    CHECK(ToString(1.0 / 3.0) == "0.3333333333333333");
    CHECK(ToString(std::nan("")) == "NaN" && ToString(-HUGE_VAL) == "-Infinity" && ToString(-42) == "-42");

    // Text intrinsics
    CHECK(FromCharacters(make_array('a', 'b')) == "ab" && FromCharacters(make_array_empty<char>()) == "");

    CHECK(Substring("abcd", 1, 2) == "bc" && Substring("abcd", -1, 3) == "ab" && Substring("abcd", 3, 9) == "d");
    CHECK(Substring("abcd", 0, 0) == "" && Substring("abcd", 0, -5) == "" && Substring("abcd", 4, 1) == "" && Substring("abcd", 9, 1) == "");
    CHECK(Substring("abcd", INT32_MIN, INT32_MAX) == "" && Substring("abcd", -1, INT32_MAX) == "abcd" && Substring("abcd", 2, INT32_MAX) == "cd" && Substring("", 0, 1) == "");

    CHECK(IsNumberText("1.") && IsNumberText(".5") && IsNumberText("1.E-06") && IsNumberText("-0") && IsNumberText("+7") && IsNumberText("12e3") && IsNumberText("0.1E+2"));
    CHECK(!IsNumberText("") && !IsNumberText(".") && !IsNumberText("-") && !IsNumberText("1e") && !IsNumberText("1e+") && !IsNumberText(".e1") && !IsNumberText("NaN")
        && !IsNumberText("Infinity") && !IsNumberText("0x10") && !IsNumberText(" 1") && !IsNumberText("1 ") && !IsNumberText("1,5") && !IsNumberText("1.5.2") && !IsNumberText("1e999") && !IsNumberText("--1"));
    CHECK(NumberFromText("1.5E1") == 15.0 && NumberFromText("abc") == 0.0 && NumberFromText(".5") == 0.5 && NumberFromText("1.") == 1.0 && NumberFromText("1.E-06") == 1e-6 && NumberFromText("+2.5") == 2.5);
    CHECK(std::signbit(NumberFromText("-0")) && std::signbit(NumberFromText("-0.0E5")) && !std::signbit(NumberFromText("0")));
    CHECK(NumberFromText("0.1") == 0.1 && NumberFromText("123456789012345678901234567890") == 1.2345678901234568e29 && NumberFromText("4.9e-324") == 4.9406564584124654e-324);
    CHECK(NumberFromText("1e-400") == 0.0 && NumberFromText("2.2250738585072011e-308") == 2.2250738585072009e-308 && NumberFromText("1.7976931348623157e308") == 1.7976931348623157e308);

    // Json: Array<double> and a nested object
    arena_reset();
    Array<double> xs = make_array(0.1, -2.5, 1e20, 3.0);
    std::string text = json_dump(to_json(xs));
    CHECK(text == "[0.1,-2.5,1E+20,3]");
    Json parsed;
    CHECK(json_parse(text, parsed));
    Array<double> back{};
    from_json(parsed, back);
    CHECK(back == xs);

    const char* doc = "{\"name\":\"a\\\"b\\n\\u00e9\",\"pts\":[{\"X\":1,\"Y\":2},{\"x\":3,\"Y\":-4.5}],\"ok\":true,\"none\":null,\"inf\":\"-Infinity\"}";
    Json d;
    std::string err;
    CHECK(json_parse(doc, d, &err));
    CHECK(d["name"].as_string() == "a\"b\n\xC3\xA9");
    Array<Pt> pts{};
    from_json(d["pts"], pts);
    CHECK(ToCount(pts) == 2 && pts.data[1].X == 3.0 && pts.data[1].Y == -4.5);   // member "x" read as X
    CHECK(d["ok"].as_bool() && d["none"].is_null() && std::isinf(d["inf"].as_number()) && d["missing"].is_null());
    std::string again = json_dump(d);
    Json d2;
    CHECK(json_parse(again, d2) && json_dump(d2) == again);
    CHECK(!json_parse("{\"a\":", d2, &err) && !json_parse("[1,]", d2) && !json_parse("1 2", d2));
    Json round = to_json(pts);
    CHECK(json_dump(round) == "[{\"X\":1,\"Y\":2},{\"X\":3,\"Y\":-4.5}]");

    std::printf(failures == 0 ? "OK\n" : "%d FAILURES\n", failures);
    return failures == 0 ? 0 : 1;
}
