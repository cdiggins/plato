using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ComponentProperty
{
    public Component Component;
    public Type ComponentType;
    public Type PropertyType;
    public string Name;
    public Func<object> Getter;
    public Action<object> Setter;
    
    public string LongName => $"{ComponentType.Name}.{Name}";
    public bool ReadOnly => Setter != null;

    public static List<ComponentProperty> GetSubProperties(ComponentProperty cp)
    {
        var r = new List<ComponentProperty>();

        var fields = cp.PropertyType.GetFields();
        foreach (var f in fields)
        {
            if (ProcessField(f))
            {
                var cp2 = new ComponentProperty
                {
                    Component = cp.Component,
                    ComponentType = cp.ComponentType,
                    PropertyType = f.FieldType,
                    Name = $"{cp.Name}.{f.Name}",
                    Getter = () => f.GetValue(cp.Getter()),
                    Setter = cp.ReadOnly || f.IsInitOnly
                        ? null
                        : x =>
                        {
                            var tmp = cp.Getter();
                            f.SetValue(tmp, x);
                            cp.Setter(tmp);
                        }
                };
                r.Add(cp2);
            }
        }

        var props = cp.PropertyType.GetProperties();
        foreach (var p in props)
        {
            if (ProcessProperty(p))
            {
                var cp2 = new ComponentProperty
                {
                    Component = cp.Component,
                    ComponentType = cp.ComponentType,
                    PropertyType = p.PropertyType,
                    Name = $"{cp.Name}.{p.Name}",
                    Getter = () => p.GetValue(cp.Getter()),
                    Setter = !cp.ReadOnly && p.CanWrite
                        ? x =>
                        {
                            var tmp = cp.Getter();
                            p.SetValue(tmp, x);
                            cp.Setter(tmp);
                        }
                        : null
                };
                r.Add(cp2);
            }
        }

        return r;
    }

    public static bool Ignore(Type t)
    {
        return t == typeof(Matrix4x4) || t == typeof(GameObject) || typeof(GameObject).IsAssignableFrom(t) || typeof(IEnumerable).IsAssignableFrom(t);
    }

    public static bool ProcessProperty(PropertyInfo p)
    {
        return p.CanRead && !p.IsStatic() && p.IsPubliclyGettable() && !Ignore(p.PropertyType) && p.GetIndexParameters().Length == 0 && p.Name != "value__" && p.Name != "mesh" && p.Name != "material";
    }

    public static bool ProcessField(FieldInfo f)
    {
        return !f.IsStatic && f.IsPublic && !Ignore(f.FieldType) && f.Name != "value__" && f.Name != "mesh" && f.Name != "material";
    }

    public static bool ProcessComponent(Component component)
    {
        return !(component is MeshRenderer) && !(component is Collider);
    }

    public static List<ComponentProperty> GetProperties(Component component)
    {
        var compType = component.GetType();
        var fields = compType.GetFields();
        var r = new List<ComponentProperty>();

        if (!ProcessComponent(component))
            return r;

        foreach (var f in fields)
        {
            if (ProcessField(f))
            {
                var cp = new ComponentProperty
                {
                    Component = component,
                    ComponentType = compType,
                    PropertyType = f.FieldType,
                    Name = f.Name,
                    Getter = () => f.GetValue(component),
                    Setter = f.IsInitOnly ? null : x => f.SetValue(component, x)
                };
                r.Add(cp);
                r.AddRange(GetSubProperties(cp));
            }
        }

        var props = compType.GetProperties();
        foreach (var p in props)
        {
            // Can't read properties are very rare, but we skip them
            if (ProcessProperty(p))
            {
                var cp = new ComponentProperty
                {
                    Component = component,
                    ComponentType = compType,
                    PropertyType = p.PropertyType,
                    Name = p.Name,
                    Getter = () => p.GetValue(component),
                    Setter = p.CanWrite ? x => p.SetValue(component, x) : null
                };

                r.Add(cp);
                r.AddRange(GetSubProperties(cp));
            }
        }

        return r;
    }

    public static List<ComponentProperty> GetProperties(GameObject gameObject)
    {
        var r = new List<ComponentProperty>();
        foreach (var c in gameObject.GetComponents<Component>())
        {
            r.AddRange(GetProperties(c));
        }
        return r;
    }
}

[Serializable]
public class NumericProperty 
{
    public double Value;
    public static implicit operator double(NumericProperty prop) => prop.Value;
    public double OldValue;

    public bool CheckModifiedAndReset()
    {
        if (Value.Equals(OldValue))
        {
            return false;
        }
        OldValue = Value;
        return true;
    }

    public void Update()
    {
        /*
        if (Getter == null || Setter == null)
        {
            var fieldInfo = Component.GetType().GetField(Name);
            if (fieldInfo != null)
            {
                // TODO: much more complex logic needs to be supported here.
                // For example: if the field is ".x" or ".position.length()"
                // TODO: how do I assure that when the component is changed that we use the correct getter/setter. 
                Getter = () => (double)fieldInfo.GetValue(Component);
                Setter = x => { fieldInfo.SetValue(Component, x); };
            }
        }

        if (Getter != null)
        {
            Value = Getter.Invoke();
        }
        */
    }

    public Func<double> Getter;
    public Action<double> Setter;
    public GameObject GameObject;
    public string Name;

    // TODO: this could be a value that when you change it it animates smoothly.
    // This would suggest that there is a "IsAnimated" property, and a duration, and an easing curve. 
    // The thing is that this would always have to animate in the same way. 
}