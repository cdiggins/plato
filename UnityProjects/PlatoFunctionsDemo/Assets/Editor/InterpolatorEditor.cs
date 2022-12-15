using SolidUtilities;
using SolidUtilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interpolator))]
public class InterpolatorEditor : Editor
{
    SerializedProperty nameProperty;

    public void OnEnable()
    {
        nameProperty = serializedObject.FindProperty(nameof(Interpolator.PropertyName));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var interp = serializedObject.targetObject as Interpolator;

        var propNames = interp.Fields.Select(f => f.Name).ToArray();
        var oldName = nameProperty.stringValue;
        var index = propNames.IndexOf(oldName);
        EditorGUILayout.PrefixLabel("Property Name");
        var newIndex = EditorGUILayout.Popup(index, propNames);
            if (newIndex < 0 || newIndex >= propNames.Length)
            return;
        nameProperty.stringValue = propNames[newIndex];
        var prop = interp.Fields[newIndex];
        EditorGUILayout.LabelField(prop.Type);

        if (prop.Type == "Single")
        {
            prop.F.From = EditorGUILayout.FloatField("From", prop.F.From);
            prop.F.To = EditorGUILayout.FloatField("To", prop.F.To);
        }
        else if (prop.Type == "Vector2")
        {
            prop.V2.From = EditorGUILayout.Vector2Field("From", prop.V2.From);
            prop.V2.To = EditorGUILayout.Vector2Field("To", prop.V2.To);
        }
        else if (prop.Type == "Vector3")
        {
            prop.V3.From = EditorGUILayout.Vector3Field("From", prop.V3.From);
            prop.V3.To = EditorGUILayout.Vector3Field("To", prop.V3.To);
        }
        else if (prop.Type == "Quaternion")
        {
            prop.Q.From = Quaternion.Euler(EditorGUILayout.Vector3Field("From", prop.Q.From.eulerAngles));
            prop.Q.To = Quaternion.Euler(EditorGUILayout.Vector3Field("To", prop.Q.To.eulerAngles));
        }

        prop.Amount = EditorGUILayout.Slider(prop.Amount, 0, 1);
        //var float 

       ///amountProperty.floatValue = EditorGUILayout.Slider(amountProperty.floatValue, 0, 1);
       // TODO: push the "from" value and the "to" value
       serializedObject.ApplyModifiedProperties();
    }
}