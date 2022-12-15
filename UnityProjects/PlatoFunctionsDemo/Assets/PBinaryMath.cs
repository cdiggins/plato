using System;
using UnityEngine;

public class PBinaryMath : MonoBehaviour
{
    public MathOperation Operation;
    public NumericProperty A;
    public NumericProperty B;
    public double Result;

    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Lesser,
        Greater,
        Average,
        Power,
        Log,
        Perlin,
        DeltaAngle,
        Magnitude,
        And,
        Or,
        Xor,
        Nand,
        Nor,
    }

    public void Start()
    {
        A.GameObject = gameObject;
        A.Name = "Transform.position.x";
        B.GameObject = gameObject;
        B.Name = "Transform.eulerAngles.y";
    }

    public void Update()
    {
        switch (Operation)
        {
            case MathOperation.Add:
                Result = A + B;
                break;
            case MathOperation.Subtract:
                Result = A - B;
                break;
            case MathOperation.Multiply:
                Result = A * B;
                break;
            case MathOperation.Divide:
                Result = A / B;
                break;
            case MathOperation.Modulo:
                Result = A % B;
                break;
            case MathOperation.Lesser:
                Result = Math.Min(A, B);
                break;
            case MathOperation.Greater:
                Result = Math.Max(A, B);
                break;
            case MathOperation.Average:
                Result = (A + B) * 0.5f;
                break;
            case MathOperation.Power:
                Result = Math.Pow(A, B);
                break;
            case MathOperation.Log:
                Result = Math.Log(A, B);
                break;
            case MathOperation.Perlin:
                Result = Mathf.PerlinNoise((float)A, (float)B);
                break;
            case MathOperation.DeltaAngle:
                Result = Mathf.DeltaAngle((float)A, (float)B);
                break;
            case MathOperation.Magnitude:
                Result = Math.Sqrt(A * A + B * B);
                break;
            case MathOperation.And:
                Result = A != 0 && B != 0 ? 1.0 : 0;
                break;
            case MathOperation.Or:
                Result = A != 0 || B != 0 ? 1.0 : 0;
                break;
            case MathOperation.Xor:
                Result = A != 0 ^ B != 0 ? 1.0 : 0;
                break;
            case MathOperation.Nand:
                Result = A != 0 && B != 0 ? 0 : 1.0;
                break;
            case MathOperation.Nor:
                Result = A != 0 || B != 0 ? 0 : 1.0;
                break;
        }
    }
}