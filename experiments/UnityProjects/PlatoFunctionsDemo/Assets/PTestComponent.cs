using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class PTestComponent : MonoBehaviour
{
    public GameObject GameObject;

    public List<string> Props = new();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject == null) GameObject = gameObject;
        var props = ComponentProperty.GetProperties(GameObject);
        //PropNames = props.Where(p => p.PropertyType == typeof(float) || p.PropertyType == typeof(int) || p.PropertyType == typeof(double)).Select(p => p.LongName).ToList();
        //PropNames = props.Where(p => typeof(float).IsAssignableFrom(p.PropertyType)).Select(p => p.LongName).ToList();
        Props = props.Where(p => p.PropertyType.IsPrimitive).Select(p => $"{p.LongName} = {p.Getter()}").ToList();
    }
}
