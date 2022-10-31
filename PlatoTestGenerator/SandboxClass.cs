// This represents a constraint on a type, but only applied to itself,
// not to all types implementing the interface.
// What do I call it? Is it a trait, mixin, constraint, concept, prototcol? 
public interface INumber 
{
    INumber Add(INumber a);
    INumber Subtract(INumber a);
    INumber Negate();
}

[Conforms(typeof(INumber))]
public class Unit
{
    public double Value { get; }
    public Unit(double x) => Value = x;
    public Unit Add(Unit x) => new Unit(Value + x.Value);
    public Unit Subtract(Unit x) => new Unit(Value - x.Value);
    public Unit Negate() => new Unit(-Value);
}

// The problem is that this is not executable. 
// I want to have operators on the INumber implementations
// Or at least I want to have literal syntax. That is not a problem. 
// To get a numeric value I would have to use a "Number" type placeholder. 

// I think the idea of using "Number" in a type is fine. 
// It is a special placeholder. When using actual types it won't work.
// It is a way of saying: I want the self type to be used throughout.
// The nice thing is that it can be used with: 
// float/double/float2/float3/float4/Double2/Double3/Double4/Complex 