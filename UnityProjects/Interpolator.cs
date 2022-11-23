using JetBrains.Annotations;
using Plato;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

[Serializable]
public class LerpableBase
{
    public Func<object> Getter;
    public Action<object> Setter;

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

    public virtual string GetName() => throw new NotImplementedException();

    public virtual object Evaluate(float t) => throw new NotImplementedException();

    public void Set(float t) 
        => Setter(Evaluate(t));
}

[Serializable]
public class LerpableQuaternion : LerpableBase 
{
    [SerializeField] public string Name;
    [SerializeField] public string Type = "Quaternion";
    [SerializeField] public Quaternion Original;
    [SerializeField] public Quaternion From;
    [SerializeField] public Quaternion To;
    public override object Evaluate(float t) => Quaternion.Slerp(From, To, t);
    public LerpableQuaternion(string name, Func<object> getter, Action<object> setter)
    {
        Name = name;
        From = To = Original = (Quaternion)getter();
        Getter = getter;
        Setter = setter;
    }
}

[Serializable]
public class LerpableVector2 : LerpableBase
{
    [SerializeField] public string Name;
    [SerializeField] public string Type = "Vector2";
    [SerializeField] public Vector2 Original;
    [SerializeField] public Vector2 From;
    [SerializeField] public Vector2 To;
    public override object Evaluate(float t) => Vector2.Lerp(From, To, t);
    public LerpableVector2(string name, Func<object> getter, Action<object> setter)
    {
        Name = name;
        From = To = Original = (Vector2)getter();
        Getter = getter;
        Setter = setter;
    }
}

[Serializable]
public class LerpableVector3 : LerpableBase
{
    [SerializeField] public string Name;
    [SerializeField] public string Type = "Vector3";
    [SerializeField] public Vector3 Original;
    [SerializeField] public Vector3 From;
    [SerializeField] public Vector3 To;
    public override object Evaluate(float t) => Vector3.Lerp(From, To, t);
    public LerpableVector3(string name, Func<object> getter, Action<object> setter)
    {
        Name = name;
        From = To = Original = (Vector3)getter();
        Getter = getter;
        Setter = setter;
    }
}

[Serializable]
public class LerpableSingle : LerpableBase
{
    [SerializeField] public string Name;
    [SerializeField] public string Type = "Single";
    [SerializeField] public Single Original;
    [SerializeField] public Single From;
    [SerializeField] public Single To;
    public override 
    public override object Evaluate(float t) => Mathf.Lerp(From, To, t);
    public LerpableSingle(string name, Func<object> getter, Action<object> setter)
    {
        Name = name;
        From = To = Original = (Single)getter();
        Getter = getter;
        Setter = setter;
    }
}

[Serializable]
public class FieldInterpolator 
{
    [SerializeField] public LerpableSingle Single;
    [SerializeField] public LerpableVector2 Vector2;
    [SerializeField] public LerpableVector3 Vector3;
    [SerializeField] public LerpableQuaternion Quaternion;

    public FieldInterpolator(string name, Func<Vector3> getter, Action<Vector3> setter)
        => Vector3 = new(name, () => getter(), x => setter((Vector3)x));

    public FieldInterpolator(string name, Func<float> getter, Action<float> setter)
        => Single = new(name, () => getter(), x => setter((float)x));

    public FieldInterpolator(string name, Func<Vector2> getter, Action<Vector2> setter)
        => Vector2 = new(name, () => getter(), x => setter((Vector2)x));

    public FieldInterpolator(string name, Func<Quaternion> getter, Action<Quaternion> setter)
        => Quaternion = new(name, () => getter(), x => setter((Quaternion)x));

    public LerpableBase GetLerpable() =>
        Vector3 != null ? Vector3 :
        Vector2 != null ? Vector2 :
        Quaternion != null ? Quaternion :
        Single != null ? Single : null;

    public void Set(float amount) => GetLerpable().Set(amount);
}


[ExecuteInEditMode]
public class Interpolator : MonoBehaviour
{
    public string PropertyName;
    public float Amount;
    public List<FieldInterpolator> Fields = new();

    public FieldInterpolator Create(string name, Type type, Func<object> getter, Action<object> setter)
    {
        if (type == typeof(float)) return new(name, () => (float)getter(), x => setter(x));
        if (type == typeof(Vector2)) return new(name, () => (Vector2)getter(), x => setter(x));
        if (type == typeof(Vector3)) return new(name, () => (Vector3)getter(), x => setter(x));
        if (type == typeof(Quaternion)) return new(name, () => (Quaternion)getter(), x => setter(x));
        return null;
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
                Fields.Add(new("Position", () => t.position, x => t.position = x));
                    Fields.Add(new("Rotation", () => t.rotation, x => t.rotation = x));
                Fields.Add(new("Scale", () => t.localScale, x => t.localScale = x));
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
        field.Set(Amount);
    }
}

// Example of making drop down lists 