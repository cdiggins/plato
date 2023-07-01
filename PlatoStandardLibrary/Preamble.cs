// This file is required to make the input compile correctly

using System;

namespace Plato
{

    // The following are prerequisites built into Plato


    public interface IValue { }
    public interface IMeasure { }
    public interface IInterval { }
    public interface IVector { }

    public class ConceptAttribute : Attribute { }
    public class VectorAttribute : Attribute { }
    public class ValueAttribute : Attribute { }
    public class MeasureAttribute : Attribute { }
    public class NumericalAttribute : Attribute { }
    public class IntervalAttribute : Attribute { }
    public class OperationsAttribute : Attribute { }
    public class VectorizedAttribute : Attribute { }
    public class IntrinsicAttribute : Attribute { }

    class Constants
    {
        public const double Tolerance = double.Epsilon * 10;
        public const double Pi = Math.PI;
        public const double TwoPi = Pi * 2;
        public const double HalfPi = Pi / 2;
        public const double FeetPerMeter = 3.280839895;
        public const int FeetPerMile = 5280;
        public const double MetersPerLightyear = 9.46073047258e+15;
        public const double MetersPerAU = 149597870691;
        public const double DaltonPerKilogram = 1.66053e-27;
        public const double PoundPerKilogram = 0.45359237;
        public const int PoundPerTon = 2000;
        public const double KilogramPerSolarMass = 1.9889200011446E+30;
        public const int JulianYearSeconds = 31557600;
        public const double GregorianYearDays = 365.2425;
        public const double Hundred = 100;  
        public const double Thousand = 1000;
        public const double Million = Thousand * Thousand;
        public const double Billion = Thousand * Million;
        public const double Trillion = Thousand * Billion;
        public const double OunceToGram = 28.349523125;
        public const double TroyOunceToGram = 31.1034768;
        public const double GrainToMilligram = 64.79891;
    }

    public static class Helpers
    {
        public static bool AlmostEquals(this int value1, int value2)
            => value1 == value2;

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

        /// <summary>
        /// A second alternative takes advantage of a design feature of the floating-point format: The
        /// difference between the integer representation of two floating-point values indicates the
        /// number of possible floating-point values that separates them
        /// https://learn.microsoft.com/en-us/dotnet/api/system.double.equals?view=net-7.0#system-double-equals(system-double)
        /// </summary>
        public static bool AlmostEquals(this float value1, float value2)
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