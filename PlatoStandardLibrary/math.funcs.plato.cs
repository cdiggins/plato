using Plato.__TYPES__;
using System;

namespace Plato.__FUNCS__
{
    // Here is a version in C#: https://gist.github.com/cdiggins/bdbc6b4c54909456d881ac704eb6f02f
    // Also look at: https://frinklang.org/frinkdata/units.txt
    // https://www.codeproject.com/Articles/23087/Measurement-Unit-Conversion-Library
    // https://github.com/angularsen/UnitsNet 
    // https://stackoverflow.com/questions/348853/units-of-measure-in-c-sharp-almost
    // https://www.codeproject.com/Articles/413750/Units-of-Measure-Validator-for-Csharp
    // https://stackoverflow.com/questions/2791724/units-of-measurement-conversion-logic-in-c-sharp
    // https://digidemic.github.io/UnitOf/
    // https://github.com/martinmoene/PhysUnits-CT-Cpp11 - Overview of C++ libraries
    [Operations]
    class UnitOperations
    {
        // https://en.wikipedia.org/wiki/Angle
        Angle Radians(double d) => d;
        Angle Turns(double d) => d * 2 * Math.PI;
        Angle Degrees(double d) => d.Turns() / 360.0;
        Angle Grads(double d) => d.Turns() / 400.0;
        Angle ArcMinutes(double d) => d.Degrees() / 60;
        Angle ArcSeconds(double d) => d.ArcMinutes() / 60;
        double ToTurns(Angle a) => a / 2 / Math.PI;
        double ToDegrees(Angle a) => a.Turns * 360;
        double ToGrads(Angle a) => a.Turns * 400;
        double ToArcMinutes(Angle a) => a.Degrees * 60;
        double ToArcSeconds(Angle a) => a.ArcMinutes * 60;

        // Proportion: percent of one 
        // https://en.wikipedia.org/wiki/Percentage
        // Some interesting discussions 
        // https://english.stackexchange.com/questions/275734/a-word-for-a-value-between-0-and-1-inclusive
        // https://twitter.com/cdiggins/status/1583610796331843584
        Proportion Proportion(double d) => d;
        Proportion Percent(double d) => d / 100;
        Proportion BasisPoints(double d) => Percent(d / 100);
        Proportion Proportion(Angle a) => a.Turns;
        double ToPercent(Proportion p) => p.Of(100);
        double Of(Proportion p, double amount) => p / amount;
        double OfOne(Proportion p) => p.Value;
        double ToBasisPoints(Proportion p) => p.Percent * 100;
        Angle ToAngle(Proportion p) => Turns(p);

        // Units of distance/length
        // https://en.wikipedia.org/wiki/Unit_of_length
        Length Meters(double value) => value;
        Length Kilometer(double value) => value * 1000;
        Length Centimeters(double value) => value / 100;
        Length Decimeters(double value) => value / 10;
        Length Millimeters(double value) => value / 100;
        Length Microns(double value) => value / 1000 / 1000;
        Length Nanometers(double value) => value / 1000 / 1000 / 1000;
        Length Inches(double value) => Feet(value / 12);
        Length Feet(double value) => value / Constants.FeetPerMeter;
        Length Yards(double value) => Feet(value * 3);
        Length Rods(double value) => Chains(value / 4);
        Length Chains(double value) => Yards(value * 22);
        Length Miles(double value) => Feet(value * Constants.FeetPerMile);
        Length Leagues(double value) => Miles(value * 3);
        Length Lightyears(double value) => value * Constants.MetersPerLightyear;
        Length AU(double value) => value * Constants.MetersPerAU;
        Length HubbleLength(double value) => value * 14.4.Billion() * Lightyears(value);
        double ToKilometers(Length length) => length.Meters / 1000;
        double ToDecimeters(Length length) => length.Meters * 10;
        double ToCentimeters(Length length) => length.Meters * 100;
        double ToMillimeters(Length length) => length.Meters * 1000;
        double ToMicrons(Length length) => length.Millimeters * 1000;
        double ToNanometers(Length length) => length.Microns * 1000;
        double ToInches(Length length) => length.Feet * 12;
        double ToFeet(Length length) => length * Constants.FeetPerMeter;
        double ToYards(Length length) => ToFeet(length) / 3;
        double ToRods(Length length) => ToChains(length) * 4;
        double ToChains(Length length) => ToYards(length) / 22;
        double ToMiles(Length length) => length.Meters * Constants.FeetPerMeter / Constants.FeetPerMile;
        double ToLeague(Length length) => ToMiles(length) / 3;
        double ToLightyears(Length length) => length / Constants.MetersPerLightyear;
        double ToAU(Length length) => length / Constants.MetersPerAU;
        double ToHubbleLength(double value) => value / 1.HubbleLength();

