// Whether reads of arrays and names are bound by reference (the borrow-reads decision).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Arrays are immutable views into the arena and generated code never rolls the arena back inside a
    /// body, so a reference to an element lives as long as the body. The body writer proves which names are
    /// never written; the policy decides whether to use that proof.
    /// </summary>
    public interface IReadBorrowPolicy
    {
        /// <summary>Array reads (<c>At</c>) return <c>const T&amp;</c>, and a conditional adjusts its branches.</summary>
        bool BorrowsReads { get; }

        /// <summary>The declaration keyword for a lowered-loop parameter bound to an element expression.</summary>
        /// <param name="readsArrayElement">The element expression indexes an array's data.</param>
        /// <param name="written">The body writes the parameter.</param>
        string ElementBinding(bool readsArrayElement, bool written);

        /// <summary>The declaration keyword for the element of a paired reduction.</summary>
        string ReductionElementBinding { get; }

        /// <summary>The declared type of a local whose value is a record or String.</summary>
        /// <param name="cppType">The local's C++ type.</param>
        /// <param name="sourceStable">The value is an array read or a name, and neither side is written in the body.</param>
        string LocalType(string cppType, bool sourceStable);
    }
}
