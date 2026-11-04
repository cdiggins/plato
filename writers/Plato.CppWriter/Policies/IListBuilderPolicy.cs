// How a List builder is created in generated C++ (the capacity decision; TKT-1183, C# --presize).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Spells the creation of a <c>List&lt;T&gt;</c> builder. The TIR rewrite <c>presize</c> (Plato.Optimizer) gives a
    /// counted fill its capacity as the constructor's one argument; what C++ does with it is decided here.</summary>
    public interface IListBuilderPolicy
    {
        /// <param name="cppType">The list type as spelled, for example <c>List&lt;Point2D&gt;</c>.</param>
        /// <param name="hasCapacity">Whether the capacity expression is written between the two returned texts.</param>
        (string Open, string Close) Create(string cppType, bool hasCapacity);
    }
}
