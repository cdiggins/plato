// A list-builder policy for --presize: a counted fill allocates its backing store once.
using System.Threading;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// A builder with a stated capacity is <c>List&lt;T&gt;(n)</c>: the prelude's explicit constructor takes
    /// <c>arena_alloc&lt;T&gt;(n)</c> once, ignores a negative <c>n</c> (no allocation, capacity 0, as the C# runtime
    /// ignores a negative hint), and later Adds never copy while the count stays within n. The paren form cannot be
    /// read as an aggregate initialisation. An empty builder is spelled as before.
    /// </summary>
    public sealed class PresizedListBuilders : IListBuilderPolicy
    {
        public (string Open, string Close) Create(string cppType, bool hasCapacity)
        {
            if (!hasCapacity)
            {
                Interlocked.Increment(ref C1bCounts.EmptyLists);
                return ($"{cppType}{{  }}", "");
            }
            Interlocked.Increment(ref C1bCounts.PresizedLists);
            return ($"{cppType}(", ")");
        }
    }
}
