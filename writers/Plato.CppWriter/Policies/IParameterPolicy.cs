// How a function parameter is passed in the generated C++ (the borrow-parameters decision).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>How one parameter is spelled in a function signature.</summary>
    public enum ParameterPassing
    {
        /// <summary>A plain by-value declaration, <c>T name</c>; the body may assign to it.</summary>
        Owned,
        /// <summary>The writer's usual spelling (<c>CppWriter.ParameterDeclaration</c>).</summary>
        Standard,
        /// <summary><c>ReadOnlyParameter&lt;T&gt; name</c>: a const reference for large records, a value otherwise.</summary>
        ReadOnly,
    }

    /// <summary>Decides the passing convention of each parameter of an emitted function.</summary>
    public interface IParameterPolicy
    {
        /// <param name="cppType">The parameter's C++ type.</param>
        /// <param name="assigned">The body assigns to the parameter, or to a member or element of it.</param>
        /// <param name="isRecord">The type is a closed, named record of this program (Cpu dialect only).</param>
        ParameterPassing Classify(string cppType, bool assigned, bool isRecord);
    }
}
