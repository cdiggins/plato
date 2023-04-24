#include <vector>

// Reuses the lower 2 bits of a pointer as a reference count.
// A reference count equal to "3" becomes stuck and can never be 
// incremented or decremented. Effectively means data will last forever. 
// A reference count of "0" means that there is only one reference. 
template<typename T>
struct RefCountAndPointer
{
    T* ptr;

    uint32_t GetBits()
    {
        (uint32_t)ptr;
    };

    void SetRefCount(int n)
    {
        (uint32_t)ptr &= ~3u;
        (uint32_t)ptr += n;
    }

    uint32_t GetRefCount()
    {
        GetBits() & 3u;
    }

    T* GetPointer()
    {
        return (T*)(GetBits() & ~3u);
    }

    // Adds a count to the reference if there are less than three 
    // references. 
    void IncRefCount()
    {
        auto n = GetRefCount();
        if (n < 3)
            SetRefCount(n + 1);
    }

    // Returns true if removing the last reference. 
    // If there are three or more references, decrementing does nothing 
    bool DecRefCount()
    {
        auto n = GetRefCount();
        assert(n >= 0);
        if (n > 0 && n < 3)
            SetRefCount(n - 1);
        return n == 0;
    }
};

/*
    The last problem is with garbage collection ... if I want to delete a list, I have to remove everyting in it.
    This means I have to pop everything out of it.

    There are two kinds of lists. Those of known types, and those that aren't.

    Well technically they are:

    * List<int>, List<Function>, List<List<something>>

    The other question is when I delete (or execute) a function what happens?

    Well executing a function should leave it where it is.

*/

// dup => increments a reference count
// pop => decrements a reference count
// quote => moves data
// apply => moves data then decrements reference count? 

/*

What happns when we decrement reference count of a fuunction?
The function gets deleted. That's it.
What about things that it references?

If a function points to a list, we would want that list to have its reference count decremented.

Maybe there is somewhere we can leak? Like within a list of functions?

One piece of data, is like a list of data.

If you put a list in a function and you delete the function, the list (and everything it pointed to)
will stay forever. No reference count happens. (maybe?)

If you put a list in a list, and delete the parent list, everything it contains remains forever?

There is an interesting problem of the dup, quote, compose. Put a quoted version of a function in
some code.

Compose might not happen very often, does it make sense to do a deep copy of the instructions?

Some more observations:

* Lists of functions are rare.
* Functions quoting functions are rare.
* Lists of lists are uncommon

What if I say:
* A list can only contain unique references to objects.
* A function can only contain unique references to objects.

This makes a lot of things simpler!!!

But is that reasonable?

We can check at run-time, but in theory the type-system can allow us to skip the checks.

The "get" function is tricky, because if someone gets a value, they can "dup" it and we have a problem.

The "swapinto" function would allow us to get a value, and then do things ... and have a check when we
put it back.

A safe system would prevent us from doing that. The interesting thing is that a "swapinto" function needs
to be put around some code. We would want to remove those.

The thing is that there are going to be some optimized higher-level functions.
We want to make sure that the semantics is solid first.

BIG PROBLEM:

A function that is not executed, and is deleted (e.g., dec_ref, pop)
Will not free any of the objects that were put into it.
I would have to guess where the functions are.

One option: scan the function and look for "quote" operations.

*/