        // Units of weight or mass 
        // https://en.wikipedia.org/wiki/Mass
        Mass Milligrams(double value) => value / 1000 / 1000;
        Mass Grams(double value) => value / 1000;
        Mass Grains(double value) => Milligrams(value * Constants.GrainToMilligram);
        Mass Kilograms(double value) => value;
        Mass Dalton(double value) => value * Constants.DaltonPerKilogram;
        Mass Tonne(double value) => value * 1000;
        Mass Pound(double value) => value * Constants.PoundPerKilogram;
        Mass Ton(double value) => Pound(value * Constants.PoundPerTon);
        Mass SolarMass(double value) => value * Constants.KilogramPerSolarMass;
        Mass Ounce(double value) => Grams(value * Constants.OunceToGram);
        double ToMilligrams(Mass m) => ToGrams(m) * 1000;
        double ToGrams(Mass m) => m.Kilograms * 1000;
        double ToGrains(Mass m) => ToMilligrams(m) / Constants.GrainToMilligram;
        double ToDalton(Mass m) => m.Kilograms / Constants.DaltonPerKilogram;
        double ToTonne(Mass m) => m.Kilograms / 1000;
        double ToPound(Mass m) => m.Kilograms / Constants.PoundPerKilogram;
        double ToTon(Mass m) => ToPound(m) / Constants.PoundPerTon;
        double ToSolarMass(Mass m) => m / Constants.KilogramPerSolarMass;
        double ToOunce(Mass m) => ToGrams(m) / Constants.OunceToGram;

        // Temperature      
        Temperature Celsius(double value) => value;
        Temperature Kelvin(double value) => value + 273.15;
        Temperature Faranheit(double value) => (value - 32) * 5.0 / 9.0;
        double ToKelvin(Temperature t) => t - 273.15;
        double ToFaranheit(Temperature t) => t * 9.0 / 5.0 + 32.0;

        // Memory
        // https://en.wikipedia.org/wiki/Byte#Multiple-byte_units
        Memory Bytes(double value) => value;
        Memory Bits(double value) => value * 8;
        Memory Octets(double value) => value;
        Memory Nibbles(double value) => value / 2;
        Memory Kilobytes(double value) => value * 1000;
        Memory Megabytes(double value) => Kilobytes(value) * 1000;
        Memory Gigabytes(double value) => Megabytes(value) * 1000;
        Memory Terabytes(double value) => Gigabytes(value) * 1000;
        Memory Petabytes(double value) => Terabytes(value) * 1000;
        Memory Exabytes(double value) => Petabytes(value) * 1000;
        Memory Kebibytes(double value) => value * 1024;
        Memory Mebibytes(double value) => Kebibytes(value) * 1024;
        Memory Gibibytes(double value) => Mebibytes(value) * 1024;
        Memory Tebibytes(double value) => Gibibytes(value) * 1024;
        Memory Pebibytes(double value) => Tebibytes(value) * 1024;
        Memory Exibytes(double value) => Pebibytes(value) * 1024;
        double ToBits(Memory m) => m.Bytes / 8;
        double ToOctet(Memory m) => m.Bytes;
        double ToNibble(Memory m) => m.Bytes / 2;
        double ToKilobytes(Memory m) => m.Bytes / 1000;
        double ToMegabytes(Memory m) => m.Kilobytes / 1000;
        double ToGigabytes(Memory m) => m.Megabytes / 1000;
        double ToTerabytes(Memory m) => m.Gigabytes / 1000;
        double ToPetabytes(Memory m) => m.Terabytes / 1000;
        double ToExabytes(Memory m) => m.Petabytes / 1000;
        double ToKebibytes(Memory m) => m.Bytes / 1024;
        double ToMebibytes(Memory m) => m.Kebibytes / 1024;
        double ToGibibytes(Memory m) => m.Mebibytes / 1024;
        double ToTebibytes(Memory m) => m.Gibibytes / 1024;
        double ToPebibytes(Memory m) => m.Tebibytes / 1024;
        double ToExibytes(Memory m) => m.Pebibytes / 1024;

        // Units of time
        // https://en.wikipedia.org/wiki/TU_(Time_Unit)
        Time Nanoseconds(double value) => value / 1000 / 1000 / 10000;
        Time Microseconds(double value) => value / 1000 / 1000;
        Time Milliseconds(double value) => value / 1000;
        Time TimeUnits(double value) => Microseconds(value * 1024);
        Time Seconds(double value) => value;
        Time Minutes(double value) => value * 60;
        Time Hours(double value) => value * 60 * 60;
        Time Days(double value) => value * 60 * 60 * 24;
        Time Weeks(double value) => value * 60 * 60 * 24 * 7;
        Time JulianYears(double value) => value * Constants.JulianYearSeconds;
        Time GregorianYears(double value) => Days(value * Constants.GregorianYearDays);
        double ToNanosecond(Time t) => t.Seconds * 1000 * 1000 * 1000;
        double ToMicroeconds(Time t) => t.Seconds * 1000 * 1000;
        double ToTimeUnits(Time t) => ToMicroeconds(t) / 1024;
        double ToMilliseconds(Time t) => t.Seconds * 1000;
        double ToMinutes(Time t) => t.Seconds / 60;
        double ToHours(Time t) => t.Seconds / (60 * 60);
        double ToDays(Time t) => t.Seconds / (60 * 60 * 24);
        double ToWeeks(Time t) => t.Seconds / (60 * 60 * 24 * 7);
        double ToJulianYears(Time t) => t.Seconds / Constants.JulianYearSeconds;
        double ToGregorianYears(Time t) => Days(t) / Constants.GregorianYearDays;
        
