using UnityEngine;

[ExecuteAlways]
public class PRotateAround : MonoBehaviour
{
    public Transform Center;
    private Vector3 Position;
    private Quaternion Rotation;
    public float Amount;

    public void Start()
    {
        Position = transform.position;
        Rotation = transform.rotation;
    }   

    public void Update()
    {
        var axis = Center.TransformVector(Vector3.up);
        transform.position = Position;
        transform.rotation = Rotation;
        transform.RotateAround(Center.position, axis, Amount);
    }
}