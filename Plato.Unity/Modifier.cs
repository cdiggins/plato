using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plato.Geometry;
using UnityEngine;

/*
 * Ideas:
 * - Spherify
 * - Boxify
 * - Capsulize
 * - Push
 * - Push with intersection
 * - Align on boundaries
 * - Drop small triangles
 * - Decimate
 * - Retessellate
 * - Deform: bend/twist/taper/skew/spiral/ripple/stretch 
 * - Trig: Sine wave / Cosine / etc. 
 * - Attract / Repel
 * - Show normals (create a line renderer for it)
 * - Curvature
 * - Mirror / reflect
 * - Slice
 * - Collide
 * - Elastic 
 * - Show colors
 * - Symmetry 
 * - Apply a vector field
 * - Some kind of lattice
 * - Subdivide 
 * - Flock: 
 * - Cloth
 * - Melt
 * - Noise:
 * - Clone
 * - Stack
 * - Translate / Rotate /
 * - Recompute normals - - https://gist.github.com/unitycoder/81888c54f87b56113f17a5c8eb6bb32b
 *  https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/transforming-normals
 * - Change pivot
 * - Show stats
 * - Morph-to
 * - Lines to 
 * 
 * 
 */
namespace Plato.Unity
{
    // https://github.com/keenanwoodall/Deform/blob/master/Code/Runtime/TriMesh/Deformable.cs
    // Why is there a culling mode?
    // We are going to have to track when a deformer is removed. 
    // Note: if there is a chain of modifiers, the first one is going to have something. 
    // Fall-offs. 

    // I think I am going to have to have a central repository for storing the original meshes.
    // OR ... each thing becomes a procedural. A procedural is a game object with a component, that has a mesh filter, and mesh renderer. 
    // It may start from a base object, or it may import another object? 

    // In order for two objects to share a script ... I may also want to have a "ReferenceScript" option. 
    // So if the whole thing is based on a procedural, how do I assure the person has a procedural? Well the procedural references another mesh. 
    // Or it is a function for defining one. But if someone does something sick like create a feedback loop, well, not my problem.
    // Unity wasn't made for modifying meshes, so not really my problem. 
    // What is interesting is that you can take your procedural stack, and just reference something else. Not my problem! 
    // Then there are some interesting choices to be made ... keep the mesh where it was, or put it where the stack is (which I find interesting). 
    // Then use the original or current transform ... or another transform (which is the most flexible choice). 

    // All of this needs to be discussed with beans.


    [RequireComponent(typeof(ProceduralMesh))]
    [RequireComponent(typeof(MeshFilter))]
    public class Modifier : MonoBehaviour
    {
        // TODO: make this a pull-down.
        public Func<IMesh, IMesh> Function;

        public virtual void Update()
        {
        }

        public virtual void OnStart()
        {

        }
    }

    public enum Attribute
    {
        Position,
        Color,
        Uv,
        Normal,
    }

    public enum FallOffFunction
    {
        None,
        Cubic
    }

    public class Deformer : Modifier
    {
        public Transform ModifierTransform;
        public Transform Influence;
        public float FallOffAmount;
        public FallOffFunction FallOffFunction = FallOffFunction.Cubic;
        public float Magnitude;

        public Attribute AffectedAttribute = Attribute.Position;
    }

    // So it just goes on an object. The "transform" can be updated and controlled.
    // What about influence. That should be an object. And the fall-off could 
}

/* More ideas
 * Bezier Attractor
Bend
Bubble
Bulge
Collision Deform
Conform
Conform Multi
Crumple
Curve Deform
Curve Sculpt
Curve Scult Layered
Cylindrify
Deformable
Displace
Displace Limits
Displace Web Cam
Displace Render target
Dynamic Ripple
FFD 2x2x2
FFD 3x3x3
FFD 4x4x4
2D versions of FFD for Sprites
Globe
Hit Deform
Hump
Melt
Noise
Page Flip
Paint
Path Deform
Pivot Adjust
Point Cache
Push
Radial Skew
Relax
Ripple
Rolled
Rope Deform
Rubber
Scale
Simple Mod
Sinus Curve
Skew
Spherify
Squeeze
Stretch
Taper
Tree Bend
Twist
Vertex Anim
Vert Noise
Wave
Waving
World Path Deform
Bend
Bubble
Cylindrify
FFD 2x2x2
FFD 3x3x3
FFD 4x4x4
Globe
Hump
Melt
Noise
Ripple
Sinus Curve
Skew
Spherify
Squeeze
Stretch
Taper
Twist
Wave
Waving
ZStretch Noise
 * */