        // https://en.wikipedia.org/wiki/Names_of_large_numbers
        // Some big numbers 
        double Hundred(double x) => x * Constants.Hundred;
        double Thousand(double x) => x * Constants.Thousand;
        double Million(double x) => x * Constants.Million;
        double Billion(double x) => x * Constants.Billion;
        double Trillion(double x) => x * Constants.Trillion;
        double Tenth(double x) => x / 10;
        double Hundredth(double x) => x / Constants.Hundred;
        double Thousandth(double x) => x / Constants.Thousand;
        double Millionth(double x) => x / Constants.Million;
        double Billionth(double x) => x / Constants.Billion;
        double Trillionth(double x) => x / Constants.Trillion;

        // https://en.wikipedia.org/wiki/Metric_prefix
        double Quetta(double x) => x * 1e+30;
        double Ronna(double x) => x * 1e+27;
        double Yotta(double x) => x * 1e+24;
        double Zetta(double x) => x * 1e+21;
        double Exa(double x) => x * 1e+18;
        double Peta(double x) => x * 1e+15;
        double Tera(double x) => x * 1e+12;
        double Giga(double x) => x * 1e+9;
        double Mega(double x) => x * 1e+6;
        double Kilo(double x) => x * 1000;
        double Hecto(double x) => x * 100;
        double Deka(double x) => x * 10;
        double Deci(double x) => x / 10;
        double Centi(double x) => x * 100;
        double Milli(double x) => x * 1e-3;
        double Micro(double x) => x * 1e-6;
        double Nano(double x) => x * 1e-9;
        double Pico(double x) => x * 1e-12;
        double Femto(double x) => x * 1e-15;
        double Atto(double x) => x * 1e-18;
        double Zepto(double x) => x * 1e-21;
        double Yocto(double x) => x * 1e-24;
        double Ronto(double x) => x * 1e-27;
        double Quecto(double x) => x * 1e-30;

        double Reciprocal(double x) => 1 / x;
        double MultiplicativeInverse(double x) => Reciprocal(x);

        Velocity Light() => 299792458;

        // Combining different units of measure to create compound units
        Area Multiply(Length length, Length width) => length.Meters * width.Meters;
        Volume Multiply(Area area, Length height) => area.MetersSquared * height.Meters;
        Volume Multiply(Length height, Area area) => Multiply(area, height);
        Force Multiply(Mass mass, Acceleration accel) => mass.Kilograms * accel.MetersPerSecondSquared;
        Force Multiply(Acceleration accel, Mass mass) => Multiply(mass, accel);
        Energy Multiply(Force force, Length length) => force.Newtons * length.Meters;
        Energy Multiply(Length length, Force force) => Multiply(force, length);

        Pressure Divide(Force force, Area area) => force.Newtons / area.MetersSquared;
        Velocity Divide(Length length, Time time) => length.Meters / time.Seconds;
        Acceleration Divide(Velocity Velocity, Time time) => Velocity.MetersPerSecond / time.Seconds;

        Length Divide(Area area, Length length) => area.MetersSquared / length.Meters;
        Area Divide(Volume volume, Length length) => volume.MetersCubed / length.Meters;
        Length Divide(Volume volume, Area area) => volume.MetersCubed / area.MetersSquared;
        Force Multiply(Pressure pressure, Area area) => pressure.Pascals * area.MetersSquared;
        Force Multiply(Area area, Pressure pressure) => Multiply(pressure, area);
        Area Divide(Pressure pressure, Force force) => pressure.Pascals * force.Newtons;
        Length Multiply(Velocity Velocity, Time time) => Velocity.MetersPerSecond * time.Seconds;
        Time Divide(Velocity Velocity, Length length) => Velocity.MetersPerSecond / length.Meters;
        Velocity Multiply(Acceleration acceleration, Time time) => acceleration.MetersPerSecondSquared * time.Seconds;
        Velocity Multiply(Time time, Acceleration acceleration) => Multiply(acceleration, time);
        Time Divide(Acceleration acceleration, Velocity Velocity) => acceleration.MetersPerSecondSquared / Velocity.MetersPerSecond;
        Mass Divide(Force force, Acceleration acceleration) => force.Newtons / acceleration.MetersPerSecondSquared;
        Acceleration Divide(Force force, Mass mass) => force.Newtons / mass.Kilograms;
        Force Divide(Energy energy, Length length) => energy.Joules / length.Meters;
        Length Divide(Energy energy, Force force) => energy.Joules / force.Newtons;
        Power Divide(Energy energy, Time time) => energy.Joules / time.Seconds;
        Energy Multiply(Power power, Time time) => power.Watts * time.Seconds;
        Energy Multiply(Time time, Power power) => Multiply(power, time);
        Power Multiply(ElectricPotential ep, ElectricCurrent ec) => ep.Volts * ec.Amperes;
        Density Divide(Mass m, Volume v) => m.Kilograms / v.MetersCubed;
        Mass Muliply(Density d, Volume v) => d.KilogramsPerMeterCubed * v.MetersCubed;
        Volume Divide(Density d, Mass m) => d.KilogramsPerMeterCubed / m.Kilograms;

