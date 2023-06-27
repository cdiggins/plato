using UnityEngine;

public class PRandomRotateAroundZ: MonoBehaviour
{
    public void Start()
    {
        float amount = Random.value * 360.0f;
        transform.eulerAngles = new Vector3(0, amount, 0);
    }
}