using SolidUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//[CustomPropertyDrawer(typeof(NumericProperty))]
public class NumericPropertyDrawer : PropertyDrawer
{
    public void DrawProperty(ref Rect position, SerializedProperty property, GUIContent label)
    {
        var h = EditorGUI.GetPropertyHeight(property);
        position.Set(position.x, position.y, position.width, h);
        if (label != null)
            EditorGUI.PropertyField(position, property, label);
        else
            EditorGUI.PropertyField(position, property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var objProperty = property.FindPropertyRelative("GameObject");
        var nameProperty = property.FindPropertyRelative("Name");
        var valueProperty = property.FindPropertyRelative("Value");

        //position.height = EditorGUI.GetPropertyHeight(objProperty);
        //EditorGUI.PropertyField(position, objProperty);
        //position.y += EditorGUIUtility.singleLineHeight;

        var obj = objProperty.objectReferenceValue as GameObject;
        //var obj = property.com

        var propName = nameProperty.stringValue; 

        var tmp = position;
        tmp.width = EditorGUIUtility.labelWidth;
        EditorGUI.LabelField(tmp, "Property");
        tmp.x += tmp.width;
        tmp.width = position.width - tmp.width;
        var options = ComponentProperty.GetProperties(obj);
        var names = options.Select(cp => cp.LongName).ToArray();
        var nameIndex = names.IndexOf(propName);
        var newNameIndex = EditorGUI.Popup(tmp, nameIndex, names);
        position.y += EditorGUIUtility.singleLineHeight;

        DrawProperty(ref position, valueProperty, new GUIContent(property.name));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var objProperty = property.FindPropertyRelative("GameObject");
        var nameProperty = property.FindPropertyRelative("Name");
        var valueProperty = property.FindPropertyRelative("Value");
        var objField = new PropertyField(objProperty);
        var nameField = new PropertyField(nameProperty);
        var valueField = new PropertyField(valueProperty);
        var height = 0f;
        height += EditorGUI.GetPropertyHeight(objProperty);
        height += EditorGUI.GetPropertyHeight(nameProperty);
        height += EditorGUI.GetPropertyHeight(valueProperty);
        height += EditorGUI.GetPropertyHeight(valueProperty);
        return height;
//        return base.GetPropertyHeight(property, label);
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        /*
        var container = new VisualElement();
        var objProperty = property.FindPropertyRelative("GameObject");
        var nameProperty = property.FindPropertyRelative("Name");
        var valueProperty = property.FindPropertyRelative("Value");
        container.Add(new PropertyField(objProperty));
        container.Add(new PropertyField(nameProperty));
        container.Add(new PropertyField(valueProperty));
        */
        return base.CreatePropertyGUI(property);
    }
}