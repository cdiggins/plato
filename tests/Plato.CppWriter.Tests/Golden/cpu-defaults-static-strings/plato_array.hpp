// array: the immutable Array<T> view and its operations (option PLATO_BORROW_READS).
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Array<T>: immutable view; At does not clamp (debug asserts, release trusts the index) ----

    template <class T>
    struct Array
    {
        const T* data = nullptr;
        int32_t count = 0;
        // CLR array defaults are null; an explicitly constructed empty view is [].
        bool has_value = false;
        Array() = default;
        Array(const T* values, int32_t size) : data(values), count(size), has_value(true) {}
        const T& operator[](int32_t i) const
        {
            PLATO_ASSERT(i >= 0 && i < count);
            return data[i];
        }
    };

    template <class T>
    inline Array<T> make_array_empty() { return Array<T>{ nullptr, 0 }; }

    template <class T>
    inline Array<T> make_array(const T* data, int32_t count) { return Array<T>{ data, count < 0 ? 0 : count }; }

    // make_array(a, b, c): a literal array, copied into the arena. Not for pointer element types
    // (that would be ambiguous with the (data, count) form above).
    template <class T, class... Rest, class = typename std::enable_if<!std::is_pointer<T>::value>::type>
    inline Array<T> make_array(T first, Rest... rest)
    {
        const int32_t n = static_cast<int32_t>(1 + sizeof...(Rest));
        T* p = arena_alloc<T>(n);
        const T vals[] = { first, static_cast<T>(rest)... };
        for (int32_t i = 0; i < n; i++)
            p[i] = vals[i];
        return Array<T>{ p, n };
    }

    template <class T> inline int32_t ToCount(Array<T> xs) { return xs.count; }
    // Internal immutable read: large values may bind directly to borrowed call
    // parameters. The reference is valid only while the array's region is alive.
#if PLATO_BORROW_READS
    template <class T> inline const T& array_at_readonly(Array<T> xs, int32_t i)
#else
    template <class T> inline ReadOnlyParameter<T> array_at_readonly(Array<T> xs, int32_t i)
