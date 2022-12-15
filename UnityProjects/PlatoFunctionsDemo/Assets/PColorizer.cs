using UnityEngine;

public class PColorizer : MonoBehaviour
{
    public Color color;
    private Material material;
    
    //public bool cloneMaterial = false;

    public Material GetMaterial()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) return null;
        return meshRenderer.sharedMaterial;
    }

    public void CloneMaterial()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) return;
        var material = meshRenderer.sharedMaterial;
        if (material == null) return;
        meshRenderer.sharedMaterial = Instantiate(material);
    }

    public void Start()
    {
        CloneMaterial();
    }

    public void Update()
    {
        /*
        if (cloneMaterial)
        {
            cloneMaterial = false;
            CloneMaterial();
        }*/

        var material = GetMaterial();
        if (material != null)
        {
            material.color = color;
        }
    }
}