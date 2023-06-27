using UnityEngine;

[ExecuteAlways]
public class PMoveTowards : MonoBehaviour
{
    public float Amount;
    public bool Clamp;
    public Transform Target;
    public bool Position = true;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public bool Rotation = true;
    public Quaternion StartRotation;
    public Quaternion EndRotation;
    public bool Scale = true;
    public Vector3 StartScale;
    public Vector3 EndScale;
    public bool Reset;

    public void OnEnable()
    {
        Recompute();
    }

    public void Recompute()
    {
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        StartScale = transform.localScale;
    }

    public void Update()
    {
        if (Reset)
        {
            Recompute();
            Amount = 0;
            Reset = false;
        }

        EndPosition = Target.position;
        EndRotation = Target.rotation;
        EndScale = Target.localScale;

        if (Clamp)
        {
            if (Position) transform.position = Vector3.Lerp(StartPosition, EndPosition, Amount);
            if (Rotation) transform.rotation = Quaternion.Slerp(StartRotation, EndRotation, Amount);
            if (Scale) transform.localScale = Vector3.Lerp(StartScale, EndScale, Amount);
        }
        else
        {
            if (Position) transform.position = Vector3.LerpUnclamped(StartPosition, EndPosition, Amount);
            if (Rotation) transform.rotation = Quaternion.SlerpUnclamped(StartRotation, EndRotation, Amount);
            if (Scale) transform.localScale = Vector3.LerpUnclamped(StartScale, EndScale, Amount);
        }
    }
}