#endif
    { PLATO_ASSERT(i >= 0 && i < xs.count); return xs.data[i]; }
    // Preserve the public value-returning accessor contract for host callers.
    template <class T> inline T At(Array<T> xs, int32_t i) { return array_at_readonly(xs, i); }

    // MapRange: f(0) to f(n - 1), each once, in index order; n <= 0 gives the empty array.
    template <class F>
    inline auto MapRange(int32_t n, F f) -> Array<std::decay_t<decltype(f(int32_t(0)))>>
    {
        using R = std::decay_t<decltype(f(int32_t(0)))>;
        if (n <= 0)
            return Array<R>{ nullptr, 0 };
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(i);
        return Array<R>{ p, n };
    }

    inline Array<int32_t> Range(int32_t n)
    {
        return MapRange(n, [](int32_t i) { return i; });
    }

    template <class T, class F>
    inline auto Map(Array<T> xs, F f) -> Array<std::decay_t<decltype(f(xs.data[0]))>>
    {
        using R = std::decay_t<decltype(f(xs.data[0]))>;
        R* p = arena_alloc<R>(xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = f(xs.data[i]);
        return Array<R>{ p, xs.count };
    }

    template <class T, class U, class F>
    inline auto Zip(Array<T> a, Array<U> b, F f) -> Array<std::decay_t<decltype(f(a.data[0], b.data[0]))>>
    {
        using R = std::decay_t<decltype(f(a.data[0], b.data[0]))>;
        const int32_t n = a.count < b.count ? a.count : b.count;
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(a.data[i], b.data[i]);
        return Array<R>{ p, n < 0 ? 0 : n };
    }

    template <class T, class U, class V, class F>
    inline auto Zip(Array<T> a, Array<U> b, Array<V> c, F f) -> Array<std::decay_t<decltype(f(a.data[0], b.data[0], c.data[0]))>>
    {
        using R = std::decay_t<decltype(f(a.data[0], b.data[0], c.data[0]))>;
        int32_t n = a.count < b.count ? a.count : b.count;
        if (c.count < n)
            n = c.count;
        R* p = arena_alloc<R>(n);
        for (int32_t i = 0; i < n; i++)
            p[i] = f(a.data[i], b.data[i], c.data[i]);
        return Array<R>{ p, n < 0 ? 0 : n };
    }

    template <class T, class Acc, class F>
    inline Acc Reduce(Array<T> xs, Acc acc, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            acc = f(acc, xs.data[i]);
        return acc;
    }

    // FlatMap: f runs once per element, in order; the parts are concatenated.
    template <class T, class F>
    inline auto FlatMap(Array<T> xs, F f) -> decltype(f(xs.data[0]))
    {
        using Part = decltype(f(xs.data[0]));
        using R = std::decay_t<decltype(*f(xs.data[0]).data)>;
        Part* parts = arena_alloc<Part>(xs.count);
        int32_t total = 0;
        for (int32_t i = 0; i < xs.count; i++)
        {
            parts[i] = f(xs.data[i]);
            total += parts[i].count;
        }
        R* p = arena_alloc<R>(total);
        int32_t k = 0;
        for (int32_t i = 0; i < xs.count; i++)
            for (int32_t j = 0; j < parts[i].count; j++)
                p[k++] = parts[i].data[j];
        return Part{ p, total };
    }

    template <class T, class F>
    inline bool All(Array<T> xs, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (!f(xs.data[i]))
                return false;
        return true;
    }

    template <class T, class F>
    inline bool Any(Array<T> xs, F f)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (f(xs.data[i]))
                return true;
        return false;
    }

    template <class T>
    inline Array<T> Reverse(Array<T> xs)
    {
        T* p = arena_alloc<T>(xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[xs.count - 1 - i];
        return Array<T>{ p, xs.count };
    }

    template <class T>
    inline Array<T> Concatenate(Array<T> xs, Array<T> ys)
    {
        const int32_t n = xs.count + ys.count;
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[i];
        for (int32_t i = 0; i < ys.count; i++)
            p[xs.count + i] = ys.data[i];
        return Array<T>{ p, n };
    }

    template <class T>
    inline Array<T> Append(Array<T> xs, typename std::common_type<T>::type x)
    {
        T* p = arena_alloc<T>(xs.count + 1);
        for (int32_t i = 0; i < xs.count; i++)
            p[i] = xs.data[i];
        p[xs.count] = x;
        return Array<T>{ p, xs.count + 1 };
    }

    template <class T>
    inline Array<T> Prepend(Array<T> xs, typename std::common_type<T>::type x)
    {
        T* p = arena_alloc<T>(xs.count + 1);
        p[0] = x;
        for (int32_t i = 0; i < xs.count; i++)
            p[i + 1] = xs.data[i];
        return Array<T>{ p, xs.count + 1 };
    }

    // Slice, Take and Skip share the storage (the view is immutable); out-of-range bounds clamp.
    template <class T>
    inline Array<T> Slice(Array<T> xs, int32_t start, int32_t count)
    {
        if (start < 0) start = 0;
        if (start > xs.count) start = xs.count;
        if (count < 0) count = 0;
        if (count > xs.count - start) count = xs.count - start;
        return Array<T>{ xs.data + start, count };
    }
    template <class T> inline Array<T> Take(Array<T> xs, int32_t n) { return Slice(xs, 0, n); }
    template <class T> inline Array<T> Skip(Array<T> xs, int32_t n) { return Slice(xs, n, xs.count); }

    template <class T>
    inline bool operator==(Array<T> a, Array<T> b)
    {
        if (a.count != b.count)
            return false;
        for (int32_t i = 0; i < a.count; i++)
            if (!(a.data[i] == b.data[i]))
                return false;
        return true;
    }
    template <class T> inline bool operator!=(Array<T> a, Array<T> b) { return !(a == b); }

    template <class T>
    inline int32_t IndexOf(Array<T> xs, typename std::common_type<T>::type x)
    {
        for (int32_t i = 0; i < xs.count; i++)
            if (xs.data[i] == x)
                return i;
        return -1;
    }
    template <class T>
    inline bool Contains(Array<T> xs, typename std::common_type<T>::type x) { return IndexOf(xs, x) >= 0; }
