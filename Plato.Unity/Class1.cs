using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
        public UnityEngine.Color Color;
        public bool Visibility;
        public Vector3 Velocity;
        public Quaternion RotationalVelocity;
        public float Birth;
    }

    public class CloneData
    {
        public Mesh Mesh;
        public Material Material;

        // https://toqoz.fyi/thousands-of-meshes.html
        public List<CloneInstanceData> Instances = new List<CloneInstanceData>();
    }

    [RequireComponent(typeof(PCloner))]
    public class PNode : MonoBehaviour
    { }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [Description("Creates a cloner")]
    public class PCloner 
    {
        public int Copies;
        
        public void Update()
        {
            Graphics.DrawMeshInstancedIndirect(me)
        }
    }

    public class PSymmetry : PNode
    {

    }

    [Description("Changes strength of previous node based on distance from")]
    public class PInfluence : PNode
    {
        public Transform Transform;
        public float Influence = 1;
        public float Distance = 10;
        public EaseType EaseType = EaseType.Linear;
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

    public class PShuffle : PNode
    { }

    [Description("Changes order of nodes")]
    public class PReverse : PNode 
    { }

    public class PVisibility : PNode
    {

    }

    [Description("Changes order of nodes")]
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

}
