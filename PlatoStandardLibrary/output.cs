class Interval
{
    var Min
    var Max
}
class Vector
{
}
class Measure
{
    var Value
}
class Numerical
{
    public static null
}
class Magnitude
{
    public static null
}
class Comparable
{
    public static null
}
class Equatable
{
    public static null
}
class Arithmetic
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class ScalarArithmetic
{
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Value
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Array
{
    public static null
    var Count
}
class Integer
{
    Integer Value
}
class Count
{
    Integer Value
}
class Index
{
    Integer Value
}
class Number
{
    var Value
}
class Unit
{
    Number Value
}
class Percent
{
    Number Value
}
class Quaternion
{
    Number X
    Number Y
    Number Z
    Number W
}
class Unit2D
{
    Unit X
    Unit Y
}
class Unit3D
{
    Unit X
    Unit Y
    Unit Z
}
class Direction3D
{
    Unit3D Value
}
class AxisAngle
{
    Unit3D Axis
    var Angle
}
class EulerAngles
{
    unresolved Yaw
    unresolved Pitch
    unresolved Roll
}
class Rotation3D
{
    Quaternion Quaternion
}
class Vector2D
{
    Number X
    Number Y
}
class Vector3D
{
    Number X
    Number Y
    Number Z
}
class Vector4D
{
    Number X
    Number Y
    Number Z
    Number W
}
class Orientation3D
{
    Rotation3D Value
}
class Pose2D
{
    Vector3D Position
    Orientation3D Orientation
}
class Pose3D
{
    Vector3D Position
    Orientation3D Orientation
}
class Transform3D
{
    Vector3D Translation
    Rotation3D Rotation
    Vector3D Scale
}
class Transform2D
{
    Vector2D Translation
    unresolved Rotation
    Vector2D Scale
}
class AlignedBox2D
{
    Vector2D A
    Vector2D B
}
class AlignedBox3D
{
    Vector3D A
    Vector3D B
}
class Complex
{
    Number Real
    Number Imaginary
}
class Ray3D
{
    Vector3D Direction
    var Position
}
class Ray2D
{
    Vector2D Direction
    var Position
}
class Sphere
{
    var Center
    Number Radius
}
class Plane
{
    Unit3D Normal
    Number D
}
class Triangle3D
{
    var A
    var B
    var C
}
class Triangle2D
{
    var A
    var B
    var C
}
class Quad3D
{
    var A
    var B
    var C
    var D
}
class Quad2D
{
    var A
    var B
    var C
    var D
}
class Point3D
{
    Vector3D Value
}
class Point2D
{
    Vector2D Value
}
class Line3D
{
    Point3D A
    Point3D B
}
class Line2D
{
    Point2D A
    Point2D B
}
class Color
{
    Unit R
    Unit G
    Unit B
    Unit A
}
class ColorLUV
{
    Percent Lightness
    Unit U
    Unit V
}
class ColorLAB
{
    Percent Lightness
    Integer A
    Integer B
}
class ColorLCh
{
    Percent Lightness
    var ChromaHue
}
class ColorHSV
{
    unresolved Hue
    Unit S
    Unit V
}
class ColorHSL
{
    unresolved Hue
    Unit Saturation
    Unit Luminance
}
class ColorYCbCr
{
    Unit Y
    Unit Cb
    Unit Cr
}
class SphericalCoordinate
{
    Number Radius
    unresolved Azimuth
    unresolved Polar
}
class PolarCoordinate
{
    Number Radius
    unresolved Angle
}
class LogPolarCoordinate
{
    Number Rho
    unresolved Azimuth
}
class CylindricalCoordinate
{
    Number RadialDistance
    unresolved Azimuth
    Number Height
}
class HorizontalCoordinate
{
    Number Radius
    unresolved Azimuth
    Number Height
}
class GeoCoordinate
{
    unresolved Latitude
    unresolved Longitude
}
class GeoCoordinateWithAltitude
{
    GeoCoordinate Coordinate
    Number Altitude
}
class Circle
{
    Point2D Center
    Number Radius
}
class Chord
{
    Circle Circle
    var Arc
}
class Size2D
{
    Number Width
    Number Height
}
class Size3D
{
    Number Width
    Number Height
    Number Depth
}
class Rectangle2D
{
    Point2D Center
    Size2D Size
}
class Proportion
{
    Number Value
}
class Fraction
{
    Number Numerator
    Number Denominator
}
class Angle
{
    Number Radians
}
class Length
{
    Number Meters
}
class Mass
{
    Number Kilograms
}
class Temperature
{
    Number Celsius
}
class TimeSpan
{
    Number Seconds
}
class TimeRange
{
    var Min
    var Max
}
class DateTime
{
}
class AnglePair
{
    Angle Start
    Angle End
}
class Ring
{
    unresolved Circle
    Number InnerRadius
}
class Arc
{
    AnglePair Angles
    unresolved Cirlce
}
class TimeInterval
{
    TimeSpan Start
    TimeSpan End
}
class RealInterval
{
    Number A
    Number B
}
class Interval2D
{
    Vector2D A
    Vector2D B
}
class Interval3D
{
    Vector3D A
    Vector3D B
}
class Capsule
{
    Line3D Line
    Number Radius
}
class Matrix3D
{
    Vector4D Column1
    Vector4D Column2
    Vector4D Column3
    Vector4D Column4
}
class Cylinder
{
    Line3D Line
    Number Radius
}
class Cone
{
    Line3D Line
    Number Radius
}
class Tube
{
    Line3D Line
    Number InnerRadius
    Number OuterRadius
}
class ConeSegment
{
    Line3D Line
    Number Radius1
    Number Radius2
}
class Box2D
{
    Point2D Center
    Angle Rotation
    Size2D Extent
}
class Box3D
{
    Point3D Center
    Rotation3D Rotation
    Size3D Extent
}
class CubicBezierTriangle3D
{
    Point3D A
    Point3D B
    Point3D C
    Point3D A2B
    Point3D AB2
    Point3D B2C
    Point3D BC2
    Point3D AC2
    Point3D A2C
    Point3D ABC
}
class CubicBezier2D
{
    Point2D A
    Point2D B
    Point2D C
    Point2D D
}
class UV
{
    Unit U
    Unit V
}
class UVW
{
    Unit U
    Unit V
    Unit W
}
class CubicBezier3D
{
    Point3D A
    Point3D B
    Point3D C
    Point3D D
}
class QuadraticBezier2D
{
    Point2D A
    Point2D B
    Point2D C
}
class QuadraticBezier3D
{
    Point3D A
    Point3D B
    Point3D C
}
class Area
{
    Number MetersSquared
}
class Volume
{
    Number MetersCubed
}
class Velocity
{
    Number MetersPerSecond
}
class Acceleration
{
    Number MetersPerSecondSquared
}
class Force
{
    Number Newtons
}
class Pressure
{
    Number Pascals
}
class Energy
{
    Number Joules
}
class Memory
{
    Count Bytes
}
class Frequency
{
    Number Hertz
}
class Loudness
{
    Number Decibels
}
class LuminousIntensity
{
    Number Candelas
}
class ElectricPotential
{
    Number Volts
}
class ElectricCharge
{
    Number Columbs
}
class ElectricCurrent
{
    Number Amperes
}
class ElectricResistance
{
    Number Ohms
}
class Power
{
    Number Watts
}
class Density
{
    Number KilogramsPerMeterCubed
}
class NormalDistribution
{
    Number Mean
    Number StandardDeviation
}
class PoissonDistribution
{
    Number Expected
    Count Occurrences
}
class BernoulliDistribution
{
    var P
}
class Probability
{
    Number Value
}
class BinomialDistribution
{
    Count Trials
    Probability P
}
class Interval
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Vector
{
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Trig
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Numerical
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Comparable
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Equatable
{
    public static null
}
class Array
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
class Easings
{
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
    public static null
}
