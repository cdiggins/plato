namespace Plato.Geometry
{
    public interface ICurve<T>
    {
        bool Closed { get; }
        T Sample(float t);
    }

    public static class CurveExtensions
    {
    }
}