        Velocity PerSecond(Length l) => l / 1.Seconds();
        Acceleration PerSecond(Velocity s) => s / 1.Seconds();
        Power PerSecond(Energy e) => e / 1.Seconds();
        Energy Kilojoules(double d) => d.Thousand().Joules();
        Energy WattHours(double d) => d * 1.Hours().Seconds;
        Energy NewtonMeters(double d) => d;
        Energy Joules(double d) => d;
        Density KilogramsPerMeterCubed(double d) => d;
        Power Watts(double d) => d;
        ElectricResistance Ohms(double d) => d;
        ElectricCurrent Amperes(double d) => d;
        ElectricCharge Columbs(double d) => d;
        LuminousIntensity Candela(double d) => d;
        Frequency Hertz(double d) => d;
        Pressure Pascals(double d) => d;
        Force Newtons(double d) => d;
        Acceleration MetersPerSecondSquared(double d) => d;
        Velocity MetersPerSecond(double d) => d;
        Area MetersSquared(double d) => d;
        Volume MetersCubed(double d) => d;

        // TODO: volume
        // https://en.wikipedia.org/wiki/Fluid_ounce
    }

    [Operations]
    class VectorOperations
    {
        Double2 Normal(Double2 v) => v / v.Length();
        Double3 Normal(Double3 v) => v / v.Length();
        Double4 Normal(Double4 v) => v / v.Length();
        Float2 Normal(Float2 v) => v / (float)v.Length();
        Float3 Normal(Float3 v) => v / (float)v.Length();
        Float4 Normal(Float4 v) => v / (float)v.Length();
        double Length(Line2D line) => Distance(line.A, line.B);
        double Distance(Double2 a, Double2 b) => (b - a).Length();
        double Distance(Double3 a, Double3 b) => (b - a).Length();
        double Distance(Double4 a, Double4 b) => (b - a).Length();
        double Distance(Float2 a, Float2 b) => (b - a).Length();
        double Distance(Float3 a, Float3 b) => (b - a).Length();
        double Distance(Float4 a, Float4 b) => (b - a).Length();
    }

    [VectorizedOperations]
    class VectorizedOperations
    {
        double SafeDivide(double x, double y) => y.AlmostZero() ? x : x / y;
        double Half(double x) => x * 0.5;
        double Quarter(double x) => x * 0.25;
        double Twice(double x) => x * 2;
        double Thrice(double x) => x * 3;
        double MinusOne(double x) => x - 1;
        double PlusOne(double x) => x + 1;
        double OneMinus(double x) => 1 - x;
        double Abs(double x) => Math.Abs(x);
        double Exp(double x) => Math.Exp(x);
        double Log(double x) => Math.Log(x);
        double Log10(double x) => Math.Log10(x);
        double Sqrt(double x) => Math.Sqrt(x);
        double Sign(double x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        double Inverse(double x) => 1 / x;
        double Truncate(double x) => Math.Truncate(x);
        double Ceiling(double x) => Math.Ceiling(x);
        double Floor(double x) => Math.Floor(x);
        double Round(double x) => Math.Round(x);
        double Smoothstep(double v) => v.Pow2() * (3 - 2 * v);
        double Lerp(double v1, double v2, double t) => v1 * (1 - t) + v2 * t;
        double Mix(double v1, double v2, double t) => Lerp(v1, v2, t);
        double InverseLerp(double v, double a, double b) => (v - a) / (b - a);
        double Unmix(double v, double a, double b) => InverseLerp(v, a, b);
        double Clamp(double v, double min, double max) => Max(Min(v, max), min);
        double ClampZeroToOne(double v) => Clamp(v, 0, 1);
        double Average(double v1, double v2) => Lerp(v1, v2, 0.5);
        double Barycentric(double v1, double v2, double v3, Double2 uv) => v1 + (v2 - v1) * uv.X + (v3 - v1) * uv.Y;
        double Min(double v1, double v2) => Math.Min(v1, v2);
        double Max(double v1, double v2) => Math.Max(v1, v2);
        double ClampPositive(double v) => Math.Max(v, 0);
        double ClampNegative(double v) => Math.Min(v, 0);
        double Pow2(double x) => x * x;
        double Pow3(double x) => x * x * x;
        double Pow4(double x) => x * x * x * x;
        double Pow5(double x) => x * x * x * x * x;
        double Pow(double x, double y) => Math.Pow(x, y);
    }

