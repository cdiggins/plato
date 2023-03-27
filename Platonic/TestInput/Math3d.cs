using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Vim.Math3d
{

    public struct Vector2
    {
        public float X; public float Y;
        public Vector2(float x, float y) 
            => (X, Y) = (x, y);
    }

    public struct Vector3
    {
        public float X; public float Y; public float Z; 
        public Vector3(float x, float y, float z) 
            => (X, Y, Z) = (x, y, z);
    }

    public struct Vector4
    {
        public float X; public float Y; public float Z; public float W;
        public Vector4(float x, float y, float z, float w) 
            => (X, Y, Z, W) = (x, y, z, w);
    }

    public struct DVector2
    {
        public double X; public double Y;
        public DVector2(double x, double y)
            => (X, Y) = (x, y);
    }

    public struct DVector3
    {
        public double X; public double Y; public double Z;
        public DVector3(double x, double y, double z)
            => (X, Y, Z) = (x, y, z);
    }

    public struct DVector4
    {
        public double X; public double Y; public double Z; public double W;
        public DVector4(double x, double y, double z, double w)
            => (X, Y, Z, W) = (x, y, z, w);
    }

    public struct Matrix4x4
    {

        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M11;

        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M12;

        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M13;

        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M14;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M21;

        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M22;

        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M23;

        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M24;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M31;

        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M32;

        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M33;

        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M34;

        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        [DataMember] public float M41;

        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        [DataMember] public float M42;

        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        [DataMember] public float M43;

        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        [DataMember] public float M44;
    }

    public struct AABox
    {
        public Vector3 Min; public Vector3 Max;
    }

    public struct Byte2
    {
        public byte X; public byte Y; 
        public Byte2(byte x, byte y) 
            => (X, Y) = (x, y);
    }

    public struct Byte3
    {
        public byte X; public byte Y; public byte Z; 
        public Byte3(byte x, byte y, byte z)
            => (X, Y, Z) = (x, y, z);
    }

    public struct Byte4
    {
        public byte X; public byte Y; public byte Z; public byte W;
        public Byte4(byte x, byte y, byte z, byte w)
            => (X, Y, Z, W) = (x, y, z, w);
    }

    public struct Int2
    {
        public int X; public int Y;
        public Int2(int x, int y)
            => (X, Y) = (x, y);
    }

    public struct Int3
    {
        public int X; public int Y; public int Z;
        public Int3(int x, int y, int z)
            => (X, Y, Z) = (x, y, z);
    }

    public struct Int4
    {
        public int X; public int Y; public int Z; public int W;
        public Int4(int x, int y, int z, int w)
            => (X, Y, Z, W) = (x, y, z, w);
    }

}
