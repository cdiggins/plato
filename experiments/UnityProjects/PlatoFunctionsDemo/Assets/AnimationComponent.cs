using System.Reflection;
using Plato;
using UnityEngine;

[ExecuteInEditMode]
public class AnimationComponent : MonoBehaviour
{
    public string PropertyName = "Transform.Position";
    public Vector3 Target = Vector3.up * 5;
    public double Duration = 3;
    public EaseType EaseType = EaseType.Linear;
    public Animator Animator;
    public AnimatedValue<Vector3> AnimatedValue;

    private string _test; 
    public string Test
    {
        get => _test;
        set => Debug.Log(_test = value);
    }

    public static FieldInfo GetField(GameObject go, string propName)
    {
        var parts = propName.Split('.');
        if (parts.Length < 2) return null;
        var comp = go.GetComponent(parts[0]);
        var fi = comp.GetType().GetField(parts[1]);
        return fi;
    }

    public static Vector3 GetVector3(GameObject go, string propName)
        => go.transform.position;

    public Vector3 GetVector3()
        => GetVector3(gameObject, PropertyName);

    public static void Set(GameObject go, string propName, Vector3 value)
        => go.transform.position = value;

    public void Set(Vector3 value)
        => Set(gameObject, PropertyName, value);

    public void Start()
    {
        Animator = Animator == null ? Animator.FindOrCreate() : Animator;
        AnimatedValue = GetVector3().Animate(Target, (a, b, t) => Vector3.Lerp(a, b, (float)t), EaseType, Duration);
    }

    public void Update()
    {
        // TOD0: how do I loop
        if (AnimatedValue != null && Animator != null)
        {
            Set(AnimatedValue.Evaluate(Animator.State));
        }
    }
}