using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

public class QuaternionRange : ScriptableObject
{
    public Quaternion From;
    public Quaternion To;
    public static QuaternionRange Create(Quaternion q)
    {
        var r = CreateInstance<QuaternionRange>(); r.From = r.To = q; return r;
    }
}
public class Vector3Range : ScriptableObject
{
    public Vector3 From;
    public Vector3 To;
    public static Vector3Range Create(Vector3 v)
    {
        var r = CreateInstance<Vector3Range>(); r.From = r.To = v; return r;
    }
}
public class Vector2Range : ScriptableObject
{
    public Vector2 From;
    public Vector2 To;
    public static Vector2Range Create(Vector2 v)
    {
        var r = CreateInstance<Vector2Range>(); r.From = r.To = v; return r;
    }
}
public class FloatRange : ScriptableObject
{
    public float From;
    public float To;
    public static FloatRange Create(float f)
    {
        var r = CreateInstance<FloatRange>(); r.From = r.To = f; return r; 
    }
}
public class FieldInterpolator : ScriptableObject
{
    public string Name;
    public FloatRange F;
    public QuaternionRange Q;
    public Vector2Range V2;
    public Vector3Range V3;
    public float Amount;

    public object From =>
        F != null ? F.From :
        Q != null ? Q.From :
        V2 != null ? V2.From:
        V3 != null ? V3.From :
        null;

    public object To =>
        F != null ? F.To :
        Q != null ? Q.To :
        V2 != null ? V2.To :
        V3 != null ? V3.To :
        null;

    public string Type =>
        F != null ? "Single" :
        Q != null ? "Quaternion" :
        V2 != null ? "Vector2" :
        V3 != null ? "Vector3" :
        "ErrorType";

    public static FieldInterpolator Create(float f)
    {
        var r = CreateInstance<FieldInterpolator>();
        r.F = FloatRange.Create(f);
        return r;
    }
    public static FieldInterpolator Create(Vector2 v)
    {
        var r = CreateInstance<FieldInterpolator>();
        r.V2 = Vector2Range.Create(v);
        return r;
    }
    public static FieldInterpolator Create(Vector3 v)
    {
        var r = CreateInstance<FieldInterpolator>();
        r.V3 = Vector3Range.Create(v);
        return r;
    }
    public static FieldInterpolator Create(Quaternion q)
    {
        var r = CreateInstance<FieldInterpolator>();
        r.Q = QuaternionRange.Create(q);
        return r;
    }
}

public class InterpolatorFuncs
{
    public Func<object> Getter;
    public Action<object> Setter;
    public Func<object, object, float, object> Lerp;
}

[ExecuteInEditMode]
public class Interpolator : MonoBehaviour
{
    public static Func<object, object, float, object> GetLerpFunction(Type type)
    {
        if (type == typeof(float)) return (a, b, t) => Mathf.Lerp((float)a, (float)b, t);
        if (type == typeof(double)) return (a, b, t) => Mathf.Lerp((float)(double)a, (float)(double)b, t);
        if (type == typeof(bool)) return (a, b, t) => Mathf.Lerp((float)a, (float)b, t) >= 0.5;
        if (type == typeof(int)) return (a, b, t) => (int)Mathf.Lerp((int)a, (int)b, t);
        if (type == typeof(Vector2)) return (a, b, t) => Vector2.Lerp((Vector2)a, (Vector2)b, t);
        if (type == typeof(Vector3)) return (a, b, t) => Vector3.Lerp((Vector3)a, (Vector3)b, t);
        if (type == typeof(Vector4)) return (a, b, t) => Vector4.Lerp((Vector4)a, (Vector4)b, t);
        if (type == typeof(Color)) return (a, b, t) => Color.Lerp((Color)a, (Color)b, t);
        if (type == typeof(Quaternion)) return (a, b, t) => Quaternion.Slerp((Quaternion)a, (Quaternion)b, t);
        return null;
    }

    public string PropertyName;
    public List<FieldInterpolator> Fields = new();
    public Dictionary<string, InterpolatorFuncs> Funcs = new ();
    
    public FieldInterpolator Create(Type type, Func<object> getter)
    {
        var val = getter();
        if (type == typeof(float)) return FieldInterpolator.Create((float)val);
        if (type == typeof(Vector2)) return FieldInterpolator.Create((Vector2)val);
        if (type == typeof(Vector3)) return FieldInterpolator.Create((Vector3)val);
        if (type == typeof(Quaternion)) return FieldInterpolator.Create((Quaternion)val); ;
        return null;
    }

    public FieldInterpolator Create<T>(string name, Func<T> getter, Action<T> setter)
    {
        return Create(name, typeof(T), () => getter(), x => setter((T)x));
    }

    public FieldInterpolator Create(string name, Type type, Func<object> getter, Action<object> setter)
    {
        var r = Create(type, getter);
        if (r == null) return null;
        r.Name = name;
        var funcs = new InterpolatorFuncs
        {
            Getter = getter,
            Setter = setter,
            Lerp = GetLerpFunction(type)
        };
        Funcs.Add(name, funcs);        
        return r;
    }

    public void Awake()
    {
        ComputeInterpolators();
    }

    public void Reset()
    {
        ComputeInterpolators();
    }

    public void ComputeInterpolators()
    { 
        Fields.Clear();
        foreach (var c in gameObject.GetComponents<Component>())
        {
            if (c is Transform t)
            {
                Fields.Add(Create("Position", () => t.position, x => t.position = x));
                Fields.Add(Create("Rotation", () => t.rotation, x => t.rotation = x));
                Fields.Add(Create("Scale", () => t.localScale, x => t.localScale = x));
            }
            else
            {
                foreach (var f in c.GetType().GetFields())
                {
                    var r = Create(f.Name, f.FieldType, () => f.GetValue(c), (x) => f.SetValue(c, x));
                    if (r != null) Fields.Add(r);
                }
            }
        }
    }

    public void Update()
    {
        var field = Fields.Find(p => p.Name == PropertyName);
        if (field == null) return;
        var funcs = Funcs[PropertyName];
        var val = funcs.Lerp(field.From, field.To, field.Amount);
        funcs.Setter(val);
    }
}

// Example of making drop down lists 