    [Operations]
    class TrigOperations
    {
        Angle Acos(double x) => Math.Acos(x);
        Angle Asin(double x) => Math.Asin(x);
        Angle Atan(double x) => Math.Atan(x);

        // Extremely esoteric functions.
        double Versin(Angle x) => 1 - Cos(x);
        double Vercosin(Angle x) => 1 + Cos(x);
        double Coversin(Angle x) => 1 - Sin(x);
        double Covercosin(Angle x) => 1 + Sin(x);
        double Haversin(Angle x) => Versin(x) / 2;
        double Havercosin(Angle x) => Vercosin(x) / 2;
        double Hacoversin(Angle x) => Coversin(x) / 2;
        double Hacovercosin(Angle x) => Coversin(x) / 2;
        double Exsec(Angle x) => Sec(x) - 1;
        double Excsc(Angle x) => Csc(x) - 1;

        double Cos(Angle x) => Math.Cos(x);
        double Cosh(Angle x) => Math.Cosh(x);
        double Sin(Angle x) => Math.Sin(x);
        double Sinh(Angle x) => Math.Sinh(x);
        double Tan(Angle x) => Math.Tan(x);
        double Tanh(Angle x) => Math.Tanh(x);

        double Sec(Angle x) => 1.0 / Cos(x);
        double Csc(Angle x) => 1.0 / Sin(x);
        double Cot(Angle x) => 1.0 / Tan(x);
    }

    [Operations]
    class ComparisonOperations
    {
        bool GtZ(double x) => x > 0;
        bool LtZ(double x) => x < 0;
        bool GtEqZ(double x) => x >= 0;
        bool LtEqZ(double x) => x <= 0;
        bool Gt(double x, double y) => x > y;
        bool Lt(double x, double y) => x > y;
        bool GtEq(double x, double y) => x >= y;
        bool LtEq(double x, double y) => x <= y;
        bool IsInfinity(double v) => double.IsInfinity(v);
        bool IsNaN(double v) => double.IsNaN(v);
        bool AlmostZero(double v) => v.AlmostEquals(0);
        bool AlmostOne(double v) => v.AlmostEquals(1);
        bool AlmostZeroOrOne(double v) => v.AlmostZero() && v.AlmostOne();
        bool Within(double v, double min, double max) => v >= min && v < max;
    }

    [Operations]
    class IntervalOperations
    {
        double Size(Interval interval) => (interval.A - interval.B).Abs();
        Interval ResetLeftToZero(Interval interval) => (0, Size(interval));
        Interval ResetRightToZero(Interval interval) => (-Size(interval), 0);
        double HalfSize(Interval interval) => (interval.A - interval.B).Abs().Half();
        Interval Invert(Interval interval) => (interval.B, interval.A);
        Interval Normalize(Interval interval) => (interval.A.Min(interval.B), interval.B.Max(interval.B));
        double Clamp(double value, Interval interval) => value.Clamp(interval.A, interval.B);
        double Lerp(double value, Interval interval) => value.Lerp(interval.A, interval.B);
        double InverseLerp(double value, Interval interval) => value.InverseLerp(interval.A, interval.B);
        double Clamp(Interval interval, double value) => Clamp(value, interval);
        double Lerp(Interval interval, double value) => Lerp(value, interval);
        double InverseLerp(Interval interval, double value) => InverseLerp(value, interval);
        double Remap(double value, Interval input, Interval output) => Lerp(InverseLerp(value, input), output);
        double Center(Interval interval) => Lerp(0.5, interval);
        Interval Left(Interval interval, double amount) => (interval.A, Lerp(amount, interval));
        Interval Right(Interval interval, double amount) => (Lerp(amount, interval), interval.B);
        (Interval, Interval) Split(Interval interval, double amount) => (Left(interval, amount), Right(interval, amount));
        Interval Resize(Interval interval, double amount) => (Center(interval) - amount / 2, Center(interval) + amount * 2);
        Interval ResizeRelative(Interval interval, double amount) => Resize(interval, Size(interval) * amount);
        Interval Multiply(Interval interval, double amount) => ResizeRelative(interval, Size(interval) * amount);
        Interval Divide(Interval interval, double amount) => Multiply(interval, 1.0 / amount);
        Interval Offset(Interval interval, double amount) => (interval.A + amount, interval.B + amount);
        Interval CenterAt(Interval interval, double value = 0) => Offset(interval, value - Center(interval));
    }

    [Operations]
    class EasingOperations
    {
        //===
        // Easings.cs
        // https://easings.net/
        // https://github.com/acron0/Easings/blob/master/Easings.cs

