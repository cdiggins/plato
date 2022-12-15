using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PBinaryMath))]
public class BinaryMathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var obj = (serializedObject.targetObject as Component).gameObject;
        
        var options = ComponentProperty.GetProperties(obj).Where(cp => cp.PropertyType == typeof(float));
        var names = options.Select(cp => cp.LongName).ToArray();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Popup(0, names);
        EditorGUILayout.DoubleField(1.0);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Popup(0, names);
        EditorGUILayout.DoubleField(2.0);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Popup(0, new[] { "Add", "Subtract" });
        EditorGUILayout.DoubleField(3.0);
    }

}