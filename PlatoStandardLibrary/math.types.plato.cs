using System.Globalization;

namespace Plato.__TYPES__     
{
    // These are the built-in types. These are usually defined in the target language. 

    [Intrinsic, Numerical]
    class Integer
    {
        long Value;
    }

    [Intrinsic, Numerical]
    class Count
    {
        long Value;
    }

    [Intrinsic, Value]
    class Index
    {
        long Value;
    }

    [Intrinsic, Numerical]
    class Number
    {
        double Value;
    }

    // The rest of the types are defined in terms of each other and the intrinsics.  

    [Numerical]
    class Unit
    {
        Number Value;
    }

    [Numerical]
    class Percent
    {
        Number Value;
    }

    [Value]
    class Quaternion 
    {
        Number X, Y, Z, W;
    }

    [Value]
    class Unit2D
    {
        Unit X, Y;
    }

    [Value]
    class Unit3D 
    {
        Unit X, Y, Z;
    }

    [Value]
    class Direction3D
    {
        Unit3D Value;
    }

    [Value]
    class AxisAngle 
    {
        Unit3D Axis;
        Angle Angle;
    }

    [Value]
    class EulerAngles 
    {
        Angle Yaw, Pitch, Roll;
    }

    [Value]
    class Rotation3D
    {
        Quaternion Quaternion;
    }

    [Vector]
    class Vector2D
    {
        Number X, Y;
    }

    [Vector]
    class Vector3D
    {
        Number X, Y, Z;
    }

    [Vector]
    class Vector4D
    {
        Number X, Y, Z, W;
    }

    [Value]
    class Orientation3D
    {
        Rotation3D Value;
    }

    [Value]
    class Pose2D 
    {
        Vector3D Position;
        Orientation3D Orientation;
    }

    [Value]
    class Pose3D
    {
        Vector3D Position;
        Orientation3D Orientation;
    }

    [Value]
    class Transform3D 
    {
        Vector3D Translation;
        Rotation3D Rotation;
        Vector3D Scale;
    }

    [Value]
    class Transform2D
    {
        Vector2D Translation;
        Angle Rotation;
        Vector2D Scale;
    }

    [Interval]
    class AlignedBox2D
    {
        Vector2D A, B;
    }

    [Interval]
    class AlignedBox3D 
    {
        Vector3D A, B;
    }

    [Vector]
    class Complex 
    {
        Number Real, Imaginary;
    }

    [Value]
    class Ray3D 
    {
        Vector3D Direction;
        Point3D Position;
    }

    [Value]
    class Ray2D
    {
        Vector2D Direction;
        Point2D Position;
    }

    [Value]
    class Sphere
    {
        Point3D Center;
        Number Radius;
    }

    [Value]
    class Plane 
    {
        Unit3D Normal;
        Number D;
    }

    [Value]
    class Triangle3D
    {
        Point3D A, B, C;
    }

    [Value]
    class Triangle2D 
    {
        Point2D A, B, C;
    }

    [Value]
    class Quad3D 
    {
        Point3D A, B, C, D;
    }

    [Value]
    class Quad2D
    {
        Point2D A, B, C, D;
    }

    [Value]
    class Point3D 
    {
        Vector3D Value;
    }

    [Value]
    class Point2D 
    {
        Vector2D Value;
    }

    [Interval]
    class Line3D 
    {
        Point3D A, B;
    }

    [Interval]
    class Line2D 
    {
        Point2D A, B;
    }

    // https://en.wikipedia.org/wiki/RGB_color_spaces
    [Value]
    class Color 
    {
        Unit R, G, B, A;
    }

    // https://en.wikipedia.org/wiki/CIELUV
    [Value]
    class ColorLUV
    {
        Percent Lightness;
        Unit U, V;
    }

    // https://en.wikipedia.org/wiki/CIELAB_color_space
    [Value]
    class ColorLAB
    {
        Percent Lightness;
        Integer A, B;
    }

    // https://en.wikipedia.org/wiki/CIELAB_color_space#Cylindrical_model
    [Value]
    class ColorLCh
    {
        Percent Lightness;
        PolarCoordinate ChromaHue;
    }

