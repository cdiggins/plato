// core: the String type, UTF-8 and UTF-16 conversion, the function-value trait.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // String: an immutable view of UTF-16 code units, a pointer and a length. It is trivially copyable and trivially
    // destructible, like Array<T> and Bytes, so a record that holds one is copied bit for bit and needs no destructor.
    // The units a String views are never written once the view exists. They live in one of three places:
    //   - static storage: a u"..." literal (the converting constructor below, PLATO_STR, the string pool);
    //   - the arena of the thread that made the String: operator+, ToString, ToText, StringFromUtf8, make_string_n,
    //     from_json and string_copy allocate there; Substring and FromCharacters view units that already exist;
    //   - storage the host owns: String(p, n) over a buffer the host keeps alive and does not change.
    // Lifetime: a String whose units are in an arena is valid until that thread resets its arena (arena_reset), rolls
    // it back past the allocation (arena_rollback), or exits. A host that keeps a String across a reset, or hands one
    // to another thread, deep-copies it first: string_copy(s) into the arena of the calling thread, or
    // std::u16string(s.data(), s.size()) / StringToUtf8(s) into storage of its own.
    // Equality, order and hashing (HashString, GetHashCode) read the units, never the pointer. There is no terminating
    // NUL and no c_str(). The std-like members are the ones the runtime, the generated code and hosts call.
    struct String
    {
        const char16_t* chars = nullptr;
        int32_t length = 0;

        constexpr String() = default;
        // A view of the n units at p; nothing is copied, and the caller answers for the lifetime. More than INT32_MAX units abort.
        constexpr String(const char16_t* p, size_t n)
            : chars(n > 0 ? p : nullptr), length(n <= static_cast<size_t>(INT32_MAX) ? static_cast<int32_t>(n) : (std::abort(), 0)) {}
        // A view of a UTF-16 literal, u"text", without its terminating NUL: static storage, so valid for the whole program.
        // For literals only: an array that is not NUL-terminated or does not outlive the String is passed as (pointer, count).
        template <size_t N>
        constexpr String(const char16_t (&literal)[N]) : chars(N > 1 ? literal : nullptr), length(static_cast<int32_t>(N - 1)) {}

        constexpr size_t size() const { return static_cast<size_t>(length); }
        constexpr bool empty() const { return length == 0; }
        constexpr const char16_t* data() const { return chars; }
        constexpr const char16_t* begin() const { return chars; }
        constexpr const char16_t* end() const { return chars + length; }
        constexpr char16_t operator[](size_t i) const { PLATO_ASSERT(i < static_cast<size_t>(length)); return chars[i]; }
        // Ordinal on UTF-16 code units, the order of std::u16string::compare: negative, zero or positive.
        constexpr int compare(const String& other) const
        {
            const int32_t n = length < other.length ? length : other.length;
            for (int32_t i = 0; i < n; ++i)
                if (chars[i] != other.chars[i]) return chars[i] < other.chars[i] ? -1 : 1;
            return length < other.length ? -1 : (length > other.length ? 1 : 0);
        }
    };
    static_assert(std::is_trivially_copyable<String>::value && std::is_trivially_destructible<String>::value,
        "String is a plain view: records that hold one are copied bit for bit and never destroyed");

    inline bool operator==(const String& a, const String& b)
    {
        return a.length == b.length && (a.length == 0 || a.chars == b.chars
            || std::memcmp(a.chars, b.chars, static_cast<size_t>(a.length) * sizeof(char16_t)) == 0);
    }
    inline bool operator!=(const String& a, const String& b) { return !(a == b); }
    inline bool operator<(const String& a, const String& b) { return a.compare(b) < 0; }
    inline bool operator>(const String& a, const String& b) { return a.compare(b) > 0; }
    inline bool operator<=(const String& a, const String& b) { return a.compare(b) <= 0; }
    inline bool operator>=(const String& a, const String& b) { return a.compare(b) >= 0; }

    // Defined in plato_string.hpp, after the arena.
    // string_alloc: room for n units in this thread's arena, not yet written; null for n == 0; more than INT32_MAX abort.
    inline char16_t* string_alloc(size_t n);
    // string_copy: a String of the same units, copied into this thread's arena (the deep copy of the lifetime rule above).
    inline String string_copy(const char16_t* p, size_t n);
    inline String string_copy(const String& s);
    // Concatenation: a new String in this thread's arena; when one side is empty, the other side itself.
    inline String operator+(const String& a, const String& b);
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

    // The UTF-16 form of n UTF-8 bytes: returns the number of code units and, when out is not null, writes them there
    // (never more than n). Malformed UTF-8 aborts. Called once to count and once to fill, so no heap buffer is needed.
    inline size_t utf16_from_utf8(const char* bytes, size_t n, char16_t* out)
    {
        size_t count = 0;
        for (size_t i = 0; i < n;)
        {
            const auto first = static_cast<uint8_t>(bytes[i++]);
            uint32_t cp = first;
            int extra = 0;
            if (first >= 0xc2 && first <= 0xdf) { cp &= 31; extra = 1; }
            else if (first >= 0xe0 && first <= 0xef) { cp &= 15; extra = 2; }
            else if (first >= 0xf0 && first <= 0xf4) { cp &= 7; extra = 3; }
            else if (first >= 0x80) std::abort();
            if (n - i < static_cast<size_t>(extra)) std::abort();
            for (int k = 0; k < extra; ++k)
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
                if (out)
                {
                    out[count] = static_cast<char16_t>(0xd800 + (cp >> 10));
                    out[count + 1] = static_cast<char16_t>(0xdc00 + (cp & 1023));
                }
                count += 2;
            }
            else
            {
                if (out) out[count] = static_cast<char16_t>(cp);
                ++count;
            }
        }
        return count;
    }

    // The String of n UTF-8 bytes, in this thread's arena.
    inline String StringFromUtf8(const char* bytes, size_t n)
    {
        const size_t count = utf16_from_utf8(bytes, n, nullptr);
        if (count == 0) return String();
        char16_t* units = string_alloc(count);
        utf16_from_utf8(bytes, n, units);
        return String(units, count);
    }
    inline String StringFromUtf8(const std::string& bytes) { return StringFromUtf8(bytes.data(), bytes.size()); }

    // A function value: a lambda or functor with one operator(). A record is not one. It tells apart the
    // function-typed and the interface-typed overload of one name (Deform(self, mapping), Deform(self, deformation)).
    template <class T, class = void> struct is_function_value : std::false_type {};
    template <class T> struct is_function_value<T, std::void_t<decltype(&T::operator())>> : std::true_type {};
