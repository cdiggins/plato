// arena: thread-local bump storage, marks, rollback and reset.
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // ---- Arena: thread-local storage with reverse-order destruction on rollback/reset ----

#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable : 4324)  // padded by alignas(16): intended
#endif
    struct alignas(16) ArenaBlock
    {
        ArenaBlock* next;
        size_t cap;
        size_t used;
    };
#ifdef _MSC_VER
#pragma warning(pop)
#endif

    struct ArenaCleanup
    {
        void* data;
        int32_t count;
        void (*destroy)(void*, int32_t);
    };

    struct ArenaState
    {
        ArenaBlock* head = nullptr;
        ArenaBlock* tail = nullptr;
        ArenaBlock* cur = nullptr;
        size_t bytes = 0;
        size_t peak = 0;
        size_t next_block = size_t(1) << 20;
        size_t generation = 0;
        std::vector<ArenaCleanup> cleanups;

        ArenaState() = default;
        ArenaState(const ArenaState&) = delete;
        ArenaState& operator=(const ArenaState&) = delete;
        void destroy_from(size_t count)
        {
            while (cleanups.size() > count)
            {
                const auto cleanup = cleanups.back();
                cleanups.pop_back();
                cleanup.destroy(cleanup.data, cleanup.count);
            }
        }
        ~ArenaState()
        {
            destroy_from(0);
            while (head)
            {
                auto* next = head->next;
                std::free(head);
                head = next;
            }
        }
    };

#if PLATO_ARENA_TLS_PTR
    // The pointer has a constant initializer, so reading it needs no guard; the state itself is created once per
    // thread by the slow path and destroyed at thread exit as before.
    inline thread_local ArenaState* arena_tls_state = nullptr;

#ifdef _MSC_VER
    __declspec(noinline)
#else
    __attribute__((noinline))
#endif
    inline ArenaState& arena_state_create()
    {
        static thread_local ArenaState s;
        arena_tls_state = &s;
        return s;
    }

    inline ArenaState& arena_state()
    {
        ArenaState* s = arena_tls_state;
        return s ? *s : arena_state_create();
    }
#else
    inline ArenaState& arena_state()
    {
        static thread_local ArenaState s;
        return s;
    }
#endif

#if PLATO_ARENA_LAZY_PEAK
    // The allocator does not touch peak: it is folded in before bytes can fall and on read.
    inline void arena_note_peak(ArenaState& s)
    {
        if (s.bytes > s.peak)
            s.peak = s.bytes;
    }
#endif

    // Finds room in a later or new block; the bump path of arena_alloc_bytes failed.
#if PLATO_ARENA_INLINE
#ifdef _MSC_VER
    __declspec(noinline)
#else
    __attribute__((noinline))
#endif
    inline unsigned char* arena_alloc_slow(ArenaState& s, size_t bytes)
    {
        for (;;)
        {
            if (s.cur && s.cur->cap - s.cur->used >= bytes)
            {
                unsigned char* p = reinterpret_cast<unsigned char*>(s.cur) + sizeof(ArenaBlock) + s.cur->used;
                s.cur->used += bytes;
                s.bytes += bytes;
#if !PLATO_ARENA_LAZY_PEAK
                if (s.bytes > s.peak)
                    s.peak = s.bytes;
#endif
                return p;
            }
            if (s.cur && s.cur->next)
            {
                s.cur = s.cur->next;
                continue;
            }
            size_t cap = s.next_block > bytes ? s.next_block : bytes;
            if (cap > std::numeric_limits<size_t>::max() - sizeof(ArenaBlock)) std::abort();
            ArenaBlock* b = static_cast<ArenaBlock*>(std::malloc(sizeof(ArenaBlock) + cap));
            if (!b)
                std::abort();
            b->next = nullptr;
            b->cap = cap;
            b->used = 0;
            if (s.tail)
                s.tail->next = b;
            else
                s.head = b;
            s.tail = b;
            s.cur = b;
            if (s.next_block < (size_t(256) << 20))
                s.next_block *= 2;
        }
    }

#ifdef _MSC_VER
    __forceinline
#else
    __attribute__((always_inline)) inline
#endif
    unsigned char* arena_alloc_bytes(size_t bytes)
    {
        if (bytes > std::numeric_limits<size_t>::max() - 15) std::abort();
        bytes = (bytes + 15) & ~size_t(15);
        if (bytes == 0)
            return nullptr;
        ArenaState& s = arena_state();
        ArenaBlock* c = s.cur;
        if (c && c->cap - c->used >= bytes)
        {
            unsigned char* p = reinterpret_cast<unsigned char*>(c) + sizeof(ArenaBlock) + c->used;
            c->used += bytes;
            s.bytes += bytes;
#if !PLATO_ARENA_LAZY_PEAK
            if (s.bytes > s.peak)
                s.peak = s.bytes;
#endif
            return p;
        }
        return arena_alloc_slow(s, bytes);
    }
