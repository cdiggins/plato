// The current lambda policy: function values capture by value, array-literal builders by reference.
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// A function value may outlive the scope that made it, so it copies what it uses (<c>[=]</c>); the
    /// array-literal builder runs where it is written and fills a local (<c>[&amp;]</c>).
    /// </summary>
    public sealed class CaptureByValueLambdas : ILambdaPolicy
    {
        public string Capture(LambdaSite site) => site == LambdaSite.ArrayLiteral ? "[&]" : "[=]";
    }
}
