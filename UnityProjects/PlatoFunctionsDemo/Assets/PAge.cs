using System.ComponentModel;
using UnityEngine;

[Description("Tracks the age of this component")]
public class PAge : MonoBehaviour
{
    public float Age;
    public float Birthtime;
    public float Lifespan = 10; // float.MaxValue;
    public void OnEnable()
    {
        Birthtime = Time.time;
    }
    public void Update()
    {
        Age = Time.time - Birthtime;
        if (Age > Lifespan)
        {
            Destroy(gameObject);
        }
    }
}