#include "plato_cpu.hpp"
#include <cstdio>
using namespace plato;
#define CHECK(c) do { if (!(c)) { std::printf("FAIL %d\n", __LINE__); return 1; } } while (0)
int main()
{
    const String text = u"Andr\u00e9 \u03a9 \U0001f600";
    CHECK(ToCount(text) == 10);
    CHECK(ToInteger(At(text, 4)) == 233 && ToInteger(At(text, 6)) == 937);
    CHECK(ToInteger(At(text, 8)) == 0xd83d && ToInteger(At(text, 9)) == 0xde00);
    CHECK(Substring(text, 8, 2) == u"\U0001f600");
    CHECK(StringFromUtf8(StringToUtf8(text)) == text);
    CHECK(StringFromUtf8(std::string("a\0b", 3)).size() == 3);
    CHECK(StringToUtf8(String(u"a\0b", 3)) == std::string("a\0b", 3));
    CHECK(make_string_n("a\0b", 3) == String(u"a\0b", 3));
    CHECK(FromCharacters(make_array(ToCharacter(937), ToCharacter(0xd83d), ToCharacter(0xde00))) == u"\u03a9\U0001f600");
    CHECK(ToInteger(ToCharacter(-1)) == 65535 && ToInteger(ToCharacter(65536)) == 0);
    CHECK(LessThanOrEquals(String(u"\U0001f600"), String(u"\ue000")));
    CHECK(!LessThanOrEquals(char16_t(255), char16_t(1)));
    CHECK(json_dump(to_json(text)) == "\"Andr\xc3\xa9 \xce\xa9 \xf0\x9f\x98\x80\"");
    Json json;
    CHECK(json_parse(json_dump(to_json(text)), json));
    String restored;
    from_json(json, restored);
    CHECK(restored == text);
    CHECK(to_json(Substring(text, 8, 1)).s == "\xef\xbf\xbd");
    char16_t character;
    from_json(to_json(char16_t(937)), character);
    CHECK(character == 937);
    CHECK(!IsNumberText(u"\u03a9") && !IsIntegerText(u"\u00e9"));
    for (int32_t code = 0; code <= 65535; ++code)
        CHECK(ToInteger(ToCharacter(code)) == code);
    return 0;
}
