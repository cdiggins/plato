using JetBrains.Annotations;
using Plato;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EasingDemo : MonoBehaviour
{
    public Func<double, double>[] EasingFuncs =
    {
        EasingOperations.Linear,
        EasingOperations.QuadraticEaseIn,
        EasingOperations.QuadraticEaseOut,
        EasingOperations.QuadraticEaseInOut,
        EasingOperations.CubicEaseIn,
        EasingOperations.CubicEaseOut,
        EasingOperations.CubicEaseInOut,
        EasingOperations.QuarticEaseIn,
        EasingOperations.QuarticEaseOut,
        EasingOperations.QuarticEaseInOut,
        EasingOperations.QuinticEaseIn,
        EasingOperations.QuinticEaseOut,
        EasingOperations.QuinticEaseInOut,
        EasingOperations.SineEaseIn,
        EasingOperations.SineEaseOut,
        EasingOperations.SineEaseInOut,
        EasingOperations.CircularEaseIn,
        EasingOperations.CircularEaseOut,
        EasingOperations.CircularEaseInOut,
        EasingOperations.ExponentialEaseIn,
        EasingOperations.ExponentialEaseOut,
        EasingOperations.ExponentialEaseInOut,
        EasingOperations.ElasticEaseIn,
        EasingOperations.ElasticEaseOut,
        EasingOperations.ElasticEaseInOut,
        EasingOperations.BackEaseIn,
        EasingOperations.BackEaseOut,
        EasingOperations.BackEaseInOut,
        EasingOperations.BounceEaseIn,
        EasingOperations.BounceEaseOut,
        EasingOperations.BounceEaseInOut,
    };

    public int Width = 5;
    public int Height = 2;
    public float Amount = 0;
    public bool Animate = true;
    public float TimePeriod = 3;
    public Material LineMaterial;
    public int LineSamples = 20;

    public List<GameObject> GameObjects = new();

    public Vector3 GetSourcePosition(int i) => Vector3.left * Width + Vector3.forward * Height * i;
    public Vector3 GetTargetPosition(int i) => Vector3.right * Width + Vector3.forward * Height * i + Vector3.up * Width;

    public void PlotLines(LineRenderer lineRenderer, int i, Func<double, double> func)
    {
        var p0 = GetSourcePosition(i);
        var p1 = GetTargetPosition(i);
        lineRenderer.positionCount = LineSamples;
        lineRenderer.widthMultiplier = 0.1f;
        for (var index = 0; index < lineRenderer.positionCount; ++index)
        {
            var amt = func(index / (double)lineRenderer.positionCount);
            var pos = Vector3.Lerp(p0, p1, (float)amt);
            lineRenderer.SetPosition(index, pos);
        }
    }

    public void Start()
    {
        for (var i = 0; i < EasingFuncs.Length; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObjects.Add(obj);
            var go = new GameObject();
            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.material = LineMaterial;
            PlotLines(lineRenderer, i, EasingFuncs[i]);
        }
    }

    public void Update()
    {
        if (Animate)
        {
            Amount = UnityEngine.Time.time % TimePeriod / TimePeriod;
        }

        for (var i = 0; i < EasingFuncs.Length; i++)
        {
            var obj = GameObjects[i];

            var source = GetSourcePosition(i);
            var target = GetTargetPosition(i);
            var func = EasingFuncs[i];
            var pos = Vector3.Lerp(source, target, (float)func(Amount));
            obj.transform.position = pos;
        }
    }

}