        Func<double, double> BlendEaseFunc(Func<double, double> easeIn, Func<double, double> easeOut) => p => p < 0.5 ? 0.5 * easeIn(p * 2) : 0.5 * easeOut(p * 2 - 1) + 0.5;
        Func<double, double> InvertEaseFunc(Func<double, double> easeIn) => p => 1 - easeIn(1 - p);

        double Linear(double p) => p;
        double QuadraticEaseIn(double p) => p.Pow2();
        double QuadraticEaseOut(double p) => InvertEaseFunc(QuadraticEaseIn)(p);
        double QuadraticEaseInOut(double p) => BlendEaseFunc(QuadraticEaseIn, QuadraticEaseOut)(p);
        double CubicEaseIn(double p) => p.Pow3();
        double CubicEaseOut(double p) => InvertEaseFunc(CubicEaseIn)(p);
        double CubicEaseInOut(double p) => BlendEaseFunc(CubicEaseIn, CubicEaseOut)(p);
        double QuarticEaseIn(double p) => p.Pow4();
        double QuarticEaseOut(double p) => InvertEaseFunc(QuarticEaseIn)(p);
        double QuarticEaseInOut(double p) => BlendEaseFunc(QuarticEaseIn, QuarticEaseOut)(p);
        double QuinticEaseIn(double p) => p.Pow5();
        double QuinticEaseOut(double p) => InvertEaseFunc(QuinticEaseIn)(p);
        double QuinticEaseInOut(double p) => BlendEaseFunc(QuinticEaseIn, QuinticEaseOut)(p);
        double SineEaseIn(double p) => InvertEaseFunc(SineEaseOut)(p);
        double SineEaseOut(double p) => p.Quarter().Turns().Sin();
        double SineEaseInOut(double p) => BlendEaseFunc(SineEaseIn, SineEaseOut)(p);
        double CircularEaseIn(double p) => 1 - (1 - p.Pow2()).Sqrt();
        double CircularEaseOut(double p) => InvertEaseFunc(CircularEaseIn)(p);
        double CircularEaseInOut(double p) => BlendEaseFunc(CircularEaseIn, CircularEaseOut)(p);
        double ExponentialEaseIn(double p) => p.AlmostZero() ? p : 2.Pow(10 * (p - 1));
        double ExponentialEaseOut(double p) => InvertEaseFunc(ExponentialEaseIn)(p);
        double ExponentialEaseInOut(double p) => BlendEaseFunc(ExponentialEaseIn, ExponentialEaseOut)(p);
        double ElasticEaseIn(double p) => (13 * p.Quarter().Turns()) * 2.Pow(10 * (p - 1)).Radians().Sin();
        double ElasticEaseOut(double p) => InvertEaseFunc(ElasticEaseIn)(p);
        double ElasticEaseInOut(double p) => BlendEaseFunc(ElasticEaseIn, ElasticEaseOut)(p);
        double BackEaseIn(double p) => p.Pow3() - p * p.Half().Turns().Sin();
        double BackEaseOut(double p) => InvertEaseFunc(BackEaseIn)(p);
        double BackEaseInOut(double p) => BlendEaseFunc(BackEaseIn, BackEaseOut)(p);
        double BounceEaseIn(double p) => InvertEaseFunc(BounceEaseOut)(p);
        double BounceEaseOut(double p)
        {
            if (p < 4 / 11.0) return 121.0 * p.Pow2() / 16.0;
            if (p < 8 / 11.0) return 363.0 / 40.0 * p.Pow2() - 99.0 / 10.0 * p + 17.0 / 5.0;
            if (p < 9 / 10.0) return 4356.0 / 361.0 * p.Pow2() - 35442.0 / 1805.0 * p + 16061.0 / 1805.0;
            return 54.0 / 5.0 * p.Pow2() - 513.0 / 25.0 * p + 268.0 / 25.0;
        }
        double BounceEaseInOut(double p) => BlendEaseFunc(BounceEaseIn, BounceEaseOut)(p);
    }

    [Operations]
    class ShapingOperations
    {
        //===
        // Signal or Shaping functions
        // https://iquilezles.org/articles/functions/
        // https://thebookofshaders.com/05/

