class P_Interval
{
    constructor(P_Min, P_Max)
    {
        this._field_Min = P_Min;
        this._field_Max = P_Max;
    }
    // field accessors
    static P_Min = function(self) { return self._field_Min; }
    static P_Max = function(self) { return self._field_Max; }
    // functions 
}
class P_Vector
{
    constructor()
    {
    }
    // field accessors
    // functions 
}
class P_Measure
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Numerical
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_FromNumber = function (P_x)
    // Candidates = Number
    {
        null}
    ;
}
class P_Magnitude
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Magnitude = function (P_x)
    // Candidates = Self
    {
        null}
    ;
}
class P_Comparable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Compare = function (P_a, P_b)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
}
class P_Equatable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Equals = function (P_a, P_b)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
}
class P_Arithmetic
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Add = function (P_self, P_other)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
    static P_Negative = function (P_self)
    // Candidates = Self
    {
        null}
    ;
    static P_Reciprocal = function (P_self)
    // Candidates = Self
    {
        null}
    ;
    static P_Multiply = function (P_self, P_other)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
    static P_Divide = function (P_self, P_other)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
    static P_Modulo = function (P_self, P_other)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
}
class P_ScalarArithmetic
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Add = function (P_self, P_scalar)
    // Candidates = Self
    // Candidates = T
    {
        null}
    ;
    static P_Subtract = function (P_self, P_scalar)
    // Candidates = Self
    // Candidates = T
    {
        null}
    ;
    static P_Multiply = function (P_self, P_scalar)
    // Candidates = Self
    // Candidates = T
    {
        null}
    ;
    static P_Divide = function (P_self, P_scalar)
    // Candidates = Self
    // Candidates = T
    {
        null}
    ;
    static P_Modulo = function (P_self, P_scalar)
    // Candidates = Self
    // Candidates = T
    {
        null}
    ;
}
class P_Boolean
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_And = function (P_a, P_b)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
    static P_Or = function (P_a, P_b)
    // Candidates = Self
    // Candidates = Self
    {
        null}
    ;
    static P_Not = function (P_a)
    // Candidates = Self
    {
        null}
    ;
}
class P_Value
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Type = function ()
    {
        null}
    ;
    static P_FieldTypes = function ()
    {
        null}
    ;
    static P_FieldNames = function ()
    {
        null}
    ;
    static P_FieldValues = function (P_self)
    // Candidates = Self
    {
        null}
    ;
    static P_Zero = function ()
    {
        null}
    ;
    static P_One = function ()
    {
        null}
    ;
    static P_Default = function ()
    {
        null}
    ;
    static P_MinValue = function ()
    {
        null}
    ;
    static P_MaxValue = function ()
    {
        null}
    ;
    static P_ToString = function (P_x)
    // Candidates = Self
    {
        null}
    ;
}
class P_Array
{
    constructor(P_Count)
    {
        this._field_Count = P_Count;
    }
    // field accessors
    static P_Count = function(self) { return self._field_Count; }
    // functions 
    static P_At = function (P_n)
    // Candidates = Index
    null;
}
class P_Integer
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Count
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Index
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Number
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Unit
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Percent
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Quaternion
{
    constructor(P_X, P_Y, P_Z, P_W)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
        this._field_W = P_W;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    static P_Z = function(self) { return self._field_Z; }
    static P_W = function(self) { return self._field_W; }
    // functions 
}
class P_Unit2D
{
    constructor(P_X, P_Y)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    // functions 
}
class P_Unit3D
{
    constructor(P_X, P_Y, P_Z)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    static P_Z = function(self) { return self._field_Z; }
    // functions 
}
class P_Direction3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_AxisAngle
{
    constructor(P_Axis, P_Angle)
    {
        this._field_Axis = P_Axis;
        this._field_Angle = P_Angle;
    }
    // field accessors
    static P_Axis = function(self) { return self._field_Axis; }
    static P_Angle = function(self) { return self._field_Angle; }
    // functions 
}
class P_EulerAngles
{
    constructor(P_Yaw, P_Pitch, P_Roll)
    {
        this._field_Yaw = P_Yaw;
        this._field_Pitch = P_Pitch;
        this._field_Roll = P_Roll;
    }
    // field accessors
    static P_Yaw = function(self) { return self._field_Yaw; }
    static P_Pitch = function(self) { return self._field_Pitch; }
    static P_Roll = function(self) { return self._field_Roll; }
    // functions 
}
class P_Rotation3D
{
    constructor(P_Quaternion)
    {
        this._field_Quaternion = P_Quaternion;
    }
    // field accessors
    static P_Quaternion = function(self) { return self._field_Quaternion; }
    // functions 
}
class P_Vector2D
{
    constructor(P_X, P_Y)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    // functions 
}
class P_Vector3D
{
    constructor(P_X, P_Y, P_Z)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    static P_Z = function(self) { return self._field_Z; }
    // functions 
}
class P_Vector4D
{
    constructor(P_X, P_Y, P_Z, P_W)
    {
        this._field_X = P_X;
        this._field_Y = P_Y;
        this._field_Z = P_Z;
        this._field_W = P_W;
    }
    // field accessors
    static P_X = function(self) { return self._field_X; }
    static P_Y = function(self) { return self._field_Y; }
    static P_Z = function(self) { return self._field_Z; }
    static P_W = function(self) { return self._field_W; }
    // functions 
}
class P_Orientation3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Pose2D
{
    constructor(P_Position, P_Orientation)
    {
        this._field_Position = P_Position;
        this._field_Orientation = P_Orientation;
    }
    // field accessors
    static P_Position = function(self) { return self._field_Position; }
    static P_Orientation = function(self) { return self._field_Orientation; }
    // functions 
}
class P_Pose3D
{
    constructor(P_Position, P_Orientation)
    {
        this._field_Position = P_Position;
        this._field_Orientation = P_Orientation;
    }
    // field accessors
    static P_Position = function(self) { return self._field_Position; }
    static P_Orientation = function(self) { return self._field_Orientation; }
    // functions 
}
class P_Transform3D
{
    constructor(P_Translation, P_Rotation, P_Scale)
    {
        this._field_Translation = P_Translation;
        this._field_Rotation = P_Rotation;
        this._field_Scale = P_Scale;
    }
    // field accessors
    static P_Translation = function(self) { return self._field_Translation; }
    static P_Rotation = function(self) { return self._field_Rotation; }
    static P_Scale = function(self) { return self._field_Scale; }
    // functions 
}
class P_Transform2D
{
    constructor(P_Translation, P_Rotation, P_Scale)
    {
        this._field_Translation = P_Translation;
        this._field_Rotation = P_Rotation;
        this._field_Scale = P_Scale;
    }
    // field accessors
    static P_Translation = function(self) { return self._field_Translation; }
    static P_Rotation = function(self) { return self._field_Rotation; }
    static P_Scale = function(self) { return self._field_Scale; }
    // functions 
}
class P_AlignedBox2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_AlignedBox3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Complex
{
    constructor(P_Real, P_Imaginary)
    {
        this._field_Real = P_Real;
        this._field_Imaginary = P_Imaginary;
    }
    // field accessors
    static P_Real = function(self) { return self._field_Real; }
    static P_Imaginary = function(self) { return self._field_Imaginary; }
    // functions 
}
class P_Ray3D
{
    constructor(P_Direction, P_Position)
    {
        this._field_Direction = P_Direction;
        this._field_Position = P_Position;
    }
    // field accessors
    static P_Direction = function(self) { return self._field_Direction; }
    static P_Position = function(self) { return self._field_Position; }
    // functions 
}
class P_Ray2D
{
    constructor(P_Direction, P_Position)
    {
        this._field_Direction = P_Direction;
        this._field_Position = P_Position;
    }
    // field accessors
    static P_Direction = function(self) { return self._field_Direction; }
    static P_Position = function(self) { return self._field_Position; }
    // functions 
}
class P_Sphere
{
    constructor(P_Center, P_Radius)
    {
        this._field_Center = P_Center;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Center = function(self) { return self._field_Center; }
    static P_Radius = function(self) { return self._field_Radius; }
    // functions 
}
class P_Plane
{
    constructor(P_Normal, P_D)
    {
        this._field_Normal = P_Normal;
        this._field_D = P_D;
    }
    // field accessors
    static P_Normal = function(self) { return self._field_Normal; }
    static P_D = function(self) { return self._field_D; }
    // functions 
}
class P_Triangle3D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    // functions 
}
class P_Triangle2D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    // functions 
}
class P_Quad3D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    static P_D = function(self) { return self._field_D; }
    // functions 
}
class P_Quad2D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    static P_D = function(self) { return self._field_D; }
    // functions 
}
class P_Point3D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Point2D
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Line3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Line2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Color
{
    constructor(P_R, P_G, P_B, P_A)
    {
        this._field_R = P_R;
        this._field_G = P_G;
        this._field_B = P_B;
        this._field_A = P_A;
    }
    // field accessors
    static P_R = function(self) { return self._field_R; }
    static P_G = function(self) { return self._field_G; }
    static P_B = function(self) { return self._field_B; }
    static P_A = function(self) { return self._field_A; }
    // functions 
}
class P_ColorLUV
{
    constructor(P_Lightness, P_U, P_V)
    {
        this._field_Lightness = P_Lightness;
        this._field_U = P_U;
        this._field_V = P_V;
    }
    // field accessors
    static P_Lightness = function(self) { return self._field_Lightness; }
    static P_U = function(self) { return self._field_U; }
    static P_V = function(self) { return self._field_V; }
    // functions 
}
class P_ColorLAB
{
    constructor(P_Lightness, P_A, P_B)
    {
        this._field_Lightness = P_Lightness;
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_Lightness = function(self) { return self._field_Lightness; }
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_ColorLCh
{
    constructor(P_Lightness, P_ChromaHue)
    {
        this._field_Lightness = P_Lightness;
        this._field_ChromaHue = P_ChromaHue;
    }
    // field accessors
    static P_Lightness = function(self) { return self._field_Lightness; }
    static P_ChromaHue = function(self) { return self._field_ChromaHue; }
    // functions 
}
class P_ColorHSV
{
    constructor(P_Hue, P_S, P_V)
    {
        this._field_Hue = P_Hue;
        this._field_S = P_S;
        this._field_V = P_V;
    }
    // field accessors
    static P_Hue = function(self) { return self._field_Hue; }
    static P_S = function(self) { return self._field_S; }
    static P_V = function(self) { return self._field_V; }
    // functions 
}
class P_ColorHSL
{
    constructor(P_Hue, P_Saturation, P_Luminance)
    {
        this._field_Hue = P_Hue;
        this._field_Saturation = P_Saturation;
        this._field_Luminance = P_Luminance;
    }
    // field accessors
    static P_Hue = function(self) { return self._field_Hue; }
    static P_Saturation = function(self) { return self._field_Saturation; }
    static P_Luminance = function(self) { return self._field_Luminance; }
    // functions 
}
class P_ColorYCbCr
{
    constructor(P_Y, P_Cb, P_Cr)
    {
        this._field_Y = P_Y;
        this._field_Cb = P_Cb;
        this._field_Cr = P_Cr;
    }
    // field accessors
    static P_Y = function(self) { return self._field_Y; }
    static P_Cb = function(self) { return self._field_Cb; }
    static P_Cr = function(self) { return self._field_Cr; }
    // functions 
}
class P_SphericalCoordinate
{
    constructor(P_Radius, P_Azimuth, P_Polar)
    {
        this._field_Radius = P_Radius;
        this._field_Azimuth = P_Azimuth;
        this._field_Polar = P_Polar;
    }
    // field accessors
    static P_Radius = function(self) { return self._field_Radius; }
    static P_Azimuth = function(self) { return self._field_Azimuth; }
    static P_Polar = function(self) { return self._field_Polar; }
    // functions 
}
class P_PolarCoordinate
{
    constructor(P_Radius, P_Angle)
    {
        this._field_Radius = P_Radius;
        this._field_Angle = P_Angle;
    }
    // field accessors
    static P_Radius = function(self) { return self._field_Radius; }
    static P_Angle = function(self) { return self._field_Angle; }
    // functions 
}
class P_LogPolarCoordinate
{
    constructor(P_Rho, P_Azimuth)
    {
        this._field_Rho = P_Rho;
        this._field_Azimuth = P_Azimuth;
    }
    // field accessors
    static P_Rho = function(self) { return self._field_Rho; }
    static P_Azimuth = function(self) { return self._field_Azimuth; }
    // functions 
}
class P_CylindricalCoordinate
{
    constructor(P_RadialDistance, P_Azimuth, P_Height)
    {
        this._field_RadialDistance = P_RadialDistance;
        this._field_Azimuth = P_Azimuth;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_RadialDistance = function(self) { return self._field_RadialDistance; }
    static P_Azimuth = function(self) { return self._field_Azimuth; }
    static P_Height = function(self) { return self._field_Height; }
    // functions 
}
class P_HorizontalCoordinate
{
    constructor(P_Radius, P_Azimuth, P_Height)
    {
        this._field_Radius = P_Radius;
        this._field_Azimuth = P_Azimuth;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_Radius = function(self) { return self._field_Radius; }
    static P_Azimuth = function(self) { return self._field_Azimuth; }
    static P_Height = function(self) { return self._field_Height; }
    // functions 
}
class P_GeoCoordinate
{
    constructor(P_Latitude, P_Longitude)
    {
        this._field_Latitude = P_Latitude;
        this._field_Longitude = P_Longitude;
    }
    // field accessors
    static P_Latitude = function(self) { return self._field_Latitude; }
    static P_Longitude = function(self) { return self._field_Longitude; }
    // functions 
}
class P_GeoCoordinateWithAltitude
{
    constructor(P_Coordinate, P_Altitude)
    {
        this._field_Coordinate = P_Coordinate;
        this._field_Altitude = P_Altitude;
    }
    // field accessors
    static P_Coordinate = function(self) { return self._field_Coordinate; }
    static P_Altitude = function(self) { return self._field_Altitude; }
    // functions 
}
class P_Circle
{
    constructor(P_Center, P_Radius)
    {
        this._field_Center = P_Center;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Center = function(self) { return self._field_Center; }
    static P_Radius = function(self) { return self._field_Radius; }
    // functions 
}
class P_Chord
{
    constructor(P_Circle, P_Arc)
    {
        this._field_Circle = P_Circle;
        this._field_Arc = P_Arc;
    }
    // field accessors
    static P_Circle = function(self) { return self._field_Circle; }
    static P_Arc = function(self) { return self._field_Arc; }
    // functions 
}
class P_Size2D
{
    constructor(P_Width, P_Height)
    {
        this._field_Width = P_Width;
        this._field_Height = P_Height;
    }
    // field accessors
    static P_Width = function(self) { return self._field_Width; }
    static P_Height = function(self) { return self._field_Height; }
    // functions 
}
class P_Size3D
{
    constructor(P_Width, P_Height, P_Depth)
    {
        this._field_Width = P_Width;
        this._field_Height = P_Height;
        this._field_Depth = P_Depth;
    }
    // field accessors
    static P_Width = function(self) { return self._field_Width; }
    static P_Height = function(self) { return self._field_Height; }
    static P_Depth = function(self) { return self._field_Depth; }
    // functions 
}
class P_Rectangle2D
{
    constructor(P_Center, P_Size)
    {
        this._field_Center = P_Center;
        this._field_Size = P_Size;
    }
    // field accessors
    static P_Center = function(self) { return self._field_Center; }
    static P_Size = function(self) { return self._field_Size; }
    // functions 
}
class P_Proportion
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_Fraction
{
    constructor(P_Numerator, P_Denominator)
    {
        this._field_Numerator = P_Numerator;
        this._field_Denominator = P_Denominator;
    }
    // field accessors
    static P_Numerator = function(self) { return self._field_Numerator; }
    static P_Denominator = function(self) { return self._field_Denominator; }
    // functions 
}
class P_Angle
{
    constructor(P_Radians)
    {
        this._field_Radians = P_Radians;
    }
    // field accessors
    static P_Radians = function(self) { return self._field_Radians; }
    // functions 
}
class P_Length
{
    constructor(P_Meters)
    {
        this._field_Meters = P_Meters;
    }
    // field accessors
    static P_Meters = function(self) { return self._field_Meters; }
    // functions 
}
class P_Mass
{
    constructor(P_Kilograms)
    {
        this._field_Kilograms = P_Kilograms;
    }
    // field accessors
    static P_Kilograms = function(self) { return self._field_Kilograms; }
    // functions 
}
class P_Temperature
{
    constructor(P_Celsius)
    {
        this._field_Celsius = P_Celsius;
    }
    // field accessors
    static P_Celsius = function(self) { return self._field_Celsius; }
    // functions 
}
class P_TimeSpan
{
    constructor(P_Seconds)
    {
        this._field_Seconds = P_Seconds;
    }
    // field accessors
    static P_Seconds = function(self) { return self._field_Seconds; }
    // functions 
}
class P_TimeRange
{
    constructor(P_Min, P_Max)
    {
        this._field_Min = P_Min;
        this._field_Max = P_Max;
    }
    // field accessors
    static P_Min = function(self) { return self._field_Min; }
    static P_Max = function(self) { return self._field_Max; }
    // functions 
}
class P_DateTime
{
    constructor()
    {
    }
    // field accessors
    // functions 
}
class P_AnglePair
{
    constructor(P_Start, P_End)
    {
        this._field_Start = P_Start;
        this._field_End = P_End;
    }
    // field accessors
    static P_Start = function(self) { return self._field_Start; }
    static P_End = function(self) { return self._field_End; }
    // functions 
}
class P_Ring
{
    constructor(P_Circle, P_InnerRadius)
    {
        this._field_Circle = P_Circle;
        this._field_InnerRadius = P_InnerRadius;
    }
    // field accessors
    static P_Circle = function(self) { return self._field_Circle; }
    static P_InnerRadius = function(self) { return self._field_InnerRadius; }
    // functions 
}
class P_Arc
{
    constructor(P_Angles, P_Cirlce)
    {
        this._field_Angles = P_Angles;
        this._field_Cirlce = P_Cirlce;
    }
    // field accessors
    static P_Angles = function(self) { return self._field_Angles; }
    static P_Cirlce = function(self) { return self._field_Cirlce; }
    // functions 
}
class P_TimeInterval
{
    constructor(P_Start, P_End)
    {
        this._field_Start = P_Start;
        this._field_End = P_End;
    }
    // field accessors
    static P_Start = function(self) { return self._field_Start; }
    static P_End = function(self) { return self._field_End; }
    // functions 
}
class P_RealInterval
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Interval2D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Interval3D
{
    constructor(P_A, P_B)
    {
        this._field_A = P_A;
        this._field_B = P_B;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    // functions 
}
class P_Capsule
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line = function(self) { return self._field_Line; }
    static P_Radius = function(self) { return self._field_Radius; }
    // functions 
}
class P_Matrix3D
{
    constructor(P_Column1, P_Column2, P_Column3, P_Column4)
    {
        this._field_Column1 = P_Column1;
        this._field_Column2 = P_Column2;
        this._field_Column3 = P_Column3;
        this._field_Column4 = P_Column4;
    }
    // field accessors
    static P_Column1 = function(self) { return self._field_Column1; }
    static P_Column2 = function(self) { return self._field_Column2; }
    static P_Column3 = function(self) { return self._field_Column3; }
    static P_Column4 = function(self) { return self._field_Column4; }
    // functions 
}
class P_Cylinder
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line = function(self) { return self._field_Line; }
    static P_Radius = function(self) { return self._field_Radius; }
    // functions 
}
class P_Cone
{
    constructor(P_Line, P_Radius)
    {
        this._field_Line = P_Line;
        this._field_Radius = P_Radius;
    }
    // field accessors
    static P_Line = function(self) { return self._field_Line; }
    static P_Radius = function(self) { return self._field_Radius; }
    // functions 
}
class P_Tube
{
    constructor(P_Line, P_InnerRadius, P_OuterRadius)
    {
        this._field_Line = P_Line;
        this._field_InnerRadius = P_InnerRadius;
        this._field_OuterRadius = P_OuterRadius;
    }
    // field accessors
    static P_Line = function(self) { return self._field_Line; }
    static P_InnerRadius = function(self) { return self._field_InnerRadius; }
    static P_OuterRadius = function(self) { return self._field_OuterRadius; }
    // functions 
}
class P_ConeSegment
{
    constructor(P_Line, P_Radius1, P_Radius2)
    {
        this._field_Line = P_Line;
        this._field_Radius1 = P_Radius1;
        this._field_Radius2 = P_Radius2;
    }
    // field accessors
    static P_Line = function(self) { return self._field_Line; }
    static P_Radius1 = function(self) { return self._field_Radius1; }
    static P_Radius2 = function(self) { return self._field_Radius2; }
    // functions 
}
class P_Box2D
{
    constructor(P_Center, P_Rotation, P_Extent)
    {
        this._field_Center = P_Center;
        this._field_Rotation = P_Rotation;
        this._field_Extent = P_Extent;
    }
    // field accessors
    static P_Center = function(self) { return self._field_Center; }
    static P_Rotation = function(self) { return self._field_Rotation; }
    static P_Extent = function(self) { return self._field_Extent; }
    // functions 
}
class P_Box3D
{
    constructor(P_Center, P_Rotation, P_Extent)
    {
        this._field_Center = P_Center;
        this._field_Rotation = P_Rotation;
        this._field_Extent = P_Extent;
    }
    // field accessors
    static P_Center = function(self) { return self._field_Center; }
    static P_Rotation = function(self) { return self._field_Rotation; }
    static P_Extent = function(self) { return self._field_Extent; }
    // functions 
}
class P_CubicBezierTriangle3D
{
    constructor(P_A, P_B, P_C, P_A2B, P_AB2, P_B2C, P_BC2, P_AC2, P_A2C, P_ABC)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_A2B = P_A2B;
        this._field_AB2 = P_AB2;
        this._field_B2C = P_B2C;
        this._field_BC2 = P_BC2;
        this._field_AC2 = P_AC2;
        this._field_A2C = P_A2C;
        this._field_ABC = P_ABC;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    static P_A2B = function(self) { return self._field_A2B; }
    static P_AB2 = function(self) { return self._field_AB2; }
    static P_B2C = function(self) { return self._field_B2C; }
    static P_BC2 = function(self) { return self._field_BC2; }
    static P_AC2 = function(self) { return self._field_AC2; }
    static P_A2C = function(self) { return self._field_A2C; }
    static P_ABC = function(self) { return self._field_ABC; }
    // functions 
}
class P_CubicBezier2D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    static P_D = function(self) { return self._field_D; }
    // functions 
}
class P_UV
{
    constructor(P_U, P_V)
    {
        this._field_U = P_U;
        this._field_V = P_V;
    }
    // field accessors
    static P_U = function(self) { return self._field_U; }
    static P_V = function(self) { return self._field_V; }
    // functions 
}
class P_UVW
{
    constructor(P_U, P_V, P_W)
    {
        this._field_U = P_U;
        this._field_V = P_V;
        this._field_W = P_W;
    }
    // field accessors
    static P_U = function(self) { return self._field_U; }
    static P_V = function(self) { return self._field_V; }
    static P_W = function(self) { return self._field_W; }
    // functions 
}
class P_CubicBezier3D
{
    constructor(P_A, P_B, P_C, P_D)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
        this._field_D = P_D;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    static P_D = function(self) { return self._field_D; }
    // functions 
}
class P_QuadraticBezier2D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    // functions 
}
class P_QuadraticBezier3D
{
    constructor(P_A, P_B, P_C)
    {
        this._field_A = P_A;
        this._field_B = P_B;
        this._field_C = P_C;
    }
    // field accessors
    static P_A = function(self) { return self._field_A; }
    static P_B = function(self) { return self._field_B; }
    static P_C = function(self) { return self._field_C; }
    // functions 
}
class P_Area
{
    constructor(P_MetersSquared)
    {
        this._field_MetersSquared = P_MetersSquared;
    }
    // field accessors
    static P_MetersSquared = function(self) { return self._field_MetersSquared; }
    // functions 
}
class P_Volume
{
    constructor(P_MetersCubed)
    {
        this._field_MetersCubed = P_MetersCubed;
    }
    // field accessors
    static P_MetersCubed = function(self) { return self._field_MetersCubed; }
    // functions 
}
class P_Velocity
{
    constructor(P_MetersPerSecond)
    {
        this._field_MetersPerSecond = P_MetersPerSecond;
    }
    // field accessors
    static P_MetersPerSecond = function(self) { return self._field_MetersPerSecond; }
    // functions 
}
class P_Acceleration
{
    constructor(P_MetersPerSecondSquared)
    {
        this._field_MetersPerSecondSquared = P_MetersPerSecondSquared;
    }
    // field accessors
    static P_MetersPerSecondSquared = function(self) { return self._field_MetersPerSecondSquared; }
    // functions 
}
class P_Force
{
    constructor(P_Newtons)
    {
        this._field_Newtons = P_Newtons;
    }
    // field accessors
    static P_Newtons = function(self) { return self._field_Newtons; }
    // functions 
}
class P_Pressure
{
    constructor(P_Pascals)
    {
        this._field_Pascals = P_Pascals;
    }
    // field accessors
    static P_Pascals = function(self) { return self._field_Pascals; }
    // functions 
}
class P_Energy
{
    constructor(P_Joules)
    {
        this._field_Joules = P_Joules;
    }
    // field accessors
    static P_Joules = function(self) { return self._field_Joules; }
    // functions 
}
class P_Memory
{
    constructor(P_Bytes)
    {
        this._field_Bytes = P_Bytes;
    }
    // field accessors
    static P_Bytes = function(self) { return self._field_Bytes; }
    // functions 
}
class P_Frequency
{
    constructor(P_Hertz)
    {
        this._field_Hertz = P_Hertz;
    }
    // field accessors
    static P_Hertz = function(self) { return self._field_Hertz; }
    // functions 
}
class P_Loudness
{
    constructor(P_Decibels)
    {
        this._field_Decibels = P_Decibels;
    }
    // field accessors
    static P_Decibels = function(self) { return self._field_Decibels; }
    // functions 
}
class P_LuminousIntensity
{
    constructor(P_Candelas)
    {
        this._field_Candelas = P_Candelas;
    }
    // field accessors
    static P_Candelas = function(self) { return self._field_Candelas; }
    // functions 
}
class P_ElectricPotential
{
    constructor(P_Volts)
    {
        this._field_Volts = P_Volts;
    }
    // field accessors
    static P_Volts = function(self) { return self._field_Volts; }
    // functions 
}
class P_ElectricCharge
{
    constructor(P_Columbs)
    {
        this._field_Columbs = P_Columbs;
    }
    // field accessors
    static P_Columbs = function(self) { return self._field_Columbs; }
    // functions 
}
class P_ElectricCurrent
{
    constructor(P_Amperes)
    {
        this._field_Amperes = P_Amperes;
    }
    // field accessors
    static P_Amperes = function(self) { return self._field_Amperes; }
    // functions 
}
class P_ElectricResistance
{
    constructor(P_Ohms)
    {
        this._field_Ohms = P_Ohms;
    }
    // field accessors
    static P_Ohms = function(self) { return self._field_Ohms; }
    // functions 
}
class P_Power
{
    constructor(P_Watts)
    {
        this._field_Watts = P_Watts;
    }
    // field accessors
    static P_Watts = function(self) { return self._field_Watts; }
    // functions 
}
class P_Density
{
    constructor(P_KilogramsPerMeterCubed)
    {
        this._field_KilogramsPerMeterCubed = P_KilogramsPerMeterCubed;
    }
    // field accessors
    static P_KilogramsPerMeterCubed = function(self) { return self._field_KilogramsPerMeterCubed; }
    // functions 
}
class P_NormalDistribution
{
    constructor(P_Mean, P_StandardDeviation)
    {
        this._field_Mean = P_Mean;
        this._field_StandardDeviation = P_StandardDeviation;
    }
    // field accessors
    static P_Mean = function(self) { return self._field_Mean; }
    static P_StandardDeviation = function(self) { return self._field_StandardDeviation; }
    // functions 
}
class P_PoissonDistribution
{
    constructor(P_Expected, P_Occurrences)
    {
        this._field_Expected = P_Expected;
        this._field_Occurrences = P_Occurrences;
    }
    // field accessors
    static P_Expected = function(self) { return self._field_Expected; }
    static P_Occurrences = function(self) { return self._field_Occurrences; }
    // functions 
}
class P_BernoulliDistribution
{
    constructor(P_P)
    {
        this._field_P = P_P;
    }
    // field accessors
    static P_P = function(self) { return self._field_P; }
    // functions 
}
class P_Probability
{
    constructor(P_Value)
    {
        this._field_Value = P_Value;
    }
    // field accessors
    static P_Value = function(self) { return self._field_Value; }
    // functions 
}
class P_BinomialDistribution
{
    constructor(P_Trials, P_P)
    {
        this._field_Trials = P_Trials;
        this._field_P = P_P;
    }
    // field accessors
    static P_Trials = function(self) { return self._field_Trials; }
    static P_P = function(self) { return self._field_P; }
    // functions 
}
class P_Interval
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Size = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_IsEmpty = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Lerp = function (P_x, P_amount)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_InverseLerp = function (P_x, P_value)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Negate = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Reverse = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Resize = function (P_x, P_size)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Center = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Contains = function (P_x, P_value)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Contains = function (P_x, P_other)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Overlaps = function (P_x, P_y)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Split = function (P_x, P_t)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Split = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Left = function (P_x, P_t)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Right = function (P_x, P_t)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_MoveTo = function (P_x, P_t)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_LeftHalf = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_RightHalf = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_HalfSize = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Recenter = function (P_x, P_c)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Clamp = function (P_x, P_y)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Clamp = function (P_x, P_value)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Between = function (P_x, P_value)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Unit = function ()
    {
        null}
    ;
}
class P_Vector
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Sum = function (P_v)
    // Candidates = Any
    {
        null}
    ;
    static P_SumSquares = function (P_v)
    // Candidates = Any
    {
        null}
    ;
    static P_LengthSquared = function (P_v)
    // Candidates = Any
    {
        null}
    ;
    static P_Length = function (P_v)
    // Candidates = Any
    {
        null}
    ;
    static P_Dot = function (P_v1, P_v2)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
}
class P_Numerical
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Cos = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Sin = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Tan = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Acos = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Asin = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Atan = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Cosh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Sinh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Tanh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Acosh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Asinh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Atanh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Pow = function (P_x, P_y)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Log = function (P_x, P_y)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_NaturalLog = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_NaturalPower = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_SquareRoot = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_CubeRoot = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Square = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Clamp = function (P_x, P_min, P_max)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Clamp = function (P_x, P_i)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Clamp = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_PlusOne = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_MinusOne = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_FromOne = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Sign = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Abs = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Half = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Third = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Quarter = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Fifth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Sixth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Seventh = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Eighth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Ninth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Tenth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Sixteenth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Hundredth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Thousandth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Millionth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Billionth = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Hundred = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Thousand = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Million = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Billion = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Twice = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Thrice = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_SmoothStep = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Pow2 = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Pow3 = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Pow4 = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Pow5 = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_Turns = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_AlmostZero = function (P_x)
    // Candidates = Any
    {
        null}
    ;
}
class P_Comparable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Equals = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_LessThan = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Lesser = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Greater = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_LessThanOrEquals = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_GreaterThan = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_GreaterThanOrEquals = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Min = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Max = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Between = function (P_v, P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Between = function (P_v, P_i)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
}
class P_Boolean
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_XOr = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_NAnd = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_NOr = function (P_a, P_b)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
}
class P_Equatable
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_NotEquals = function (P_x)
    // Candidates = Any
    {
        null}
    ;
}
class P_Array
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Map = function (P_xs, P_f)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Zip = function (P_xs, P_ys, P_f)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Skip = function (P_xs, P_n)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Take = function (P_xs, P_n)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Aggregate = function (P_xs, P_init, P_f)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Rest = function (P_xs)
    // Candidates = Any
    {
        null}
    ;
    static P_IsEmpty = function (P_xs)
    // Candidates = Any
    {
        null}
    ;
    static P_First = function (P_xs)
    // Candidates = Any
    {
        null}
    ;
    static P_Last = function (P_xs)
    // Candidates = Any
    {
        null}
    ;
    static P_Slice = function (P_xs, P_from, P_count)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Join = function (P_xs, P_sep)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_All = function (P_xs, P_f)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_JoinStrings = function (P_xs, P_sep)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
}
class P_Easings
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_BlendEaseFunc = function (P_p, P_easeIn, P_easeOut)
    // Candidates = Any
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_InvertEaseFunc = function (P_p, P_easeIn)
    // Candidates = Any
    // Candidates = Any
    {
        null}
    ;
    static P_Linear = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuadraticEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuadraticEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuadraticEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CubicEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CubicEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CubicEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuarticEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuarticEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuarticEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuinticEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuinticEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_QuinticEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_SineEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_SineEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_SineEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CircularEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CircularEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_CircularEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ExponentialEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ExponentialEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ExponentialEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ElasticEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ElasticEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_ElasticEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BackEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BackEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BackEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BounceEaseIn = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BounceEaseOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
    static P_BounceEaseInOut = function (P_p)
    // Candidates = Any
    {
        null}
    ;
}
class P_Intrinsics
{
    constructor()
    {
    }
    // field accessors
    // functions 
    static P_Interpolate = function (P_xs)
    // Candidates = Any
    {
        null}
    ;
    static P_Throw = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_TypeOf = function (P_x)
    // Candidates = Any
    {
        null}
    ;
    static P_New = function (P_x)
    // Candidates = Any
    {
        null}
    ;
}
