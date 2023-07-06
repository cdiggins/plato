using Plato;

class Concept
{
    public bool HasField(string name);
}

class Interval<T>
{
    T Min { get; }
    T Max { get; }
}

[Operations]
class Interval
{
    Any Extent() => Min - Max;
    bool Empty() => Min >= Max;
    Any Lerp(Number amount) => Min * (1.0 - amount) + Max * amount;
    Number InverseLerp(Any value) => (value - Min) / Extent;
}

[Operations]
class Interval
{
    function Extent() => Min - Max;
    function Empty() => Min >= Max;
    function Lerp(amount) => Min * (1.0 - amount) + Max * amount;
    function InverseLerp(value) => (value - Min) / Extent;
}

class Ops
{

}

class Vector<T>
{
    const int Count;
    T At(int index);
    string Name(int index);
}

class Interval : Concept
{
    bool Implements()
    {
        var result = HasField("Min")
            && HasField("Max");

            && TypeOf(Min) == TypeOf(Max);

            && Implements(Min, Number);
        if (result)
            ElementType = TypeOf(Min);
        return result;
    }

    Type ElementType;

    bool IsEmpty()
        => Min >= Max;

    Array ToArray()
        => [Min, Max];

    object Lerp(Unit amount)
        => Min * (1.0 - amount) + Second * amount;
}