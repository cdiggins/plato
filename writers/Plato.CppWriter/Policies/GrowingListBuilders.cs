// The list-builder policy of today: braced construction, so a stated capacity is passed through as written.
using System.Threading;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// <c>List&lt;T&gt;{ }</c> for an empty builder (no allocation; the first Add takes 4 and doubles) and
    /// <c>List&lt;T&gt;{ n }</c> when the source states a capacity. This is the output before --presize existed.
    /// </summary>
    public sealed class GrowingListBuilders : IListBuilderPolicy
    {
        public (string Open, string Close) Create(string cppType, bool hasCapacity)
        {
            if (!hasCapacity)
                Interlocked.Increment(ref C1bCounts.EmptyLists);
            return hasCapacity ? ($"{cppType}{{ ", " }") : ($"{cppType}{{  }}", "");
        }
    }
}