        double ExponentialImpulse(double x, double k) => k * x * (1.0 - k * x).Exp();
        double QuadraticImpulse(double x, double k) => 2.0 * k.Sqrt() * x / (1.0 + k * x * x);
        double PolynomalialImpulse(double x, double k, double n) => (n / (n - 1.0)) * ((n - 1.0) * k).Pow(1.0 / n) * x / (1.0 + k * x.Pow(n));
        double NormalizedPowerCurve(double x, double a, double b) => (a + b.Pow(a + b) / (a.Pow(a) * (b.Pow(b)) * UnnormalizedPowerCurve(x, a, b)));
        double UnnormalizedPowerCurve(double x, double a, double b) => x.Pow(a) * (1.0 - x).Pow(b);
        double Parabola(double x, double k) => 4.0 * x * (1.0 - x).Pow(k);
        double Sinc(double x, double k) { var a = k * (x - 1.0).Half().Turns(); return a.Sin() / a; }
        double Gain(double x, double k) { var a = 0.5 * (2.0 * ((x < 0.5) ? x : 1.0 - x).Pow(k)); return (x < 0.5) ? a : 1.0 - a; }
        double ExponentialStep(double x, double k, double n) => (-k * x.Pow(n)).Exp();
        double NearIdentityCubic(double x, double threshold, double constant = 0) { if (x > threshold) return x; var a = 2.0 * constant - threshold; var b = 2.0 * threshold - 3.0 * constant; var t = x / threshold; return (a * t + b) * t * t + constant; }
        double NearIdentitySqrt(double x, double constant = 0) => (x * x + constant).Sqrt();
        double NearUnityIdentity(double x) => x * x * (2.0 - x);
        double IntegralSmoothstep(double x, double t) => (x > t) ? x - t / 2.0 : (x).Pow3() * (1.0 - x * 0.5 / t) / t / t;
    }

    [Operations]
    class CurveOperations
    {
        //===
        // http://www.flong.com/archive/texts/code/shapers_circ/

        double CircleSigmoid(double x, double a = 0.5) => x <= a
            ? a - (a.Pow2() - x.Pow2()).Sqrt()
            : a + ((1 - a).Pow2() - (x - 1).Pow2().Pow2()).Sqrt();

        double CircularSeat(double x, double a = 0.5) =>
            x <= a ? (a.Pow2() - (x - a).Pow2()).Sqrt() : ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        double EllipticalSeat(double x, double a, double b) => (a.AlmostZeroOrOne()) ? a :
            (x <= a) ? (b / a) * (a.Pow2() - (x - a).Pow2()).Sqrt() :
            1 - ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        double EllipticalSigmoid(double x, double a, double b) => a.AlmostZeroOrOne() ? a :
            (x <= a) ? b * (1 - ((a.Pow2() - x.Pow2()) / a).Sqrt()) :
            b + ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - 1).Pow2()).Sqrt();

        // https://en.wikipedia.org/wiki/Catenary
        double Caternay(double x, double a = 1.0) => (x / a).Radians().Cosh();

        //== 
        // Some parametric curves 
        // https://en.wikipedia.org/wiki/Cycloid
        // https://en.wikipedia.org/wiki/Trochoid
        // https://en.wikipedia.org/wiki/Epitrochoid
        // https://en.wikipedia.org/wiki/Hypotrochoid 
        // https://en.wikipedia.org/wiki/Hypocycloid
        // https://en.wikipedia.org/wiki/Cyclocycloid
        // https://en.wikipedia.org/wiki/Epicycloid
        // https://en.wikipedia.org/wiki/Lissajous_curve
        // https://en.wikipedia.org/wiki/Lemniscate_of_Gerono
        // https://en.wikipedia.org/wiki/Parametric_equation
        // https://en.wikipedia.org/wiki/Lima%C3%A7on

        Double2 Parabola(double x)
            => (x, x.Pow2());

        Double2 Hyperbola(Angle t)
            => (t.Sec(), t.Tan());

        Double2 LeminscateOfGerono(Angle t)
            => (t.Cos(), t.Sin() * t.Cos());

        Double2 Circle(Angle t)
            => (t.Cos(), t.Sin());

        Double2 Lissajous(Angle t, double a, double b, double kx, double ky)
            => (a * (kx * t).Cos(),
                b * (ky * t).Sin());

        Double2 Hypotrochoid(Angle t, double r, double d)
            => ((1 - r) * t.Cos() + d * (t - 1.0).Cos(),
                (1 - r) * t.Sin() + d * (t - 1.0).Sin());

        Double2 Epicycloid(Angle t, double r, double k)
            => (r * (k + 1) * t.Cos() - r * ((k + 1) * t).Cos(),
                r * (k + 1) * t.Sin() - r * ((k + 1) * t).Sin());

        Double2 ClosedEpicycloid(Angle t, int k) => Epicycloid(t, 1.0 / k, 1);
        Double2 Cardoid(Angle t) => ClosedEpicycloid(t, 1);
        Double2 Nephroid(Angle t) => ClosedEpicycloid(t, 2);
        Double2 Trefoiloid(Angle t) => ClosedEpicycloid(t, 3);
        Double2 Quatrefoiloid(Angle t) => ClosedEpicycloid(t, 4);

        Double2 Hypocycloid(Angle t, double r, double k)
            => (r * (k - 1) * t.Cos() + r * ((k - 1) * t).Cos(),
                r * (k - 1) * t.Sin() + r * ((k - 1) * t).Cos());

        Double2 ClosedHypocycloid(Angle t, int k) => Hypocycloid(t, 1.0 / k, 1);
        Double2 Deltoid(Angle t) => ClosedHypocycloid(t, 3);
        Double2 Astroid(Angle t) => ClosedHypocycloid(t, 4);