    // https://en.wikipedia.org/wiki/HSL_and_HSV
    [Value]
    class ColorHSV 
    {
        Angle Hue;
        Unit S, V;
    }
    
    // https://en.wikipedia.org/wiki/HSL_and_HSV
    [Value]
    class ColorHSL
    {
        Angle Hue;
        Unit Saturation, Luminance;
    }

    // https://en.wikipedia.org/wiki/YCbCr
    [Value]
    class ColorYCbCr
    {
        Unit Y, Cb, Cr;
    }

    // https://en.wikipedia.org/wiki/Spherical_coordinate_system
    [Value]
    class SphericalCoordinate
    {
        Number Radius;
        Angle Azimuth;
        Angle Polar ;
    }

    // https://en.wikipedia.org/wiki/Polar_coordinate_system
    [Value]
    class PolarCoordinate
    {
        Number Radius;
        Angle Angle;
    }

    // https://en.wikipedia.org/wiki/Log-polar_coordinates
    [Value]
    class LogPolarCoordinate
    {
        Number Rho;
        Angle Azimuth;
    }

    // https://en.wikipedia.org/wiki/Cylindrical_coordinate_system
    [Value]
    class CylindricalCoordinate
    {
        Number RadialDistance;
        Angle Azimuth;
        Number Height;
    }

    // https://en.wikipedia.org/wiki/Horizontal_coordinate_system
    [Value]
    class HorizontalCoordinate
    {
        Number Radius;
        Angle Azimuth;
        Number Height;
    }

    // https://en.wikipedia.org/wiki/Geographic_coordinate_system
    [Value]
    class GeoCoordinate
    {
        Angle Latitude, Longitude;
    }

    // https://en.wikipedia.org/wiki/Geographic_coordinate_system
    [Value]
    class GeoCoordinteWithAltitude
    {
        GeoCoordinate Coordinate;
        Number Altitude;
    }

    [Value]
    class Circle
    {
        Point2D Center;
        Number Radius;
    }

    [Value]
    class Chord
    {
        Circle Circle;
        Arc Arc;
    }

    [Value]
    class Size2D
    {
        Number Width, Height;
    }

    [Value]
    class Size3D
    {
        Number Width, Height, Depth;
    }

    [Value]
    class Rectangle2D
    {
        Point2D Center;
        Size2D Size;
    }

    [Numerical]
    class Proportion 
    {
        Number Value;
    }

    [Value]
    class Fraction 
    {
        Number Numerator, Denominator;
    }

    [Measure]
    class Angle
    {
        Number Radians;
    }

    [Measure]
    class Length
    {
        Number Meters;
    }

    [Measure]
    class Mass 
    {
        Number Kilograms;
    }

    [Measure]
    class Temperature 
    {
        Number Celsius;
    }

    [Measure]
    class TimeSpan
    {
        Number Seconds;
    }

    [Interval]
    class Arc
    {
        Angle Start, End;
    }

    [Interval]
    class TimeInterval 
    {
        TimeSpan Start, End;
    }

    [Interval]
    class RealInterval
    {
        Number A, B;
    }

    [Interval]
    class Interval2D 
    {
        Vector2D A, B;
    }

    [Interval]
    class Interval3D 
    {
        Vector3D A, B;
    }

    [Value]
    class Capsule 
    {
        Line3D Line;
        Number Radius;
    }

    // https://mindcontrol.org/~hplus/graphics/matrix-layout.html
    // Column major layout in memory 
    // Translation component is in Column4.XYZ 
    [Value]
    class Matrix3D
    {
        Vector4D Column1, Column2, Column3, Column4;
    }

    [Value]
    class Cylinder 
    {
        Line3D Line;
        Number Radius;
    }

    [Value]
    class Cone 
    {
        Line3D Line;
        Number Radius;
    }

    [Value]
    class Tube 
    {
        Line3D Line;
        Number InnerRadius;
        Number OuterRadius;
    }

