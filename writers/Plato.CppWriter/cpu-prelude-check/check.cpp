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

    // Bytes JSON must preserve Latin-1 values, including control bytes and UTF-8 encodings above 127.
    uint8_t everyByte[256];
    for (int i = 0; i < 256; ++i) everyByte[i] = static_cast<uint8_t>(i);
    const auto byteValues = make_array(make_bytes(everyByte, 256), Bytes{});
    const auto byteJson = json_dump(to_json(byteValues));
    Json byteParsed;
    CHECK(json_parse(byteJson, byteParsed));
    Array<Bytes> byteRestored;
    from_json(byteParsed, byteRestored);
    CHECK(byteRestored.count == 2 && byteRestored.data[0] == byteValues.data[0] && byteRestored.data[1].count == 0);
    CHECK(to_json(make_bytes(everyByte + 128, 1)).s == std::string("\xc2\x80"));

    // Match CLR defaults without conflating null and constructed empty arrays.
    Array<int32_t> absent;
    const auto empty = make_array_empty<int32_t>();
    CHECK(json_dump(to_json(absent)) == "null");
    CHECK(json_dump(to_json(empty)) == "[]");
    CHECK(json_dump(to_json(Range(0))) == "[]");
    const auto nested = make_array(absent, empty, Range(2));
    CHECK(json_dump(to_json(nested)) == "[null,[],[0,1]]");
    Json nestedJson;
    CHECK(json_parse("[null,[],[0,1]]", nestedJson));
    Array<Array<int32_t>> restored;
    from_json(nestedJson, restored);
    CHECK(json_dump(to_json(restored)) == "[null,[],[0,1]]");
    from_json(Json{}, restored);
    CHECK(json_dump(to_json(restored)) == "null");

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
    List<int32_t> capacityHint{ 2 };
    CHECK(capacityHint.count == 0 && capacityHint.cap == 2);
    auto* reserved = capacityHint.items;
    capacityHint = Add(capacityHint, 7);
    capacityHint = Add(capacityHint, 8);
    CHECK(capacityHint.items == reserved);
    capacityHint = Add(capacityHint, 9);
    CHECK(ToCount(capacityHint) == 3 && At(Freeze(capacityHint), 2) == 9);
    CHECK(List<int32_t>{ -1 }.cap == 0 && List<int32_t>{ 0 }.items == nullptr);
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
    CHECK(Cbrt(8.0) == 2.0 && Cbrt(-27.0) == -3.0 && std::signbit(Cbrt(-0.0)) && !std::signbit(Cbrt(0.0)) && Cbrt(0.125) == 0.5);
    CHECK(LessThanOrEquals(std::string("B"), std::string("a")) && !LessThanOrEquals(std::string("a"), std::string("B")) && LessThanOrEquals(std::string("ab"), std::string("abc")) && !LessThanOrEquals(std::string("abc"), std::string("ab")) && LessThanOrEquals(std::string(), std::string("a")) && !LessThanOrEquals(std::string("é"), std::string("z")));

    // ToString
    CHECK(ToString(1.5) == "1.5" && ToString(100.0) == "100" && ToString(0.1) == "0.1");
    CHECK(ToString(1e15) == "1000000000000000" && ToString(1e16) == "10000000000000000" && ToString(1e17) == "1E+17" && ToString(123456789012345.0) == "123456789012345");
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

    CHECK(IsIntegerText("-12") && IsIntegerText("+7") && IsIntegerText("007") && IsIntegerText("-0") && IsIntegerText("2147483647") && IsIntegerText("-2147483648") && IsIntegerText("000000000000000000000000000001"));
    CHECK(!IsIntegerText("") && !IsIntegerText("+") && !IsIntegerText("-") && !IsIntegerText("1.0") && !IsIntegerText("1e3") && !IsIntegerText(" 1") && !IsIntegerText("1 ")
        && !IsIntegerText("2147483648") && !IsIntegerText("-2147483649") && !IsIntegerText("99999999999999999999") && !IsIntegerText("--1") && !IsIntegerText("0x10"));
    CHECK(IntegerFromText("-12") == -12 && IntegerFromText("1.0") == 0 && IntegerFromText("2147483647") == INT32_MAX && IntegerFromText("-2147483648") == INT32_MIN && IntegerFromText("2147483648") == 0 && IntegerFromText("+7") == 7);

    CHECK(NumberToShortestString(0.5) == "0.5" && NumberToShortestString(1e-5) == "1E-05" && NumberToShortestString(-0.0) == "-0" && NumberToShortestString(1e300) == "1E+300" && NumberToShortestString(1e15) == "1000000000000000");
    for (double x : { 0.1, 0.1 + 0.2, 1.0 / 3.0, -2.5e-300, 5e-324, 1.7976931348623157e308, 123456789.123456789, 9007199254740993.0, -0.0, 1e15, 1e-5, 4.35, 100.0 })
    {
        double back = NumberFromText(NumberToShortestString(x));
        CHECK(std::memcmp(&back, &x, sizeof x) == 0);
    }
    CHECK(!IsNumberText(NumberToShortestString(std::nan(""))) && !IsNumberText(NumberToShortestString(HUGE_VAL)) && !IsNumberText(NumberToShortestString(-HUGE_VAL)));

    CHECK(ToInteger('a') == 97 && ToInteger(static_cast<char>(200)) == 200 && ToCharacter(97) == 'a' && ToInteger(ToCharacter(255)) == 255 && At(std::string("abc"), 1) == 'b');
    CHECK(FromCharacters(make_array(ToCharacter(10), ToCharacter(13))) == std::string("\x0a\x0d"));

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

    // Bytes (TKT-1143): a view of bytes; the expected values are what the C# runtime gives.
    {
        const unsigned char sample[] = { 'x', 'x', '1', '.', '5', 'E', '1', 'y', 'y' };
        Bytes all = make_bytes(sample, 9);
        Bytes num = SubBytes(all, 2, 5);
        CHECK(ToCount(all) == 9 && ToCount(num) == 5 && At(num, 0) == '1' && At(num, 4) == '1');
        CHECK(num.data == sample + 2);   // a slice is a view of the same bytes
        CHECK(NumberFromBytes(num) == 15.0 && std::isnan(NumberFromBytes(all)) && std::isnan(NumberFromBytes(SubBytes(all, 2, 4))));
        CHECK(ToCount(SubBytes(all, -1, 3)) == 2 && ToCount(SubBytes(all, 8, 9)) == 1 && ToCount(SubBytes(all, 9, 1)) == 0 && ToCount(SubBytes(all, 0, 0)) == 0);
        CHECK(ToCount(SubBytes(SubBytes(all, 2, 5), 1, 9)) == 4);
        auto num_of = [](const char* t) { return NumberFromBytes(make_bytes(t, static_cast<int32_t>(std::strlen(t)))); };
        CHECK(num_of("1.") == 1.0 && num_of("-0.5E-3") == -0.0005 && num_of(".5") == 0.5 && num_of("+2") == 2.0 && num_of("1.E-06") == 1e-6);
        CHECK(std::signbit(num_of("-0")) && !std::signbit(num_of("0")) && num_of("1e-400") == 0.0 && num_of("9007199254740993") == 9007199254740992.0);
        CHECK(std::isnan(num_of("")) && std::isnan(num_of(".")) && std::isnan(num_of("1e")) && std::isnan(num_of("NaN")) && std::isnan(num_of("0x10"))
              && std::isnan(num_of(" 1")) && std::isnan(num_of("1e999")) && std::isnan(num_of("--1")) && std::isnan(num_of("1.5.2")));
        auto bytes_of = [](const char* t) { return make_bytes(t, static_cast<int32_t>(std::strlen(t))); };
        CHECK(Hash(bytes_of("")) == static_cast<int32_t>(2166136261u) && Hash(bytes_of("a")) == static_cast<int32_t>(0xE40C292Cu)
              && Hash(bytes_of("foo")) == static_cast<int32_t>(0xA9F37ED7u) && Hash(bytes_of("foobar")) == static_cast<int32_t>(0xBF9CF968u));
        CHECK(Hash(SubBytes(bytes_of("zzfoobarzz"), 2, 6)) == Hash(bytes_of("foobar")));
        {   // ToText: every byte value, 0 to 255, is one character of that code, and a view gives the text of its range
            uint8_t all256[256]; for (int i = 0; i < 256; i++) all256[i] = static_cast<uint8_t>(i);
            std::string t = ToText(make_bytes(all256, 256));
            bool ok = t.size() == 256;
            for (int i = 0; ok && i < 256; i++) ok = static_cast<uint8_t>(t[static_cast<size_t>(i)]) == i;
            CHECK(ok && ToText(Bytes{}) == "" && ToText(SubBytes(bytes_of("zzfoobarzz"), 2, 6)) == "foobar");
        }
        CHECK(bytes_of("abc") == bytes_of("abc") && !(bytes_of("abc") == bytes_of("abd")) && bytes_of("") == Bytes{});
        CHECK(LessThanOrEquals(bytes_of("ab"), bytes_of("abc")) && !LessThanOrEquals(bytes_of("abc"), bytes_of("ab")) && LessThanOrEquals(bytes_of("B"), bytes_of("a")));
        const unsigned char high[] = { 255 }, low[] = { 1 };
        CHECK(!LessThanOrEquals(make_bytes(high, 1), make_bytes(low, 1)) && LessThanOrEquals(make_bytes(low, 1), make_bytes(high, 1)));   // unsigned order
        arena_reset();
        Bytes made = FromIntegers(make_array<int32_t>(97, 98, 256 + 99, -1));
        CHECK(ToCount(made) == 4 && At(made, 0) == 97 && At(made, 2) == 99 && At(made, 3) == 255);
        CHECK(ToCount(FromIntegers(make_array_empty<int32_t>())) == 0);
        arena_reset();
    }

    std::printf(failures == 0 ? "OK\n" : "%d FAILURES\n", failures);
    return failures == 0 ? 0 : 1;
}