        Double2 Epitrochoid(Angle t, double r, double d)
            => ((1 + r) * t.Cos() - d * ((1 + r) / r * t).Cos(),
                (1 + r) * t.Sin() - d * ((1 + r) / r * t).Sin());

        // TODO: there are some cute examples in Wikipedia at https://en.wikipedia.org/wiki/Parametric_equation, but no names
        Double2 GeneralizedParametricCurve(Angle t, double a, double b, double c, double d, double j, double k)
            => ((a * t).Cos() - (b * t).Cos().Pow(j),
                (c * t).Sin() - (d * t).Sin().Pow(k));

        double PolarLimacon(Angle t, double a, double b)
            => b + a * t.Cos();

        Double2 CartesianLimacon(Angle t, double a = 1, double b = 1)
            => ToCartesian((PolarLimacon(t, a, b), t));

        Double2 ToCartesian(PolarCoordinate polar)
            => Circle(polar.Angle) * polar.Radius;

        // TODO: wave / signal functions 
        // https://en.wikipedia.org/wiki/Sawtooth_wave
        // https://en.wikipedia.org/wiki/Square_wave 

        // TODO: more polar functions
        // Lemniscates
    }

    [Operations]
    class DistanceField2DOperations
    {    
        // https://iquilezles.org/articles/distfunctions2d/
        
        double CircleSDF(Double2 p) 
            => CircleSDF(p, 1);
        
        double CircleSDF(Double2 p, double r) 
            => p.Length() - r;

        Double2 XY(Double4 d) => (d.X, d.Y);
        Double2 ZW(Double4 d) => (d.Z, d.W);

        double RoundedBoxSDF(Double2 p, Double2 size, Double4 r)
        {
            var xy = p.X > 0.0 ? r.XY() : r.ZW();
            var x = p.Y > 0.0 ? xy.X : xy.Y;
            var q = p.Abs() - size + x;
            return q.X.Max(q.Y).Min(0.0) + q.Max(Double2.Zero).Length() - x;
        }

        double BoxSDF(Double2 p, Double2 size)
        {
            var d = p.Abs() - size;
            return d.ClampPositive().Length() + d.X.Max(d.Y).ClampPositive();
        }

        // TODO: I need a "Matrix2"
        /*
        double Line(Double2 p, Line2D line, double thickness)
        {
            var d = (line.B - line.A) / line.Length();
            var q = (p - (line.A + line.B) * 0.5);
            q = Mat2(d.x, -d.y, d.y, d.x) * q;
            q = Abs(q) - new Double2(line.Length(), thickness) * 0.5;
            return q.ClampPositive().Length() + Max(q.x, q.y).ClampNegative();
        }*/

        double LineSDF(Double2 p, Line2D line)
        {
            var pa = p - line.A;
            // TODO: Point2D - Point2D == Vector2D
            var ba = line.B.Value - line.A.Value;
            var h = (pa.Dot(ba) / ba.Dot(ba)).Clamp(0.0, 1.0);
            return (pa - ba * h).Length();
        }

        double PolygonSDF(Double2 p, IArray<Double2> v)
        {
            var d = (p - v[0]).Dot(p - v[0]);
            var s = 1.0;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i, i++)
            {
                // distance
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * (w.Dot(e) / e.Dot(e)).ClampZeroToOne();
                d = d.Min(b.Dot(b));

                // winding number  http://geomalgorithms.com/a03-_inclusion.html
                var cond1 = p.Y >= v[i].Y;
                var cond2 = p.Y < v[j].Y;
                var cond3 = e.X * w.Y > e.Y * w.X;
                if (cond1 == cond2 && cond1 == cond3) s = -s;
            }

            return s * d.Sqrt();
        }

        double EquilateralTriangleSDF(Double2 p)
        {
            var k = 3.Sqrt();
            var x = p.X.Abs() - 1.0;
            var y = p.Y + 1.0 / k;
            var r = p;
            if (x + k * y > 0.0) r = new Double2(x - k * y, -k * x - y) / 2.0;
            r = r.WithX(r.X - p.X.Clamp(-2.0, 0.0));
            return -r.Length() * r.Y.Sign();
        }

        // Distance to a rounded "X" shape, given its width and thickness. It is exact in the exterior, and a bound in the interior
        double RoundXSDF(Double2 p, double w, double r)
        {
            p = p.Abs();
            return (p - (p.X + p.Y.Min(w) * 0.5)).Length() - r;
        }

        Func<Double2, double> RoundedSDF(Func<Double2, double> func, double r)
            => p => func(p) - r;

        Func<Double2, double> AnnularSDF(Func<Double2, double> func, double r)
            => p => func(p).Abs() - r;

        Func<Double2, double> RepeatSDF(Func<Double2, double> func, Double2 period)
            => p => func((p + new Double2(0.5, 0.5) * period).Modulo(period) - period * 0.5);
    }
}