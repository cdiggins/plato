using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine;
using Component = UnityEngine.Component;

[Description("Enables a component every X seconds")]
public class PTimer : MonoBehaviour
{
    public float Duration = 2;
    //public int MaxCount = 100; // int.MaxValue;
    //public int Count = 0;
    public Behaviour Behaviour;
    private float Started;
    private float Elapsed;

    public void Start()
    {
        Started = Time.time;
    }

    public void Update()
    {
        if (Time.time - Started > Duration)
        {
            Started = Time.time;
            Behaviour.enabled = true;
        }
    }
}