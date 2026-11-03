// plato_config.hpp: the defaults of the runtime options of plato_cpu.hpp. CppWriter writes one of these from
// CppCpuOptions; every macro keeps a value given on the command line (cl /DPLATO_BORROW_READS=1).

// Smallest sizeof(T) of a record that ReadOnlyParameter<T> passes by const reference.
#ifndef PLATO_BORROW_MIN_BYTES
#define PLATO_BORROW_MIN_BYTES 256
#endif
// 1: ReadOnlyParameter also borrows every record that is not trivially copyable, whatever its size.
#ifndef PLATO_BORROW_NON_TRIVIAL
#define PLATO_BORROW_NON_TRIVIAL 0
#endif
// 1: array_at_readonly returns const T& (arrays are immutable arena views) instead of ReadOnlyParameter<T>.
#ifndef PLATO_BORROW_READS
#define PLATO_BORROW_READS 0
#endif
