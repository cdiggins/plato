# Plato Standard Library

This is the standard library shipped with the Plato programming language. 

## Applications

Primary applications:
* 2D/3D geometry
* Numerical programing
* Graphics
* Rendering

Secondary applications:
* Image processing
* Scientific computing 
* Mathematics 
* Physics
* Animation 
* Motion graphics
* Engineering 
* Game development 
* Visual FX

## About Plato 

Plato is a pure functional language that is cross-compiled to multiple back-ends.

**in priority order**:

1. **C#** (primary; the reference runtime)
2. **C++**
3. **CUDA**
4. **TypeScript**
5. Others as capacity allows: **GLSL**, **Rust**, **Python**

Plato code is monomorphized and aggressively inlined. It has near zero performance overhead for using abstractions.    

Affine types (marked as `unique`) allow imperative style code without breaking purity. 

## Structure

Every file holds exactly one **kind** of declaration.

- `<stem>.concepts.plato` holds concepts
- `<stem>.types.plato` holds types
- `<stem>.library.plato` holds exactly one `library` block 

The library is separated into the following folders:

1. `stdlib/foundation`
2. `stdlib/geometry`
3. `stdlib/graphics`
4. `stdlib/future`

A folder may reference only itself and the folders before it; 

- `future` may reach anything, nothing reaches into `future`. 
- `foundation` reaches nothing, anything can reach into `foundation`

## Conventions and style

Companion docs — read before editing this folder:

- [`CONVENTIONS.md`](CONVENTIONS.md) 
- [`STYLE_GUIDE.md`](STYLE_GUIDE.md) 
