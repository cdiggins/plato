// This file is required to make the input compile correctly
using Plato.__TYPES__;
using System;

namespace Plato
{
    // The following are prerequisites built into Plato

    public interface IValue { }
    public interface IMeasure { }
    public interface IInterval { }
    public interface IVector { }

    public class VectorAttribute : Attribute { }
    public class ValueAttribute : Attribute { }
    public class MeasureAttribute : Attribute { }
    public class NumberAttribute : Attribute { }
    public class IntervalAttribute : Attribute { }
    public class OperationsAttribute : Attribute { }
    public class VectorizedOperationsAttribute : Attribute { }

    class Constants
    {
        public const double Tolerance = double.Epsilon * 10;
        public const double Pi = Math.PI;
        public const double TwoPi = Pi * 2;
        public const double HalfPi = Pi / 2;
    }

    public static class Helpers
    {
        /// <summary>
        /// A second alternative takes advantage of a design feature of the floating-point format: The
        /// difference between the integer representation of two floating-point values indicates the
        /// number of possible floating-point values that separates them
        /// https://learn.microsoft.com/en-us/dotnet/api/system.double.equals?view=net-7.0#system-double-equals(system-double)
        /// </summary>
        public static bool AlmostEquals(this double value1, double value2)
        {
            var lValue1 = BitConverter.DoubleToInt64Bits(value1);
            var lValue2 = BitConverter.DoubleToInt64Bits(value2);

            // If the signs are different, return false except for +0 and -0.
            if (lValue1 >> 63 != lValue2 >> 63)
            {
                // Will return true iff one value is +0 and the other is -0
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                return value1 == value2;
            }

            return Math.Abs(lValue1 - lValue2) <= 1;
        }
    }
}