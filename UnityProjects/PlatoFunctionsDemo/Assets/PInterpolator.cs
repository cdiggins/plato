using Plato;
using System;
using UnityEngine;
using Time = UnityEngine.Time;

[ExecuteAlways]
public class PInterpolator : MonoBehaviour
{
    public Component Component;
    public string PropertyName = "Amount";
    //public float OriginalValue;
    //public float FromValue = 0;
    //public float ToValue = 1;
    public float Amount;
    //public bool YoYo;
    //public bool Loop;
    public bool Animate;
    public float Duration = 3;
    public bool Reset;
    private float OldAmount;

    public EaseType Easing;
    private EaseType oldEasing;
    public Func<double, double> EaseFunc;

    public Action<float> Action;

    public Action<float> GetUpdateAction()
    {
        var fieldInfo = Component.GetType().GetField(PropertyName);
        return fieldInfo == null ? x => { } : x => { fieldInfo.SetValue(Component, x); };
    }

    public void Awake()
    {
        var comps = gameObject.GetComponents<Component>();
        var index = Array.IndexOf(comps, this);
        if (index > 1)
            Component = comps[index - 1];
    }

    public void Update()
    {
        if (Animate)
        {
            Amount = Time.time % Duration / Duration;
        }

        if (Component == null)
        {
            Action = null;
            return;
        }

        if (Action == null && Component != null)
        {
            Action = GetUpdateAction();
        }

        if (Easing != oldEasing || EaseFunc == null)
        {
            EaseFunc = Easing.ToFunc();
            oldEasing = Easing;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Amount != OldAmount)
        {
            OldAmount = Amount;

            var easedAmount = Easing == EaseType.Linear ? Amount : EaseFunc(Amount); 
            Action?.Invoke((float)easedAmount);
        }
    }
}