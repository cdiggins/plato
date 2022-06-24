using Plato.Math;

public class Sandbox
{
    public interface IMeshQuery
    {
        Box Box { get; }
        Vector3 WeightedCenter { get; }
    }


}