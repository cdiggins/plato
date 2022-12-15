using System.ComponentModel;
using UnityEngine;
using Component = UnityEngine.Component;

[Description("Clones a game object when enabled, disables self immediate afterwards.")]
public class PSpawner : MonoBehaviour
{
    public GameObject Object;
    //public bool spawnNow = false;

    public static Component CopyComponent(Component original, GameObject destination) 
    {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type);
        if (!dst) dst = destination.AddComponent(type);
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst;
    }

    public void OnEnable()
    {
        SpawnNow();
    }

    public void Update()
    {
        /*
        if (spawnNow)
        {
            spawnNow = false;
            SpawnNow();
        }*/
    }

    public void SpawnNow()
    {
        if (Object == null) return;
        var spawn = Instantiate(Object);
        spawn.SetActive(true);
        enabled = false;
    }
}