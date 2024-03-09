using System;
using Plato.__TYPES__;
using PlatoStandardLibrary;

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
        Angle Radians(Number d) => d;
        Angle Turns(Number d) => d * 2 * Math.PI;
        Angle Degrees(Number d) => d.Turns() / 360.0;
        Angle Grads(Number d) => d.Turns() / 400.0;
        Angle ArcMinutes(Number d) => d.Degrees() / 60;
        Angle ArcSeconds(Number d) => d.ArcMinutes() / 60;
        Number ToTurns(Angle a) => a / 2 / Math.PI;
        Number ToDegrees(Angle a) => a.Turns * 360;
        Number ToGrads(Angle a) => a.Turns * 400;
        Number ToArcMinutes(Angle a) => a.Degrees * 60;
        Number ToArcSeconds(Angle a) => a.ArcMinutes * 60;

        // Proportion: percent of one 
        // https://en.wikipedia.org/wiki/Percentage
        // Some interesting discussions 
        // https://english.stackexchange.com/questions/275734/a-word-for-a-value-between-0-and-1-inclusive
        // https://twitter.com/cdiggins/status/1583610796331843584
        Proportion Proportion(Number d) => d;
        Proportion Percent(Number d) => d / 100;
        Proportion BasisPoints(Number d) => Percent(d / 100);
        Proportion Proportion(Angle a) => a.Turns;
        Number ToPercent(Proportion p) => p.Of(100);
        Number Of(Proportion p, Number amount) => p / amount;
        Number OfOne(Proportion p) => p.Value;
        Number ToBasisPoints(Proportion p) => p.Percent * 100;
        Angle ToAngle(Proportion p) => Turns(p);

        // Units of distance/length
        // https://en.wikipedia.org/wiki/Unit_of_length
        Length Meters(Number value) => value;
        Length Kilometer(Number value) => value * 1000;
        Length Centimeters(Number value) => value / 100;
        Length Decimeters(Number value) => value / 10;
        Length Millimeters(Number value) => value / 100;
        Length Microns(Number value) => value / 1000 / 1000;
        Length Nanometers(Number value) => value / 1000 / 1000 / 1000;
        Length Inches(Number value) => Feet(value / 12);
        Length Feet(Number value) => value / Constants.FeetPerMeter;
        Length Yards(Number value) => Feet(value * 3);
        Length Rods(Number value) => Chains(value / 4);
        Length Chains(Number value) => Yards(value * 22);
        Length Miles(Number value) => Feet(value * Constants.FeetPerMile);
        Length Leagues(Number value) => Miles(value * 3);
        Length Lightyears(Number value) => value * Constants.MetersPerLightyear;
        Length AU(Number value) => value * Constants.MetersPerAU;
        Length HubbleLength(Number value) => value * 14.4.Billion() * Lightyears(value);
        Number ToKilometers(Length length) => length.Meters / 1000;
        Number ToDecimeters(Length length) => length.Meters * 10;
        Number ToCentimeters(Length length) => length.Meters * 100;
        Number ToMillimeters(Length length) => length.Meters * 1000;
        Number ToMicrons(Length length) => length.Millimeters * 1000;
        Number ToNanometers(Length length) => length.Microns * 1000;
        Number ToInches(Length length) => length.Feet * 12;
        Number ToFeet(Length length) => length * Constants.FeetPerMeter;
        Number ToYards(Length length) => ToFeet(length) / 3;
        Number ToRods(Length length) => ToChains(length) * 4;
        Number ToChains(Length length) => ToYards(length) / 22;
        Number ToMiles(Length length) => length.Meters * Constants.FeetPerMeter / Constants.FeetPerMile;
        Number ToLeague(Length length) => ToMiles(length) / 3;
        Number ToLightyears(Length length) => length / Constants.MetersPerLightyear;
        Number ToAU(Length length) => length / Constants.MetersPerAU;
        Number ToHubbleLength(Number value) => value / 1.HubbleLength();

        // Units of weight or mass 
        // https://en.wikipedia.org/wiki/Mass
        Mass Milligrams(Number value) => value / 1000 / 1000;
        Mass Grams(Number value) => value / 1000;
        Mass Grains(Number value) => Milligrams(value * Constants.GrainToMilligram);
        Mass Kilograms(Number value) => value;
        Mass Dalton(Number value) => value * Constants.DaltonPerKilogram;
        Mass Tonne(Number value) => value * 1000;
        Mass Pound(Number value) => value * Constants.PoundPerKilogram;
        Mass Ton(Number value) => Pound(value * Constants.PoundPerTon);
        Mass SolarMass(Number value) => value * Constants.KilogramPerSolarMass;
        Mass Ounce(Number value) => Grams(value * Constants.OunceToGram);
        Number ToMilligrams(Mass m) => ToGrams(m) * 1000;
        Number ToGrams(Mass m) => m.Kilograms * 1000;
        Number ToGrains(Mass m) => ToMilligrams(m) / Constants.GrainToMilligram;
        Number ToDalton(Mass m) => m.Kilograms / Constants.DaltonPerKilogram;
        Number ToTonne(Mass m) => m.Kilograms / 1000;
        Number ToPound(Mass m) => m.Kilograms / Constants.PoundPerKilogram;
        Number ToTon(Mass m) => ToPound(m) / Constants.PoundPerTon;
        Number ToSolarMass(Mass m) => m / Constants.KilogramPerSolarMass;
        Number ToOunce(Mass m) => ToGrams(m) / Constants.OunceToGram;

        // Temperature      
        Temperature Celsius(Number value) => value;
        Temperature Kelvin(Number value) => value + 273.15;
        Temperature Faranheit(Number value) => (value - 32) * 5.0 / 9.0;
        Number ToKelvin(Temperature t) => t - 273.15;
        Number ToFaranheit(Temperature t) => t * 9.0 / 5.0 + 32.0;

        // Memory
        // https://en.wikipedia.org/wiki/Byte#Multiple-byte_units
        Memory Bytes(Number value) => value;
        Memory Bits(Number value) => value * 8;
        Memory Octets(Number value) => value;
        Memory Nibbles(Number value) => value / 2;
        Memory Kilobytes(Number value) => value * 1000;
        Memory Megabytes(Number value) => Kilobytes(value) * 1000;
        Memory Gigabytes(Number value) => Megabytes(value) * 1000;
        Memory Terabytes(Number value) => Gigabytes(value) * 1000;
        Memory Petabytes(Number value) => Terabytes(value) * 1000;
        Memory Exabytes(Number value) => Petabytes(value) * 1000;
        Memory Kebibytes(Number value) => value * 1024;
        Memory Mebibytes(Number value) => Kebibytes(value) * 1024;
        Memory Gibibytes(Number value) => Mebibytes(value) * 1024;
        Memory Tebibytes(Number value) => Gibibytes(value) * 1024;
        Memory Pebibytes(Number value) => Tebibytes(value) * 1024;
        Memory Exibytes(Number value) => Pebibytes(value) * 1024;
        Number ToBits(Memory m) => m.Bytes / 8;
        Number ToOctet(Memory m) => m.Bytes;
        Number ToNibble(Memory m) => m.Bytes / 2;
        Number ToKilobytes(Memory m) => m.Bytes / 1000;
        Number ToMegabytes(Memory m) => m.Kilobytes / 1000;
        Number ToGigabytes(Memory m) => m.Megabytes / 1000;
        Number ToTerabytes(Memory m) => m.Gigabytes / 1000;
        Number ToPetabytes(Memory m) => m.Terabytes / 1000;
        Number ToExabytes(Memory m) => m.Petabytes / 1000;
        Number ToKebibytes(Memory m) => m.Bytes / 1024;
        Number ToMebibytes(Memory m) => m.Kebibytes / 1024;
        Number ToGibibytes(Memory m) => m.Mebibytes / 1024;
        Number ToTebibytes(Memory m) => m.Gibibytes / 1024;
        Number ToPebibytes(Memory m) => m.Tebibytes / 1024;
        Number ToExibytes(Memory m) => m.Pebibytes / 1024;

        // Units of time
        // https://en.wikipedia.org/wiki/TU_(Time_Unit)
        Time Nanoseconds(Number value) => value / 1000 / 1000 / 10000;
        Time Microseconds(Number value) => value / 1000 / 1000;
        Time Milliseconds(Number value) => value / 1000;
        Time TimeUnits(Number value) => Microseconds(value * 1024);
        Time Seconds(Number value) => value;
        Time Minutes(Number value) => value * 60;
        Time Hours(Number value) => value * 60 * 60;
        Time Days(Number value) => value * 60 * 60 * 24;
        Time Weeks(Number value) => value * 60 * 60 * 24 * 7;
        Time JulianYears(Number value) => value * Constants.JulianYearSeconds;
        Time GregorianYears(Number value) => Days(value * Constants.GregorianYearDays);
        Number ToNanosecond(Time t) => t.Seconds * 1000 * 1000 * 1000;
        Number ToMicroeconds(Time t) => t.Seconds * 1000 * 1000;
        Number ToTimeUnits(Time t) => ToMicroeconds(t) / 1024;
        Number ToMilliseconds(Time t) => t.Seconds * 1000;
        Number ToMinutes(Time t) => t.Seconds / 60;
        Number ToHours(Time t) => t.Seconds / (60 * 60);
        Number ToDays(Time t) => t.Seconds / (60 * 60 * 24);
        Number ToWeeks(Time t) => t.Seconds / (60 * 60 * 24 * 7);
        Number ToJulianYears(Time t) => t.Seconds / Constants.JulianYearSeconds;
        Number ToGregorianYears(Time t) => Days(t) / Constants.GregorianYearDays;
        
        // https://en.wikipedia.org/wiki/Names_of_large_numbers
        // Some big numbers 
        Number Hundred(Number x) => x * Constants.Hundred;
        Number Thousand(Number x) => x * Constants.Thousand;
        Number Million(Number x) => x * Constants.Million;
        Number Billion(Number x) => x * Constants.Billion;
        Number Trillion(Number x) => x * Constants.Trillion;
        Number Tenth(Number x) => x / 10;
        Number Hundredth(Number x) => x / Constants.Hundred;
        Number Thousandth(Number x) => x / Constants.Thousand;
        Number Millionth(Number x) => x / Constants.Million;
        Number Billionth(Number x) => x / Constants.Billion;
        Number Trillionth(Number x) => x / Constants.Trillion;

        // https://en.wikipedia.org/wiki/Metric_prefix
        Number Quetta(Number x) => x * 1e+30;
        Number Ronna(Number x) => x * 1e+27;
        Number Yotta(Number x) => x * 1e+24;
        Number Zetta(Number x) => x * 1e+21;
        Number Exa(Number x) => x * 1e+18;
        Number Peta(Number x) => x * 1e+15;
        Number Tera(Number x) => x * 1e+12;
        Number Giga(Number x) => x * 1e+9;
        Number Mega(Number x) => x * 1e+6;
        Number Kilo(Number x) => x * 1000;
        Number Hecto(Number x) => x * 100;
        Number Deca(Number x) => x * 10;
        Number Deci(Number x) => x / 10;
        Number Centi(Number x) => x * 100;
        Number Milli(Number x) => x * 1e-3;
        Number Micro(Number x) => x * 1e-6;
        Number Nano(Number x) => x * 1e-9;
        Number Pico(Number x) => x * 1e-12;
        Number Femto(Number x) => x * 1e-15;
        Number Atto(Number x) => x * 1e-18;
        Number Zepto(Number x) => x * 1e-21;
        Number Yocto(Number x) => x * 1e-24;
        Number Ronto(Number x) => x * 1e-27;
        Number Quecto(Number x) => x * 1e-30;

        Number Reciprocal(Number x) => 1 / x;
        Number MultiplicativeInverse(Number x) => Reciprocal(x);

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
        Energy Kilojoules(Number d) => d.Thousand().Joules();
        Energy WattHours(Number d) => d * 1.Hours().Seconds;
        Energy NewtonMeters(Number d) => d;
        Energy Joules(Number d) => d;
        Density KilogramsPerMeterCubed(Number d) => d;
        Power Watts(Number d) => d;
        ElectricResistance Ohms(Number d) => d;
        ElectricCurrent Amperes(Number d) => d;
        ElectricCharge Columbs(Number d) => d;
        LuminousIntensity Candela(Number d) => d;
        Frequency Hertz(Number d) => d;
        Pressure Pascals(Number d) => d;
        Force Newtons(Number d) => d;
        Acceleration MetersPerSecondSquared(Number d) => d;
        Velocity MetersPerSecond(Number d) => d;
        Area MetersSquared(Number d) => d;
        Volume MetersCubed(Number d) => d;

        // TODO: volume
        // https://en.wikipedia.org/wiki/Fluid_ounce
    }

    [Operations]
    class VectorOperations
    {
        Vector2D Normal(Vector2D v) => v / v.Length();
        Vector3D Normal(Vector3D v) => v / v.Length();
        Vector4D Normal(Vector4D v) => v / v.Length();
        Float2 Normal(Float2 v) => v / (float)v.Length();
        Float3 Normal(Float3 v) => v / (float)v.Length();
        Float4 Normal(Float4 v) => v / (float)v.Length();
        Number Length(Line2D line) => Distance(line.A, line.B);
        Number Distance(Vector2D a, Vector2D b) => (b - a).Length();
        Number Distance(Vector3D a, Vector3D b) => (b - a).Length();
        Number Distance(Vector4D a, Vector4D b) => (b - a).Length();
        Number Distance(Float2 a, Float2 b) => (b - a).Length();
        Number Distance(Float3 a, Float3 b) => (b - a).Length();
        Number Distance(Float4 a, Float4 b) => (b - a).Length();
    }

    [Vectorized]
    class VectorizedOperations
    {
        Number SafeDivide(Number x, Number y) => y.AlmostZero() ? x : x / y;
        Number Half(Number x) => x * 0.5;
        Number Quarter(Number x) => x * 0.25;
        Number Twice(Number x) => x * 2;
        Number Thrice(Number x) => x * 3;
        Number MinusOne(Number x) => x - 1;
        Number PlusOne(Number x) => x + 1;
        Number OneMinus(Number x) => 1 - x;
        Number Abs(Number x) => Math.Abs(x);
        Number Exp(Number x) => Math.Exp(x);
        Number Log(Number x) => Math.Log(x);
        Number Log10(Number x) => Math.Log10(x);
        Number Sqrt(Number x) => Math.Sqrt(x);
        Number Sign(Number x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        Number Truncate(Number x) => Math.Truncate(x);
        Number Ceiling(Number x) => Math.Ceiling(x);
        Number Floor(Number x) => Math.Floor(x);
        Number Round(Number x) => Math.Round(x);
        Number Smoothstep(Number v) => v.Pow2() * (3 - 2 * v);
        Number Lerp(Number v1, Number v2, Number t) => v1 * (1 - t) + v2 * t;
        Number Mix(Number v1, Number v2, Number t) => Lerp(v1, v2, t);
        Number InverseLerp(Number v, Number a, Number b) => (v - a) / (b - a);
        Number Unmix(Number v, Number a, Number b) => InverseLerp(v, a, b);
        Number Clamp(Number v, Number min, Number max) => Max(Min(v, max), min);
        Number ClampZeroToOne(Number v) => Clamp(v, 0, 1);
        Number Average(Number v1, Number v2) => Lerp(v1, v2, 0.5);
        Number Barycentric(Number v1, Number v2, Number v3, Vector2D uv) => v1 + (v2 - v1) * uv.X + (v3 - v1) * uv.Y;
        Number Min(Number v1, Number v2) => Math.Min(v1, v2);
        Number Max(Number v1, Number v2) => Math.Max(v1, v2);
        Number ClampPositive(Number v) => Math.Max(v, 0);
        Number ClampNegative(Number v) => Math.Min(v, 0);
        Number Pow2(Number x) => x * x;
        Number Pow3(Number x) => x * x * x;
        Number Pow4(Number x) => x * x * x * x;
        Number Pow5(Number x) => x * x * x * x * x;
        Number Pow(Number x, Number y) => Math.Pow(x, y);
    }

    [Operations]
    class TrigOperations
    {
        Angle Acos(Number x) => Math.Acos(x);
        Angle Asin(Number x) => Math.Asin(x);
        Angle Atan(Number x) => Math.Atan(x);

        // Extremely esoteric functions.
        Number Versin(Angle x) => 1 - Cos(x);
        Number Vercosin(Angle x) => 1 + Cos(x);
        Number Coversin(Angle x) => 1 - Sin(x);
        Number Covercosin(Angle x) => 1 + Sin(x);
        Number Haversin(Angle x) => Versin(x) / 2;
        Number Havercosin(Angle x) => Vercosin(x) / 2;
        Number Hacoversin(Angle x) => Coversin(x) / 2;
        Number Hacovercosin(Angle x) => Coversin(x) / 2;
        Number Exsec(Angle x) => Sec(x) - 1;
        Number Excsc(Angle x) => Csc(x) - 1;

        Number Cos(Angle x) => Math.Cos(x);
        Number Cosh(Angle x) => Math.Cosh(x);
        Number Sin(Angle x) => Math.Sin(x);
        Number Sinh(Angle x) => Math.Sinh(x);
        Number Tan(Angle x) => Math.Tan(x);
        Number Tanh(Angle x) => Math.Tanh(x);

        Number Sec(Angle x) => Cos(x).Reciprocal();
        Number Csc(Angle x) => Sin(x).Reciprocal();
        Number Cot(Angle x) => Tan(x).Reciprocal();
    }

    [Operations]
    class ComparisonOperations
    {
        bool GtZ(Number x) => x > 0;
        bool LtZ(Number x) => x < 0;
        bool GtEqZ(Number x) => x >= 0;
        bool LtEqZ(Number x) => x <= 0;
        bool Gt(Number x, Number y) => x > y;
        bool Lt(Number x, Number y) => x > y;
        bool GtEq(Number x, Number y) => x >= y;
        bool LtEq(Number x, Number y) => x <= y;
        bool IsInfinity(Number v) => Number.IsInfinity(v);
        bool IsNaN(Number v) => Number.IsNaN(v);
        bool AlmostZero(Number v) => v.AlmostEquals(0);
        bool AlmostOne(Number v) => v.AlmostEquals(1);
        bool AlmostZeroOrOne(Number v) => v.AlmostZero() && v.AlmostOne();
        bool Within(Number v, Number min, Number max) => v >= min && v < max;
    }

    [Operations]
    class IntervalOperations
    {
        Number Size(Interval interval) => (interval.A - interval.B).Abs();
        Interval ResetLeftToZero(Interval interval) => (0, Size(interval));
        Interval ResetRightToZero(Interval interval) => (-Size(interval), 0);
        Number HalfSize(Interval interval) => (interval.A - interval.B).Abs().Half();
        Interval Invert(Interval interval) => (interval.B, interval.A);
        Interval Normalize(Interval interval) => (interval.A.Min(interval.B), interval.B.Max(interval.B));
        Number Clamp(Number value, Interval interval) => value.Clamp(interval.A, interval.B);
        Number Lerp(Number value, Interval interval) => value.Lerp(interval.A, interval.B);
        Number InverseLerp(Number value, Interval interval) => value.InverseLerp(interval.A, interval.B);
        Number Clamp(Interval interval, Number value) => Clamp(value, interval);
        Number Lerp(Interval interval, Number value) => Lerp(value, interval);
        Number InverseLerp(Interval interval, Number value) => InverseLerp(value, interval);
        Number Remap(Number value, Interval input, Interval output) => Lerp(InverseLerp(value, input), output);
        Number Center(Interval interval) => Lerp(0.5, interval);
        Interval Left(Interval interval, Number amount) => (interval.A, Lerp(amount, interval));
        Interval Right(Interval interval, Number amount) => (Lerp(amount, interval), interval.B);
        (Interval, Interval) Split(Interval interval, Number amount) => (Left(interval, amount), Right(interval, amount));
        Interval Resize(Interval interval, Number amount) => (Center(interval) - amount / 2, Center(interval) + amount * 2);
        Interval ResizeRelative(Interval interval, Number amount) => Resize(interval, Size(interval) * amount);
        Interval Multiply(Interval interval, Number amount) => ResizeRelative(interval, Size(interval) * amount);
        Interval Divide(Interval interval, Number amount) => Multiply(interval, 1.0 / amount);
        Interval Offset(Interval interval, Number amount) => (interval.A + amount, interval.B + amount);
        Interval CenterAt(Interval interval, Number value = 0) => Offset(interval, value - Center(interval));
    }

    [Operations]
    class EasingOperations
    {
        //===
        // Easings.cs
        // https://easings.net/
        // https://github.com/acron0/Easings/blob/master/Easings.cs

        Func<Number, Number> BlendEaseFunc(Func<Number, Number> easeIn, Func<Number, Number> easeOut) => p => p < 0.5 ? 0.5 * easeIn(p * 2) : 0.5 * easeOut(p * 2 - 1) + 0.5;
        Func<Number, Number> InvertEaseFunc(Func<Number, Number> easeIn) => p => 1 - easeIn(1 - p);

        Number Linear(Number p) => p;
        Number QuadraticEaseIn(Number p) => p.Pow2();
        Number QuadraticEaseOut(Number p) => InvertEaseFunc(QuadraticEaseIn)(p);
        Number QuadraticEaseInOut(Number p) => BlendEaseFunc(QuadraticEaseIn, QuadraticEaseOut)(p);
        Number CubicEaseIn(Number p) => p.Pow3();
        Number CubicEaseOut(Number p) => InvertEaseFunc(CubicEaseIn)(p);
        Number CubicEaseInOut(Number p) => BlendEaseFunc(CubicEaseIn, CubicEaseOut)(p);
        Number QuarticEaseIn(Number p) => p.Pow4();
        Number QuarticEaseOut(Number p) => InvertEaseFunc(QuarticEaseIn)(p);
        Number QuarticEaseInOut(Number p) => BlendEaseFunc(QuarticEaseIn, QuarticEaseOut)(p);
        Number QuinticEaseIn(Number p) => p.Pow5();
        Number QuinticEaseOut(Number p) => InvertEaseFunc(QuinticEaseIn)(p);
        Number QuinticEaseInOut(Number p) => BlendEaseFunc(QuinticEaseIn, QuinticEaseOut)(p);
        Number SineEaseIn(Number p) => InvertEaseFunc(SineEaseOut)(p);
        Number SineEaseOut(Number p) => p.Quarter().Turns().Sin();
        Number SineEaseInOut(Number p) => BlendEaseFunc(SineEaseIn, SineEaseOut)(p);
        Number CircularEaseIn(Number p) => 1 - (1 - p.Pow2()).Sqrt();
        Number CircularEaseOut(Number p) => InvertEaseFunc(CircularEaseIn)(p);
        Number CircularEaseInOut(Number p) => BlendEaseFunc(CircularEaseIn, CircularEaseOut)(p);
        Number ExponentialEaseIn(Number p) => p.AlmostZero() ? p : 2.Pow(10 * (p - 1));
        Number ExponentialEaseOut(Number p) => InvertEaseFunc(ExponentialEaseIn)(p);
        Number ExponentialEaseInOut(Number p) => BlendEaseFunc(ExponentialEaseIn, ExponentialEaseOut)(p);
        Number ElasticEaseIn(Number p) => (13 * p.Quarter().Turns()) * 2.Pow(10 * (p - 1)).Radians().Sin();
        Number ElasticEaseOut(Number p) => InvertEaseFunc(ElasticEaseIn)(p);
        Number ElasticEaseInOut(Number p) => BlendEaseFunc(ElasticEaseIn, ElasticEaseOut)(p);
        Number BackEaseIn(Number p) => p.Pow3() - p * p.Half().Turns().Sin();
        Number BackEaseOut(Number p) => InvertEaseFunc(BackEaseIn)(p);
        Number BackEaseInOut(Number p) => BlendEaseFunc(BackEaseIn, BackEaseOut)(p);
        Number BounceEaseIn(Number p) => InvertEaseFunc(BounceEaseOut)(p);
        Number BounceEaseOut(Number p)
        {
            if (p < 4 / 11.0) return 121.0 * p.Pow2() / 16.0;
            if (p < 8 / 11.0) return 363.0 / 40.0 * p.Pow2() - 99.0 / 10.0 * p + 17.0 / 5.0;
            if (p < 9 / 10.0) return 4356.0 / 361.0 * p.Pow2() - 35442.0 / 1805.0 * p + 16061.0 / 1805.0;
            return 54.0 / 5.0 * p.Pow2() - 513.0 / 25.0 * p + 268.0 / 25.0;
        }
        Number BounceEaseInOut(Number p) => BlendEaseFunc(BounceEaseIn, BounceEaseOut)(p);
    }

    [Operations]
    class ShapingOperations
    {
        //===
        // Signal or Shaping functions
        // https://iquilezles.org/articles/functions/
        // https://thebookofshaders.com/05/

        Number ExponentialImpulse(Number x, Number k) => k * x * (1.0 - k * x).Exp();
        Number QuadraticImpulse(Number x, Number k) => 2.0 * k.Sqrt() * x / (1.0 + k * x * x);
        Number PolynomalialImpulse(Number x, Number k, Number n) => (n / (n - 1.0)) * ((n - 1.0) * k).Pow(1.0 / n) * x / (1.0 + k * x.Pow(n));
        Number NormalizedPowerCurve(Number x, Number a, Number b) => (a + b.Pow(a + b) / (a.Pow(a) * (b.Pow(b)) * UnnormalizedPowerCurve(x, a, b)));
        Number UnnormalizedPowerCurve(Number x, Number a, Number b) => x.Pow(a) * (1.0 - x).Pow(b);
        Number Parabola(Number x, Number k) => 4.0 * x * (1.0 - x).Pow(k);
        Number Sinc(Number x, Number k) { var a = k * (x - 1.0).Half().Turns(); return a.Sin() / a; }
        Number Gain(Number x, Number k) { var a = 0.5 * (2.0 * ((x < 0.5) ? x : 1.0 - x).Pow(k)); return (x < 0.5) ? a : 1.0 - a; }
        Number ExponentialStep(Number x, Number k, Number n) => (-k * x.Pow(n)).Exp();
        Number NearIdentityCubic(Number x, Number threshold, Number constant = 0) { if (x > threshold) return x; var a = 2.0 * constant - threshold; var b = 2.0 * threshold - 3.0 * constant; var t = x / threshold; return (a * t + b) * t * t + constant; }
        Number NearIdentitySqrt(Number x, Number constant = 0) => (x * x + constant).Sqrt();
        Number NearUnityIdentity(Number x) => x * x * (2.0 - x);
        Number IntegralSmoothstep(Number x, Number t) => (x > t) ? x - t / 2.0 : (x).Pow3() * (1.0 - x * 0.5 / t) / t / t;
    }

    [Operations]
    class CurveOperations
    {
        //===
        // http://www.flong.com/archive/texts/code/shapers_circ/

        Number CircleSigmoid(Number x, Number a = 0.5) => x <= a
            ? a - (a.Pow2() - x.Pow2()).Sqrt()
            : a + ((1 - a).Pow2() - (x - 1).Pow2().Pow2()).Sqrt();

        Number CircularSeat(Number x, Number a = 0.5) =>
            x <= a ? (a.Pow2() - (x - a).Pow2()).Sqrt() : ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        Number EllipticalSeat(Number x, Number a, Number b) => (a.AlmostZeroOrOne()) ? a :
            (x <= a) ? (b / a) * (a.Pow2() - (x - a).Pow2()).Sqrt() :
            1 - ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();

        Number EllipticalSigmoid(Number x, Number a, Number b) => a.AlmostZeroOrOne() ? a :
            (x <= a) ? b * (1 - ((a.Pow2() - x.Pow2()) / a).Sqrt()) :
            b + ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - 1).Pow2()).Sqrt();

            // https://en.wikipedia.org/wiki/Catenary
            Number Caternay(Number x, Number a = 1.0) => (x / a).Radians().Cosh();

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

        Vector2D Parabola(Number x)
            => (x, x.Pow2());

        Vector2D Hyperbola(Angle t)
            => (t.Sec(), t.Tan());

        Vector2D LeminscateOfGerono(Angle t)
            => (t.Cos(), t.Sin() * t.Cos());

        Vector2D Circle(Angle t)
            => (t.Cos(), t.Sin());

        Vector2D Lissajous(Angle t, Number a, Number b, Number kx, Number ky)
            => (a * (kx * t).Cos(),
                b * (ky * t).Sin());

        Vector2D Hypotrochoid(Angle t, Number r, Number d)
            => ((1 - r) * t.Cos() + d * (t - 1.0).Cos(),
                (1 - r) * t.Sin() + d * (t - 1.0).Sin());

        Vector2D Epicycloid(Angle t, Number r, Number k)
            => (r * (k + 1) * t.Cos() - r * ((k + 1) * t).Cos(),
                r * (k + 1) * t.Sin() - r * ((k + 1) * t).Sin());

        Vector2D ClosedEpicycloid(Angle t, int k) => Epicycloid(t, 1.0 / k, 1);
        Vector2D Cardoid(Angle t) => ClosedEpicycloid(t, 1);
        Vector2D Nephroid(Angle t) => ClosedEpicycloid(t, 2);
        Vector2D Trefoiloid(Angle t) => ClosedEpicycloid(t, 3);
        Vector2D Quatrefoiloid(Angle t) => ClosedEpicycloid(t, 4);

        Vector2D Hypocycloid(Angle t, Number r, Number k)
            => (r * (k - 1) * t.Cos() + r * ((k - 1) * t).Cos(),
                r * (k - 1) * t.Sin() + r * ((k - 1) * t).Cos());

        Vector2D ClosedHypocycloid(Angle t, int k) => Hypocycloid(t, 1.0 / k, 1);
        Vector2D Deltoid(Angle t) => ClosedHypocycloid(t, 3);
        Vector2D Astroid(Angle t) => ClosedHypocycloid(t, 4);

        Vector2D Epitrochoid(Angle t, Number r, Number d)
            => ((1 + r) * t.Cos() - d * ((1 + r) / r * t).Cos(),
                (1 + r) * t.Sin() - d * ((1 + r) / r * t).Sin());

        // TODO: there are some cute examples in Wikipedia at https://en.wikipedia.org/wiki/Parametric_equation, but no names
        Vector2D GeneralizedParametricCurve(Angle t, Number a, Number b, Number c, Number d, Number j, Number k)
            => ((a * t).Cos() - (b * t).Cos().Pow(j),
                (c * t).Sin() - (d * t).Sin().Pow(k));

        Number PolarLimacon(Angle t, Number a, Number b)
            => b + a * t.Cos();

        Vector2D CartesianLimacon(Angle t, Number a = 1, Number b = 1)
            => ToCartesian((PolarLimacon(t, a, b), t));

        Vector2D ToCartesian(PolarCoordinate polar)
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
        
        Number CircleSDF(Vector2D p) 
            => CircleSDF(p, 1);
        
        Number CircleSDF(Vector2D p, Number r) 
            => p.Length() - r;

        Vector2D XY(Vector4D d) => (d.X, d.Y);
        Vector2D ZW(Vector4D d) => (d.Z, d.W);

        Number RoundedBoxSDF(Vector2D p, Vector2D size, Vector4D r)
        {
            var xy = p.X > 0.0 ? r.XY() : r.ZW();
            var x = p.Y > 0.0 ? xy.X : xy.Y;
            var q = p.Abs() - size + x;
            return q.X.Max(q.Y).Min(0.0) + q.Max(Vector2D.Zero).Length() - x;
        }

        Number BoxSDF(Vector2D p, Vector2D size)
        {
            var d = p.Abs() - size;
            return d.ClampPositive().Length() + d.X.Max(d.Y).ClampPositive();
        }

        // TODO: I need a "Matrix2"
        /*
        Number Line3D(Vector2D p, Line2D line3D, Number thickness)
        {
            var d = (line3D.B - line3D.A) / line3D.Length();
            var q = (p - (line3D.A + line3D.B) * 0.5);
            q = Mat2(d.x, -d.y, d.y, d.x) * q;
            q = Abs(q) - new Vector2D(line3D.Length(), thickness) * 0.5;
            return q.ClampPositive().Length() + Max(q.x, q.y).ClampNegative();
        }*/

        Number LineSDF(Vector2D p, Line2D line)
        {
            var pa = p - line.A;
            // TODO: Point2D - Point2D == Vector2D
            var ba = line.B.Value - line.A.Value;
            var h = (pa.Dot(ba) / ba.Dot(ba)).Clamp(0.0, 1.0);
            return (pa - ba * h).Length();
        }

        Number PolygonSDF(Vector2D p, IArray<Vector2D> v)
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

        Number EquilateralTriangleSDF(Vector2D p)
        {
            var k = 3.Sqrt();
            var x = p.X.Abs() - 1.0;
            var y = p.Y + 1.0 / k;
            var r = p;
            if (x + k * y > 0.0) r = new Vector2D(x - k * y, -k * x - y) / 2.0;
            r = r.WithX(r.X - p.X.Clamp(-2.0, 0.0));
            return -r.Length() * r.Y.Sign();
        }

        // Distance to a rounded "X" shape, given its width and thickness. It is exact in the exterior, and a bound in the interior
        Number RoundXSDF(Vector2D p, Number w, Number r)
        {
            p = p.Abs();
            return (p - (p.X + p.Y.Min(w) * 0.5)).Length() - r;
        }

        Func<Vector2D, Number> RoundedSDF(Func<Vector2D, Number> func, Number r)
            => p => func(p) - r;

        Func<Vector2D, Number> AnnularSDF(Func<Vector2D, Number> func, Number r)
            => p => func(p).Abs() - r;

        Func<Vector2D, Number> RepeatSDF(Func<Vector2D, Number> func, Vector2D period)
            => p => func((p + (0.5, 0.5) * period) % period - period * 0.5);
    }
}