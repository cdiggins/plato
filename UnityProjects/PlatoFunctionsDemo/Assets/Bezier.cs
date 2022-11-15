using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plato;

public static class Extensions
{
    public static Angle FractionalAngle(this double value, double max)
        => (value / max).Revs();

    public static Double2 Plot(this Angle angle, Func<Angle, double> f)
        => (angle.Revs(), f(angle));
}

public static class UnityConverters
{
    public static Vector2 ToUnity(this Double2 self)
        => new((float)self.X, (float)self.Y);

    public static Vector3 ToUnity(this Double3 self)
        => new((float)self.X, (float)self.Y, (float)self.Z);
}

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    public double Revolutions = 5;
    public int NumVertices = 100;
    public double radius = 3;
    public int k = 4;
    public int function = 0;

    
    public Double2 Evaluate(double i)
    {
        var angle = i.FractionalAngle(NumVertices); ;

        switch (function)
        {
            case 0: return angle.Circle();
            case 1: return angle.Astroid();
            case 2: return angle.Cardoid();
            case 3: return angle.Deltoid();
            case 4: return angle.Hyperbola();
            case 5: return angle.LeminscateOfGerono();
            case 6: return angle.ClosedEpicycloid(k);
            case 7: return angle.ClosedHypocycloid(k);
            case 8: return angle.ClosedHypocycloid(k);
            case 9: return angle.Quatrefoiloid();
            case 10: return angle.Nephroid();
            case 11: return angle.Plot(TrigOperations.Sin);
            case 12: return angle.Plot(TrigOperations.Cos);
            case 13: return angle.Plot(TrigOperations.Tan);
            case 14: return angle.Epitrochoid(1, k);
            case 15: return angle.Epitrochoid(2, k);
            case 16: return angle.Epitrochoid(3, k);
            case 17: return angle.Epitrochoid(0.5, k);
        }

        return (i, i);
    }

    void Update()
    {
        var lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = NumVertices;

        for (var i=0.0; i < NumVertices; i += 1)
        {
            var pos = Evaluate(i);
            lineRenderer.SetPosition((int)i, pos.ToUnity());
        }
    }
}