    [Value]
    class ConeSegment 
    {
        Line3D Line;
        Number Radius1, Radius2;
    }

    [Value]
    class Box3D 
    {
        Point3D Center;
        Rotation3D Rotation;
        Size3D Extent;
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_triangle
    [Value]
    class CubicBezierTriangle3D
    {
        Point3D A, B, C, A2B, AB2, B2C, BC2, AC2, A2C, ABC;
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    [Value]
    class CubicBezier2D
    {
        Point2D A, B, C, D;
    }

    // https://en.wikipedia.org/wiki/UV_mapping
    [Vector]
    class UV
    {
        Unit U, V;
    }

    [Vector]
    class UVW
    {
        Unit U, V, W;
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    [Value]
    class CubicBezier3D
    {
        Point3D A, B, C, D;
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    [Value]
    class QuadraticBezier2D
    {
        Point2D A, B, C;
    }

    // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    [Value]
    class QuadraticBezier3D
    {
        Point3D A, B, C;
    }

    // https://en.wikipedia.org/wiki/Area
    [Measure]
    class Area
    {
        Number MetersSquared;
    }

    // https://en.wikipedia.org/wiki/Volume
    [Measure]
    class Volume
    {
        Number MetersCubed;
    }

    // https://en.wikipedia.org/wiki/Velocity
    [Measure]
    class Velocity
    {
        Number MetersPerSecond;
    }

    // https://en.wikipedia.org/wiki/Acceleration
    [Measure]
    class Acceleration
    {
        Number MetersPerSecondSquared;
    }

    // https://en.wikipedia.org/wiki/Force
    [Measure]
    class Force
    {
        Number Newtons; 
    }

    // https://en.wikipedia.org/wiki/Pressure
    [Measure]
    class Pressure
    {
        Number Pascals;
    }

    // https://en.wikipedia.org/wiki/Energy
    [Measure]
    class Energy
    {
        Number Joules;
    }

    // https://en.wikipedia.org/wiki/Byte
    [Measure]
    class Memory
    {
        Count Bytes;
    }

    // https://en.wikipedia.org/wiki/Frequency
    [Measure]
    class Frequency
    {
        Number Hertz;
    }

    // https://en.wikipedia.org/wiki/Loudness
    [Measure]
    class Loudness
    {
        Number Decibels;
    }

    // https://en.wikipedia.org/wiki/Luminous_intensity
    [Measure]
    class LuminousIntensity
    {
        Number Candelas;
    }

    // https://en.wikipedia.org/wiki/Electric_potential
    [Measure]
    class ElectricPotential
    {
        Number Volts;
    }

    // https://en.wikipedia.org/wiki/Electric_charge
    [Measure]
    class ElectricCharge
    {
        Number Columbs;
    }

    // https://en.wikipedia.org/wiki/Electric_current
    [Measure]
    class ElectricCurrent
    {
        Number Amperes;
    }

    // https://en.wikipedia.org/wiki/Electrical_resistance_and_conductance
    [Measure]
    class ElectricResistance
    {
        Number Ohms;
    }

    // https://en.wikipedia.org/wiki/Power_(physics)
    [Measure]
    class Power
    {
        Number Watts;
    }

    // https://en.wikipedia.org/wiki/Density
    [Measure]
    class Density
    {
        Number KilogramsPerMeterCubed;
    }

    // https://en.wikipedia.org/wiki/Normal_distribution
    [Value]
    class NormalDistribution
    {
        Number Mean;
        Number StandardDeviation;
    }

    // https://en.wikipedia.org/wiki/Poisson_distribution
    [Value]
    class PoissonDistribution
    {
        Number Expected;
        Count Occurrences;
    }

    // https://en.wikipedia.org/wiki/Bernoulli_distribution
    [Value]
    class BernoulliDistribution
    {
        Probability P;
    }

    [Numerical]
    class Probability
    {
        Number Value;
    }

    // https://en.wikipedia.org/wiki/Binomial_distribution
    [Value]
    class BinomialDistribution
    {
        Count Trials;
        Probability P;
    }
}