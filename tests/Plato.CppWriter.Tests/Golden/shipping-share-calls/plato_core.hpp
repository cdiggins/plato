// core: the String type, UTF-8 and UTF-16 conversion, the function-value trait.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    using String = std::u16string;
    #include "plato_borrow.hpp"


    // Plato text uses UTF-16 code units, like C#. JSON and host byte streams use UTF-8.
    inline void append_utf8(std::string& out, uint32_t cp)
    {
        if (cp < 0x80) out.push_back(static_cast<char>(cp));
        else if (cp < 0x800) { out.push_back(static_cast<char>(0xc0 | (cp >> 6))); out.push_back(static_cast<char>(0x80 | (cp & 63))); }
        else if (cp < 0x10000) { out.push_back(static_cast<char>(0xe0 | (cp >> 12))); out.push_back(static_cast<char>(0x80 | ((cp >> 6) & 63))); out.push_back(static_cast<char>(0x80 | (cp & 63))); }
        else { out.push_back(static_cast<char>(0xf0 | (cp >> 18))); out.push_back(static_cast<char>(0x80 | ((cp >> 12) & 63))); out.push_back(static_cast<char>(0x80 | ((cp >> 6) & 63))); out.push_back(static_cast<char>(0x80 | (cp & 63))); }
    }

    inline std::string StringToUtf8(const String& text)
    {
        std::string out;
        out.reserve(text.size());
        for (size_t i = 0; i < text.size(); ++i)
        {
            uint32_t cp = text[i];
            if (cp >= 0xd800 && cp <= 0xdbff && i + 1 < text.size() && text[i + 1] >= 0xdc00 && text[i + 1] <= 0xdfff)
                cp = 0x10000 + ((cp - 0xd800) << 10) + (text[++i] - 0xdc00);
            else if (cp >= 0xd800 && cp <= 0xdfff) cp = 0xfffd;
            append_utf8(out, cp);
        }
        return out;
    }

    inline String StringFromUtf8(const std::string& bytes)
    {
        String out;
        out.reserve(bytes.size());
        for (size_t i = 0; i < bytes.size();)
        {
            const auto first = static_cast<uint8_t>(bytes[i++]);
            uint32_t cp = first;
            int extra = 0;
            if (first >= 0xc2 && first <= 0xdf) { cp &= 31; extra = 1; }
            else if (first >= 0xe0 && first <= 0xef) { cp &= 15; extra = 2; }
            else if (first >= 0xf0 && first <= 0xf4) { cp &= 7; extra = 3; }
            else if (first >= 0x80) std::abort();
            if (bytes.size() - i < static_cast<size_t>(extra)) std::abort();
            for (int n = 0; n < extra; ++n)
            {
                const auto next = static_cast<uint8_t>(bytes[i++]);
                if ((next & 0xc0) != 0x80) std::abort();
                cp = (cp << 6) | (next & 63);
            }
            if ((extra == 1 && cp < 0x80) || (extra == 2 && cp < 0x800) || (extra == 3 && cp < 0x10000)
                || cp > 0x10ffff || (cp >= 0xd800 && cp <= 0xdfff)) std::abort();
            if (cp >= 0x10000)
            {
                cp -= 0x10000;
                out.push_back(static_cast<char16_t>(0xd800 + (cp >> 10)));
                out.push_back(static_cast<char16_t>(0xdc00 + (cp & 1023)));
            }
            else out.push_back(static_cast<char16_t>(cp));
        }
        return out;
    }

    // A function value: a lambda or functor with one operator(). A record is not one. It tells apart the
    // function-typed and the interface-typed overload of one name (Deform(self, mapping), Deform(self, deformation)).
    template <class T, class = void> struct is_function_value : std::false_type {};
    template <class T> struct is_function_value<T, std::void_t<decltype(&T::operator())>> : std::true_type {};
