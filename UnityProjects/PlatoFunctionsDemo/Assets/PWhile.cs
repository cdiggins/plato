using UnityEngine;

public class PWhile : MonoBehaviour
{
    public bool Condition;
    public Behaviour Behaviour;
    public void Update()
    {
        if (!Condition && Behaviour != null)
        {
            Behaviour.enabled = false;
            enabled = false;
        }
    }
}