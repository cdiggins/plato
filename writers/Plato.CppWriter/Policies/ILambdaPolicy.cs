// How the generated C++ lambdas capture (the capture-mode decision).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>Where the writer emits a C++ lambda.</summary>
    public enum LambdaSite
    {
        /// <summary>A function value of the Plato program (a callback, a closure returned or stored).</summary>
        FunctionValue,
        /// <summary>The immediately invoked lambda that builds an array literal.</summary>
        ArrayLiteral,
    }

    /// <summary>Chooses the capture list of each emitted lambda.</summary>
    public interface ILambdaPolicy
    {
        string Capture(LambdaSite site);
    }
}
