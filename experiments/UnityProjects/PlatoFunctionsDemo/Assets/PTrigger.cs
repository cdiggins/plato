using UnityEngine;

public class PTrigger : MonoBehaviour
{
    public Collider Collider;

    public void OnTriggerEnter(Collider other)
    {
        Collider = other;
    }
}