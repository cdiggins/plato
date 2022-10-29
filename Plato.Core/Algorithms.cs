namespace Plato.Definitions
{
    class VectorAlgorithms
    {
        float Dot(Float3 a, Float3 b) => (a * b).Sum();
        double Dot(Double3 a, Double3 b) => (a * b).Sum();
    }

    class Algorithms
    {
        Time FromNanoseconds(double value) => value / 1000 / 1000 / 10000;
        Time FromMicroseconds(double value) => value / 1000 / 1000;
        Time FromMilliseconds(double value) => value / 1000;
        Time FromSeconds(double value) => value;
        Time FromMinutes(double value) => value * 60;
        Time FromHours(double value) => value * 60 * 60;
        Time FromDays(double value) => value * 60 * 60 * 24;
        Time FromWeeks(double value) => value * 60 * 60 * 24 * 7;

        double ToNanosecond(Time t) => t.Seconds * 1000 * 1000 * 1000;
        double ToMicroeconds(Time t) => t.Seconds * 1000 * 1000;
        double ToMilliseconds(Time t) => t.Seconds * 1000;
        double ToSeconds(Time t) => t.Seconds;
        double ToMinutes(Time t) => t.Seconds / 60;
        double ToHours(Time t) => t.Seconds / (60 * 60);
        double ToDays(Time t) => t.Seconds / (60 * 60 * 24);
        double ToWeeks(Time t) => t.Seconds / (60 * 60 * 24 * 7);

        Percent FromUnit(double value) => value * 100.0;
        Unit ToUnit() => Value / 100.0;
        implicit operator double(Percent amount) => amount.Value;
        implicit operator Percent(double value) => new Percent(value);


        Unit FromPercent(double percent) => percent / 100.0;
        Percent ToPercent() => 100.0 * Value;
        implicit operator double(Unit amount) => amount.Value;
        implicit operator Unit(double value) => new Unit(value);

        double FeetPerMeter = 3.280839895;
        double FeetPerMile = 5280;
        double MetersPerLightyear = 9.46073047258e+15;
        double MetersPerAU = 149597870691;
        string InternalUnit = nameof(Radians);
        Angle FromRadians(double radians) => new Angle(radians);
        Angle FromRevolutions(double revolutions) => revolutions * RadiansPerRevolution;
        Angle FromDegrees(double degrees) => FromRevolutions(DegreesPerRevolution / degrees);
        double ToDegrees() => DegreesPerRevolution * ToRevolutions();
        double ToRevolutions() => Radians / RadiansPerRevolution;
        double ToRadians() => Radians;
        implicit operator double(Angle angle) => angle.Radians;
        implicit operator Angle(double radians) => new Angle(radians);

        string InternalUnit = nameof(Meters);
        Distance FromMeters(double value) => value;
        Distance FromKilometer(double value) => value * 1000;
        Distance FromCentimeters(double value) => value / 100;
        Distance FromMillimeters(double value) => value / 100;
        Distance FromMicrons(double value) => value / 1000 / 1000;
        Distance FromNanometers(double value) => value / 1000 / 1000 / 1000;
        Distance FromInches(double value) => FromFeet(value / 12);
        Distance FromFeet(double value) => value / FeetPerMeter;
        Distance FromYards(double value) => FromFeet(value * 3);
        Distance FromMiles(double value) => FromFeet(value * FeetPerMile);
        Distance FromLightyears(double value) => value * MetersPerLightyear;
        Distance FromAU(double value) => value * MetersPerAU;
        double ToMeters() => Meters;
        double ToKilometers() => Meters / 1000;
        double ToCentimeters() => Meters * 100;
        double ToMillimeters() => Meters * 1000;
        double ToMicrons() => ToMillimeters() * 1000;
        double ToNanometers() => ToMicrons() * 1000;
        double ToInches() => ToFeet() * 12;
        double ToFeet() => Meters * FeetPerMeter;
        double ToYards() => ToFeet() / 3;
        double ToMiles() => Meters * FeetPerMeter / FeetPerMile;
        double ToLightuears() => Meters / MetersPerLightyear;
        double ToAU() => Meters / MetersPerAU;

        implicit operator double(Distance distance) => distance.Meters;
        implicit operator Distance(double meters) => new Distance(meters);

        double FeetPerMeter = 3.280839895;
        double FeetPerMile = 5280;
        double MetersPerLightyear = 9.46073047258e+15;
        double MetersPerAU = 149597870691;
        string InternalUnit = nameof(Radians);
        Angle FromRadians(double radians) => new Angle(radians);
        Angle FromRevolutions(double revolutions) => revolutions * RadiansPerRevolution;
        Angle FromDegrees(double degrees) => FromRevolutions(DegreesPerRevolution / degrees);
        double ToDegrees() => DegreesPerRevolution * ToRevolutions();
        double ToRevolutions() => Radians / RadiansPerRevolution;
        double ToRadians() => Radians;
        implicit operator double(Angle angle) => angle.Radians;
        implicit operator Angle(double radians) => new Angle(radians);

        string InternalUnit = nameof(Meters);
        Distance FromMeters(double value) => value;
        Distance FromKilometer(double value) => value * 1000;
        Distance FromCentimeters(double value) => value / 100;
        Distance FromMillimeters(double value) => value / 100;
        Distance FromMicrons(double value) => value / 1000 / 1000;
        Distance FromNanometers(double value) => value / 1000 / 1000 / 1000;
        Distance FromInches(double value) => FromFeet(value / 12);
        Distance FromFeet(double value) => value / FeetPerMeter;
        Distance FromYards(double value) => FromFeet(value * 3);
        Distance FromMiles(double value) => FromFeet(value * FeetPerMile);
        Distance FromLightyears(double value) => value * MetersPerLightyear;
        Distance FromAU(double value) => value * MetersPerAU;
        double ToMeters() => Meters;
        double ToKilometers() => Meters / 1000;
        double ToCentimeters() => Meters * 100;
        double ToMillimeters() => Meters * 1000;
        double ToMicrons() => ToMillimeters() * 1000;
        double ToNanometers() => ToMicrons() * 1000;
        double ToInches() => ToFeet() * 12;
        double ToFeet() => Meters * FeetPerMeter;
        double ToYards() => ToFeet() / 3;
        double ToMiles() => Meters * FeetPerMeter / FeetPerMile;
        double ToLightuears() => Meters / MetersPerLightyear;
        double ToAU() => Meters / MetersPerAU;

        implicit operator double(Distance distance) => distance.Meters;
        implicit operator Distance(double meters) => new Distance(meters);

        Temperature(double celsius) => Celsius = celsius;

        Temperature FromCelsius(double value) => value;
        Temperature FromKelvin(double value) => value - 273.15;
        Temperature FromFaranheit(double value) => (value - 32.0) * 5.0 / 9.0;

        double ToCelsius() => Celsius;
        double ToKelvin() => Celsius + 273.15;
        double ToFaranheit() => (Celsius * 9.0 / 5.0) + 32.0;

        implicit operator double(Temperature temperature) => temperature.Celsius;
        implicit operator Temperature(double celsius) => new Temperature(celsius);
        double DaltonPerKilogram = 1.66053e-27;
        double PoundPerKilogram = 0.45359237;
        double PoundPerTon = 2000;
        double KilogramPerSolarMass = 1.9889200011446E+30;

        string InternalUnit = nameof(Kilograms);
        Mass FromMilligrams(double value) => value / 1000 / 1000;
        Mass FromGrams(double value) => value / 1000;
        Mass FromKilograms(double value) => value;
        Mass FromDalton(double value) => value * DaltonPerKilogram;
        Mass FromTonne(double value) => value * 1000;
        Mass FromPound(double value) => value * PoundPerKilogram;
        Mass FromTon(double value) => FromPound(value * PoundPerTon);
        Mass FromSolarMass(double value) => value * KilogramPerSolarMass;

        double ToMilligrams() => ToGrams() * 1000;
        double ToGrams() => Kilograms * 1000;
        double ToKilograms() => Kilograms;
        double ToDalton() => Kilograms / DaltonPerKilogram;
        double ToTonne() => Kilograms / 1000;
        double ToPound() => Kilograms / PoundPerKilogram;
        double ToTon() => ToPound() / PoundPerTon;
        double ToSolarMass() => Kilograms / KilogramPerSolarMass;

        implicit operator double(Mass mass) => mass.Kilograms;
        implicit operator Mass(double kilograms) => new Mass(kilograms);
    }
}