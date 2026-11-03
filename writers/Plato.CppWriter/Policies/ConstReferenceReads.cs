// The current read-borrow policies: copies everywhere, or const references where a name is never written.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>--cpp-borrow-reads: bind array elements and unwritten locals by <c>const</c> reference.</summary>
    public sealed class ConstReferenceReads : IReadBorrowPolicy
    {
        public bool BorrowsReads => true;
        public string ElementBinding(bool readsArrayElement, bool written)
            => readsArrayElement && !written ? "const auto&" : "auto";
        public string ReductionElementBinding => "const auto&";
        public string LocalType(string cppType, bool sourceStable)
            => sourceStable ? $"const {cppType}&" : cppType;
    }

    /// <summary>The default: every read copies.</summary>
    public sealed class CopyingReads : IReadBorrowPolicy
    {
        public bool BorrowsReads => false;
        public string ElementBinding(bool readsArrayElement, bool written) => "auto";
        public string ReductionElementBinding => "auto";
        public string LocalType(string cppType, bool sourceStable) => cppType;
    }
}
