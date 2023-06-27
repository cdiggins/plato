using UnityEngine;

public class PRotationalDistance : MonoBehaviour
{
    public Transform Source;
    public Transform Target;
    
    // If true, we are talking about the rotational distance to look at something 
    public bool LookAt;

    public Quaternion Result;

    public void Update()
    {
        var targetRotation = LookAt 
            ? Quaternion.LookRotation(Target.position - Source.position) 
            : Target.rotation;

        Result = Quaternion.RotateTowards(Source.rotation, Target.rotation, 360);
    }
}