#else
    inline unsigned char* arena_alloc_bytes(size_t bytes)
    {
        if (bytes > std::numeric_limits<size_t>::max() - 15) std::abort();
        bytes = (bytes + 15) & ~size_t(15);
        if (bytes == 0)
            return nullptr;
        ArenaState& s = arena_state();
        for (;;)
        {
            if (s.cur && s.cur->cap - s.cur->used >= bytes)
            {
                unsigned char* p = reinterpret_cast<unsigned char*>(s.cur) + sizeof(ArenaBlock) + s.cur->used;
                s.cur->used += bytes;
                s.bytes += bytes;
#if !PLATO_ARENA_LAZY_PEAK
                if (s.bytes > s.peak)
                    s.peak = s.bytes;
#endif
                return p;
            }
            if (s.cur && s.cur->next)
            {
                s.cur = s.cur->next;
                continue;
            }
            size_t cap = s.next_block > bytes ? s.next_block : bytes;
            if (cap > std::numeric_limits<size_t>::max() - sizeof(ArenaBlock)) std::abort();
            ArenaBlock* b = static_cast<ArenaBlock*>(std::malloc(sizeof(ArenaBlock) + cap));
            if (!b)
                std::abort();
            b->next = nullptr;
            b->cap = cap;
            b->used = 0;
            if (s.tail)
                s.tail->next = b;
            else
                s.head = b;
            s.tail = b;
            s.cur = b;
            if (s.next_block < (size_t(256) << 20))
                s.next_block *= 2;
        }
    }
#endif

    template <class T>
    inline T* arena_alloc(int32_t n)
    {
        static_assert(alignof(T) <= 16, "arena_alloc: over-aligned type");
        if (n <= 0)
            return nullptr;
        if (static_cast<size_t>(n) > std::numeric_limits<size_t>::max() / sizeof(T)) std::abort();
        T* p = reinterpret_cast<T*>(arena_alloc_bytes(sizeof(T) * static_cast<size_t>(n)));
#if PLATO_ARENA_UNINIT
        if constexpr (!std::is_trivially_default_constructible<T>::value && !std::is_trivially_copyable<T>::value)
#else
        if constexpr (!std::is_trivially_default_constructible<T>::value)
#endif
            for (int32_t i = 0; i < n; i++)
                new (p + i) T();
        if constexpr (!std::is_trivially_destructible<T>::value)
            arena_state().cleanups.push_back({ p, n, [](void* data, int32_t count) {
                auto* values = static_cast<T*>(data);
                while (count > 0) values[--count].~T();
            } });
        return p;
    }

    // Marks belong to this thread and remain valid only until reset or rollback past them.
    struct ArenaMark
    {
        ArenaState* owner;
        ArenaBlock* block;
        size_t used, bytes, cleanups, generation;
    };

    inline ArenaMark arena_mark()
    {
        auto& s = arena_state();
        return { &s, s.cur, s.cur ? s.cur->used : 0, s.bytes, s.cleanups.size(), s.generation };
    }

    inline void arena_rollback(const ArenaMark& mark)
    {
        auto& s = arena_state();
        if (mark.owner != &s || mark.generation != s.generation || mark.bytes > s.bytes
            || mark.cleanups > s.cleanups.size()) std::abort();
#if PLATO_ARENA_LAZY_PEAK
        arena_note_peak(s);
#endif
        s.destroy_from(mark.cleanups);
        for (auto* block = mark.block ? mark.block->next : s.head; block; block = block->next)
            block->used = 0;
        if (mark.block) mark.block->used = mark.used;
        s.cur = mark.block ? mark.block : s.head;
        s.bytes = mark.bytes;
    }

    inline void arena_reset()
    {
        ArenaState& s = arena_state();
#if PLATO_ARENA_LAZY_PEAK
        arena_note_peak(s);
#endif
        s.destroy_from(0);
        ++s.generation;
        for (ArenaBlock* b = s.head; b; b = b->next)
            b->used = 0;
        s.cur = s.head;
        s.bytes = 0;
    }

    inline size_t arena_bytes() { return arena_state().bytes; }
#if PLATO_ARENA_LAZY_PEAK
    inline size_t arena_peak_bytes()
    {
        auto& s = arena_state();
        arena_note_peak(s);
        return s.peak;
    }
#else
    inline size_t arena_peak_bytes() { return arena_state().peak; }
#endif
