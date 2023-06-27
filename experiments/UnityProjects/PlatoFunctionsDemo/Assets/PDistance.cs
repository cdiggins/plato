using UnityEngine;

public class PDistance : MonoBehaviour
{
    public Transform Source;
    public Transform Target;

    public float Distance;

    public void Update()
    {
        Distance = Vector3.Distance(Source.position, Target.position);
    }
}