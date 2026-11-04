// borrow: the parameter borrow rule (options PLATO_BORROW_MIN_BYTES, PLATO_BORROW_NON_TRIVIAL; see plato_config.hpp).
// A fragment of plato_cpu.hpp: included by it, in order, inside namespace plato. Not self-contained.

    // Use the target compiler's real layout instead of duplicating its ABI rules.
    // Return values remain owned; callers may bind temporaries for the call duration.
#if PLATO_BORROW_NON_TRIVIAL
    template <class T> using ReadOnlyParameter = std::conditional_t<(sizeof(T) >= PLATO_BORROW_MIN_BYTES || !std::is_trivially_copyable<T>::value), const T&, T>;
#else
    template <class T> using ReadOnlyParameter = std::conditional_t<(sizeof(T) >= PLATO_BORROW_MIN_BYTES), const T&, T>;
#endif
