using System;
using System.Collections.Generic;
using Plato;
using UnityEngine;
using AnimationState = Plato.AnimationState;
using Time = Plato.Time;

[ExecuteInEditMode]
public class Animator : MonoBehaviour
{
    public AnimationState State = new(true, false, 0, Time.MaxValue, 0, 0);
    public readonly List<Action<AnimationState>> Actions = new();
    public Time CurrentTime => UnityEngine.Time.timeAsDouble;

    public void Start()
    {
        State = State.Start(CurrentTime);
    }

    public void Update()
    {
        State = State.Update(CurrentTime);
        if (!State.Active) return;
        foreach (var action in Actions)
            action(State);
    }

    public void Stop()
    {
        State = State.Stop(CurrentTime);
    }

    public void Resume()
    {
        State = State.Resume(CurrentTime);
    }

    public static Animator FindOrCreate()
        => FindObjectOfType<Animator>() ?? CreateAnimator();

    public static Animator CreateAnimator()
        => new GameObject().AddComponent<Animator>();
}

public static class ReflectionUtils
{
    public static object GetValue(this Vector3 self, string name)
    {
        if (name == "X") return self.x;
        if (name == "Y") return self.y;
        if (name == "Z") return self.z;
        if (name == "this") return self;
        throw new Exception();
    }

    public static string[] GetNames(this Vector3 self)
    {
        return new[] { "X", "Y", "Z" };
    }

    public static Func<object> GetFunc0(this Vector3 self, string name)
    {
        if (name == "Normal") return () => { var r = self; r.Normalize(); return r; };
        throw new Exception();
    }

    public static Func<object, object> GetFunc1(this Vector3 self, string name)
    {
        if (name == "Dot") return (x) => Vector3.Dot(self, (Vector3)x);
        throw new Exception();
    }

    public static AnimatedValue<Vector3> Animate(this Vector3 from, Vector3 to, EaseType type = EaseType.Linear, double duration = 1)
        => AnimationExtensions.Animate(from, to, (a, b, t) => Vector3.Lerp(a, b, (float)t), type, duration);
}


/*
public enum ConditionType {
    GreaterThan,
    LessThan,
    EqualTo,
}

public class Trigger : MonoBehaviour
{
    public string PropertyName { get; }
    public double Value { get; }
    public ConditionType ConditionType { get; }
    
    public void Update()
    {
    }
}
*/
