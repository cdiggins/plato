// How a string constant is spelled in generated C++ (the literal decision).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Spells a Plato string constant as a C++ expression of type <c>String</c>.</summary>
    public interface IStringLiteralPolicy
    {
        /// <param name="text">The constant, unescaped.</param>
        /// <param name="length">The length the prelude's string needs: bytes in the Cpu dialect, characters elsewhere.</param>
        string Literal(string text, int length);
    }
}
