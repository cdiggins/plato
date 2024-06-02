namespace Plato.Compiler.Symbols
{
    public class MapExpression : Expression
    {
        public override string Name => "Map";
    }

    public class ReduceExpression : Expression
    {
        public override string Name => "Reduce";
    }

    public class FilterExpression : Expression
    {
        public override string Name => "Filter";
    }
}