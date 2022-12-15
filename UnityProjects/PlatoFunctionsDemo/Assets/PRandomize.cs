using UnityEngine;

public class PRandomize : MonoBehaviour
{
    private float X;
    private float Y;

    public float Amount = 1;
    public float Offset = 1;
    public float Speed = 0.1f;
    public bool Animate;

    public float Noise(int n)
        => Mathf.PerlinNoise(X + Offset * n, Y);

    public void Start()
    {
        Y = Time.time * Speed;
    }

    public void Update()
    {
        if (Animate)
            Y = Time.time * Speed;

        transform.position = new Vector3(Noise(0), Noise(1), Noise(2)) * Amount;
        transform.eulerAngles = new Vector3(Noise(3) * 360, Noise(4) * 360, Noise(5) * 360) * Amount;
        transform.localScale = new Vector3(0.5f + Noise(6), 0.5f + Noise(7), 0.5f + Noise(8)) * Amount;
    }
}