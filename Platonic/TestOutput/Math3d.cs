using System;

using System.Collections.Generic;

using System.Runtime.InteropServices;

using System.Runtime.Serialization;

using System.Text;

namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Vector2
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (float X, float Y)
        // member references = X, Y
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Vector2(float x, float y) 
            => (X, Y) = (x, y);

        // A public instance field named X with a type float
        // No associated operation
        // No data-flow analysis could be created
                public float X; 
        // A public instance field named Y with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float Y;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Vector3
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (float X, float Y, float Z)
        // member references = X, Y, Z
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Vector3(float x, float y, float z) 
            => (X, Y, Z) = (x, y, z);

        // A public instance field named X with a type float
        // No associated operation
        // No data-flow analysis could be created
                public float X; 
        // A public instance field named Y with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float Y; 
        // A public instance field named Z with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float Z; 

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Vector4
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (float X, float Y, float Z, float W)
        // member references = X, Y, Z, W
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter), (Name=w Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Vector4(float x, float y, float z, float w) 
            => (X, Y, Z, W) = (x, y, z, w);

        // A public instance field named X with a type float
        // No associated operation
        // No data-flow analysis could be created
                public float X; 
        // A public instance field named Y with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float Y; 
        // A public instance field named Z with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float Z; 
        // A public instance field named W with a type float
        // No associated operation
        // No data-flow analysis could be created
        public float W;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct DVector2
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (double X, double Y)
        // member references = X, Y
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public DVector2(double x, double y)
            => (X, Y) = (x, y);

        // A public instance field named X with a type double
        // No associated operation
        // No data-flow analysis could be created
                public double X; 
        // A public instance field named Y with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double Y;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct DVector3
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (double X, double Y, double Z)
        // member references = X, Y, Z
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public DVector3(double x, double y, double z)
            => (X, Y, Z) = (x, y, z);

        // A public instance field named X with a type double
        // No associated operation
        // No data-flow analysis could be created
                public double X; 
        // A public instance field named Y with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double Y; 
        // A public instance field named Z with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double Z;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct DVector4
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (double X, double Y, double Z, double W)
        // member references = X, Y, Z, W
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter), (Name=w Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public DVector4(double x, double y, double z, double w)
            => (X, Y, Z, W) = (x, y, z, w);

        // A public instance field named X with a type double
        // No associated operation
        // No data-flow analysis could be created
                public double X; 
        // A public instance field named Y with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double Y; 
        // A public instance field named Z with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double Z; 
        // A public instance field named W with a type double
        // No associated operation
        // No data-flow analysis could be created
        public double W;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Matrix4x4
    {
        // A public instance field named M11 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M11;

        // A public instance field named M12 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M12;

        // A public instance field named M13 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M13;

        // A public instance field named M14 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M14;

        // A public instance field named M21 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M21;

        // A public instance field named M22 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M22;

        // A public instance field named M23 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M23;

        // A public instance field named M24 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M24;

        // A public instance field named M31 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M31;

        // A public instance field named M32 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M32;

        // A public instance field named M33 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M33;

        // A public instance field named M34 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M34;

        // A public instance field named M41 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M41;

        // A public instance field named M42 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M42;

        // A public instance field named M43 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M43;

        // A public instance field named M44 with a type float
        // No associated operation
        // No data-flow analysis could be created
        
        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M44;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct AABox
    {
        // A public instance field named Min with a type Vim.Math3d.Vector3
        // No associated operation
        // No data-flow analysis could be created
                public Vector3 Min; 
        // A public instance field named Max with a type Vim.Math3d.Vector3
        // No associated operation
        // No data-flow analysis could be created
        public Vector3 Max;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Byte2
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (byte X, byte Y)
        // member references = X, Y
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Byte2(byte x, byte y) 
            => (X, Y) = (x, y);

        // A public instance field named X with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte X; 
        // A public instance field named Y with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte Y; 

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Byte3
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (byte X, byte Y, byte Z)
        // member references = X, Y, Z
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Byte3(byte x, byte y, byte z)
            => (X, Y, Z) = (x, y, z);

        // A public instance field named X with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte X; 
        // A public instance field named Y with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte Y; 
        // A public instance field named Z with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte Z; 

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Byte4
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (byte X, byte Y, byte Z, byte W)
        // member references = X, Y, Z, W
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter), (Name=w Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Byte4(byte x, byte y, byte z, byte w)
            => (X, Y, Z, W) = (x, y, z, w);

        // A public instance field named X with a type byte
        // No associated operation
        // No data-flow analysis could be created
                public byte X; 
        // A public instance field named Y with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte Y; 
        // A public instance field named Z with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte Z; 
        // A public instance field named W with a type byte
        // No associated operation
        // No data-flow analysis could be created
        public byte W;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Int2
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (int X, int Y)
        // member references = X, Y
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Int2(int x, int y)
            => (X, Y) = (x, y);

        // A public instance field named X with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int X; 
        // A public instance field named Y with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int Y;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Int3
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (int X, int Y, int Z)
        // member references = X, Y, Z
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Int3(int x, int y, int z)
            => (X, Y, Z) = (x, y, z);

        // A public instance field named X with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int X; 
        // A public instance field named Y with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int Y; 
        // A public instance field named Z with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int Z;

    } // type
} // namespace
namespace Vim.Math3d
{
    // Type has fields True
    // Type has writable fields True
    // Type has public setters False
    public struct Int4
    {
        // A public instance method named .ctor with a type void
        // operation kind is DeconstructionAssignment and type (int X, int Y, int Z, int W)
        // member references = X, Y, Z, W
        // assignments = Conversion
        // Written symbols are (Name=this Kind=Parameter)
        // Read symbols are (Name=x Kind=Parameter), (Name=y Kind=Parameter), (Name=z Kind=Parameter), (Name=w Kind=Parameter)
        // Captured symbols are 
        // Variables declared are 
                public Int4(int x, int y, int z, int w)
            => (X, Y, Z, W) = (x, y, z, w);

        // A public instance field named X with a type int
        // No associated operation
        // No data-flow analysis could be created
                public int X; 
        // A public instance field named Y with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int Y; 
        // A public instance field named Z with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int Z; 
        // A public instance field named W with a type int
        // No associated operation
        // No data-flow analysis could be created
        public int W;

    } // type
} // namespace
