using System.ComponentModel;
using UnityEngine;

[Description("Updates translation, rotation, and scale by specified amount over a second")]
[ExecuteAlways]
public class PMover : MonoBehaviour
{
    public float Amount = 1;

    // TODO: if this was a gameObject it could treated like a joystick. 
    // When creating this object, I would create a child gameObject that is used automatically 
    // The neat thing is that we can have acceleration by applying an animation to that object. 
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;

    public static Quaternion FractionOfRotation(Quaternion q, float amount)
        => Quaternion.LerpUnclamped(Quaternion.identity, q, amount);

    public void Update()
    {
        transform.position += transform.TransformDirection(Translation) * Amount * Time.unscaledDeltaTime;
        transform.rotation *= FractionOfRotation(Rotation, Amount * Time.unscaledDeltaTime);
        transform.localScale += Scale * Amount * Time.unscaledDeltaTime;
    }
}