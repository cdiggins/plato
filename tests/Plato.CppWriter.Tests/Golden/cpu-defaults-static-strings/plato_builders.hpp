// builders: List<T> and Buffer<T>, arena-backed.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Builders (unique List<T> and Buffer<T>): arena-backed; the old value is dead after Add/Set ----

    template <class T>
    struct PlatoList
    {
        T* items;
        int32_t count;
        int32_t cap;
        PlatoList() : items(nullptr), count(0), cap(0) {}
        explicit PlatoList(int32_t capacity)
            : items(arena_alloc<T>(capacity)), count(0), cap(capacity > 0 ? capacity : 0) {}
        PlatoList(T* data, int32_t size, int32_t capacity) : items(data), count(size), cap(capacity) {}
    };
    template <class T> using List = PlatoList<T>;

    template <class T> inline PlatoList<T> EmptyList(Array<T>) { return PlatoList<T>{ nullptr, 0, 0 }; }
    template <class T> inline int32_t ToCount(PlatoList<T> l) { return l.count; }
    template <class T> inline T At(PlatoList<T> l, int32_t i) { PLATO_ASSERT(i >= 0 && i < l.count); return l.items[i]; }

    template <class T>
    inline PlatoList<T> list_reserve(PlatoList<T> l, int32_t need)
    {
        if (need <= l.cap)
            return l;
        int32_t cap = l.cap == 0 ? 4 : l.cap * 2;
        if (cap < need)
            cap = need;
        T* p = arena_alloc<T>(cap);
        for (int32_t i = 0; i < l.count; i++)
            p[i] = l.items[i];
        return PlatoList<T>{ p, l.count, cap };
    }

    template <class T>
    inline PlatoList<T> Add(PlatoList<T> l, typename std::common_type<T>::type x)
    {
        l = list_reserve(l, l.count + 1);
        l.items[l.count++] = x;
        return l;
    }

    template <class T>
    inline PlatoList<T> AddRange(PlatoList<T> l, Array<T> xs)
    {
        l = list_reserve(l, l.count + xs.count);
        for (int32_t i = 0; i < xs.count; i++)
            l.items[l.count + i] = xs.data[i];
        l.count += xs.count;
        return l;
    }

    template <class T>
    inline PlatoList<T> Set(PlatoList<T> l, int32_t i, typename std::common_type<T>::type x)
    {
        PLATO_ASSERT(i >= 0 && i < l.count);
        l.items[i] = x;
        return l;
    }

    template <class T> inline Array<T> Freeze(PlatoList<T> l) { return Array<T>{ l.items, l.count }; }

    template <class T>
    struct PlatoBuffer
    {
        T* items;
        int32_t count;
    };
    template <class T> using Buffer = PlatoBuffer<T>;

    // new Buffer<T>(n): n slots, value-initialised (zero for scalars) until written.
    template <class T>
    inline PlatoBuffer<T> make_buffer(int32_t n)
    {
        if (n < 0)
            n = 0;
        T* p = arena_alloc<T>(n);
        for (int32_t i = 0; i < n; i++)
            new (p + i) T();
        return PlatoBuffer<T>{ p, n };
    }
    template <class T> inline int32_t ToCount(PlatoBuffer<T> b) { return b.count; }
    template <class T> inline T At(PlatoBuffer<T> b, int32_t i) { PLATO_ASSERT(i >= 0 && i < b.count); return b.items[i]; }
    template <class T>
    inline PlatoBuffer<T> Set(PlatoBuffer<T> b, int32_t i, typename std::common_type<T>::type x)
    {
        PLATO_ASSERT(i >= 0 && i < b.count);
        b.items[i] = x;
        return b;
    }
    template <class T> inline Array<T> Freeze(PlatoBuffer<T> b) { return Array<T>{ b.items, b.count }; }
