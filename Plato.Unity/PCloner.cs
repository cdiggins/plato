using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Component = UnityEngine.Component;
using Random = UnityEngine.Random;


namespace Plato.Unity
{
    public enum EaseType
    {
        Linear,
        QuadraticEaseIn,
        QuadraticEaseOut,
        QuadraticEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        QuarticEaseIn,
        QuarticEaseOut,
        QuarticEaseInOut,
        QuinticEaseIn,
        QuinticEaseOut,
        QuinticEaseInOut,
        SineEaseIn,
        SineEaseOut,
        SineEaseInOut,
        CircularEaseIn,
        CircularEaseOut,
        CircularEaseInOut,
        ExponentialEaseIn,
        ExponentialEaseOut,
        ExponentialEaseInOut,
        ElasticEaseIn,
        ElasticEaseOut,
        ElasticEaseInOut,
        BackEaseIn,
        BackEaseOut,
        BackEaseInOut,
        BounceEaseIn,
        BounceEaseOut,
        BounceEaseInOut,
    }

    public static class Easing
    {
    }

    public class CloneInstanceData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public int Id;
        public float SelectionWeight;
        public UnityEngine.Color Color;
        public float Visibility;
        public Vector3 Velocity;
        public float Mass;
        
        // public Quaternion RotationalVelocity;
        // public float Birth;
        // Stickiness
        // Friction
        // Age
        // Shininess 
    }

    public class CloneData
    {
        public Mesh Mesh;
        public Material Material;

        // https://toqoz.fyi/thousands-of-meshes.html
        public List<CloneInstanceData> Instances = new List<CloneInstanceData>();
    }

    /// <summary>
    /// Do not add directly to a node. 
    /// </summary>
    [RequireComponent(typeof(PCloner))]
    public class PNode : MonoBehaviour
    {
        public Func<CloneData, CloneData> Transform;
    }

    /// <summary>
    /// A modifier receives to previous and next version of a function and
    /// generates a new version.
    /// </summary>
    [RequireComponent(typeof(PCloner))]
    public class PModifier : MonoBehaviour
    {
        public Func<CloneData, CloneData, CloneData> Func;
    }

    [Description("Creates a cloner")]
    public class PCloner 
    {
        public int Copies;
        public bool Draw;
        public Mesh Mesh;
        public Material Material;

        public void Update()
        {
            Graphics.DrawMeshInstancedIndirect()
        }
    }

    public class PReflect : PNode
    {
        public bool KeepOriginal = false;

        public Transform ReflectionPlane;
    }

    [Description("Changes strength of previous node based on distance from")]
    public class PInfluence : PNode
    {
        public Transform Transform;
        public float Influence = 1;
        public float Distance = 10;
        public EaseType EaseType = EaseType.Linear;
    }

    [Description("Follows another node")]
    public class PFollow : PNode
    {
        public Transform Transform;
    }

    public class PSelect : PNode
    {
    }

    [Description("")]
    public class PMove : PNode
    {
        public Transform Transform;
        public float Speed;
    }

    [Description("Orients the objects to look at a target")]
    public class PLookAt : PNode
    {
        public Transform Target;
    }

    public class PCloneAnimate : PNode
    {
    }

    public class PCloneVertices : PNode
    {
        public Mesh Mesh;
    }

    public class PFaces : PNode
    {   
        public Mesh Mesh;
    }

    [Description("Transforms nodes")]
    public class PTransform : PNode
    {
        public UnityEngine.Transform Transform = (new GameObject("POffset Transform")).transform;
        public bool Incremental = true;
        public bool Local = true;
    }

    public enum CurveType
    {
        Sine,
        Circle,
        Parabola,
        Line,
    }

    public class PPath : PNode
    {
        public float Radius = 1;
        // TODO: range slider 
        public float Start = 0;
        public float Length = 100;
        public CurveType Curve = CurveType.Circle;
    }

    public class PMute: PNode
    {
        public float Strength;
    }

    [Description("Changes order of nodes")]
    public class PShuffle : PNode
    { }

    [Description("Changes order of nodes")]
    public class PReverse : PNode 
    { }

    [Description("Changes the visibility of nodes")]
    public class PVisibility : PNode
    {
        public float Visibility;
    }

    public class PFalloff : PNode
    {
        public EaseType EaseIn;
        public EaseType EaseOut;
        public float Center;
        public float Width;
    }

    public class PRandomize : PNode
    {
    }

    public class PAnimate : PNode
    {
        public float Duration;
        public CurveType Curve;
    }

    public class PFlowField : PNode
    {
    }

    [Description("Kills nodes based on some criteria")]
    public class PReaper : PNode
    { }

    [Description("Creates nodes based on some criteria")]
    public class PBirther : PNode
    { }

    [Description("Creates a fireworks")]
    public class PExplode : PNode 
    { }

    public class PSticky : PNode 
    { }

    public class PGravity : PNode
    { }

    // I want this to allow me to make color space animations 
    public class PColor : PNode
    {
    }

    /// <summary>
    /// A generic tween function.  
    /// </summary>
    public class PTweener : MonoBehaviour
    {
        public Component Component;
        public string PropertyName;
        public float OriginalValue;
        public float FromValue = 0;
        public float ToValue = 1;
        public float Amount;
        public EaseType Easing;
        public bool YoYo;
        public bool Loop;
        public bool Animate;
        public float Duration;
        public bool Reset;
    }

    enum Within
    {
            
    }

    public class PMeasure : MonoBehaviour
    {
        public float Result;
    }

    public class PMeasureDistance : PMeasure
    {
        public Transform Self;
        public Transform Other;
    }

    public class PMeasureFalloff : PMeasure
    {

    }

    public class PMeasureTime : PMeasure
    {
        public float Since;
    }

    public class PMeasureScale : PMeasure
    {
        public float Scale;
    }

    public class PEvent
    {
        public bool Happened;
        public float When;
    }

    public class PEventKeypress : PMeasure
    {
        public char KeyToListen;
    }

    public class PCompare : PMeasure
    {
        public float Other;
    }

    public enum CompareType
    {
        LessThan,
        LessThanOrEqualTo,
        EqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
    }

    public class PCompare : PMeasure
    {
        public CompareType CompareType;
        public PMeasure Measurement;
        public float Amount;
        public bool Invert;
        public bool Absolute;
        public float Sensitivity;
    }

    public class PTrigger : MonoBehaviour
    {
        public PMeasure PMeasure;
        public float Amount;
        public float Distance; 
        public Component Component;
        public bool EnableOrDisable = true;
        public float Triggered;
    }

    /// <summary>
    /// Enables
    /// </summary>
    public class PStartTime : MonoBehaviour
    {
        public float Value;
    }
}
