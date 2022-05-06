using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoIR
{
    public class AbstractValue
    {
    }

    public class ValueSet<T> : AbstractValue<T>
    {
        public List<AbstractValue<T>> Values { get; } = new List<AbstractValue<T>>();
    }

    public class RangedValue<T> : AbstractValue<T>
    {
        public AbstractValue<T> Min;
        public AbstractValue<T> Max;
    }

    public class AbstractValue<T> : AbstractValue
    { }

    public class ConditionalValue<T> : AbstractValue<T>
    {
        public AbstractValue<bool> Conditional;
        public AbstractValue<T> OnTrue;
        public AbstractValue<T> OnFalse;
    }

    public class AnyValue<T> : AbstractValue<T>
    { }

    public class LoopedValue<T> : AbstractValue<T>
    {
        public AbstractValue<bool> Conditional;
        public AbstractValue<T> Value;
    }

    public class KnownValue<T> : AbstractValue<T>
    {
        public T Value { get; }
    }

    public class AbstractExpression : AbstractValue
    { }

    public class AbstractInvoke<T> : AbstractValue
    {
        public AbstractValue Function;
        public List<AbstractValue> Args { get; } = new List<AbstractValue>();

    }
}
