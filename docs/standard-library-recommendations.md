# Plato Standard Library Recommendations

These are the highest-leverage recommendations for evolving the Plato standard
library. The main design assumption is that the generated C# library is the
primary user-facing surface, with TypeScript and Rust following the same semantic
model where practical.

## Guiding Principles

1. Prefer familiar C# names when ecosystem gravity is strong.
2. Add semantic types when they encode a real invariant, role, or transform rule.
3. Avoid duplicate concrete types that represent the same value unless they are
   intentionally not interchangeable.
4. Keep constrained values explicit. Do not silently clamp arithmetic unless the
   type name says so.
5. Make every geometry type answer the same practical questions: bounds,
   containment, distance, closest point, intersections, sampling, and transforms.

## Highest Priority Recommendations

### 1. Keep `Vector2`, `Vector3`, and `Vector4` as canonical C# names

Keep the current names:

```plato
type Vector2 { X:Number; Y:Number; }
type Vector3 { X:Number; Y:Number; Z:Number; }
type Vector4 { X:Number; Y:Number; Z:Number; W:Number; }
```

Do not rename `Vector3` to `Vector3D` as the canonical C# type. `Vector3` is the
name C# users expect from `System.Numerics`, Unity, MonoGame, SharpDX-style APIs,
and most generated examples. The small inconsistency with `Point3D` is worth the
familiarity.

Recommended convention:

- `Vector3` is the C#-friendly numeric vector and geometric displacement.
- `Point3D` is an affine point.
- `Direction3D`, `Normal3D`, and `Tangent3D` are semantic roles built on unit
  vectors.
- `Vector3D` may appear as documentation language or a Plato-level alias, but
  should not become a separate public C# struct unless there is a hard type-safety
  reason.

Avoid generating both `Vector3` and `Vector3D` as separate structs. That would
create conversion friction for little benefit.

### 2. Separate points from vectors semantically

`Point2D` and `Point3D` should not behave like general vectors. Points are
affine positions; vectors are linear displacements. This prevents operations like
scaling a point from looking as natural as scaling a displacement.

Recommended model:

```plato
type Point2D implements ICoordinate,IGeometricPrimitive2D,IDifference<Vector2> {
  X:Number;
  Y:Number;
}

type Point3D implements ICoordinate,IGeometricPrimitive3D,IDifference<Vector3> {
  X:Number;
  Y:Number;
  Z:Number;
}
```

Keep operations like:

```plato
Point3D - Point3D -> Vector3
Point3D + Vector3 -> Point3D
Point3D - Vector3 -> Point3D
Vector3 + Vector3 -> Vector3
Vector3 * Number -> Vector3
```

### 3. Use `Amount` for unconstrained interpolation

For `Lerp`, use `Amount`. It is friendly, C#-like, and correctly does not imply
clamping.

```plato
type Amount implements IValue,IOrderable {
  Value:Number;
}

concept IInterpolatable {
  Lerp(a:Self, b:Self, t:Amount):Self;
}
```

Document the meaning:

- `0` returns `a`.
- `0.5` returns the midpoint.
- `1` returns `b`.
- Values below `0` and above `1` extrapolate.

Avoid `UnconstrainedUnitInterval`; it contradicts itself because a unit interval
is specifically `[0,1]`.

Recommended related types:

```plato
type UnitInterval implements IInterval<Number> {}

type UnitAmount implements IValue,IOrderable {
  Value:Number; // constrained to [0,1]
}

type SignedUnitAmount implements IValue,IOrderable {
  Value:Number; // constrained to [-1,1]
}

type Percent implements IValue,IOrderable {
  Ratio:Number; // 0.25 means 25%; not necessarily constrained
}

type Probability implements IValue,IOrderable {
  Value:UnitAmount;
}
```

Use `UnitInterval` for the interval object `[0,1]`, not for a value inside that
interval.

### 4. Add constrained and semantic scalar types

These types make generated APIs clearer and reduce accidental misuse.

```plato
type Byte implements IWholeNumber {}
type BigInteger implements IWholeNumber {}

type Rational implements IValue,IOrderable {
  Numerator:BigInteger;
  Denominator:BigInteger;
}

type Float16 implements IValue {}
type Float32 implements IValue {}
type Float64 implements IValue {}

type FixedDecimal implements IValue,IOrderable {
  Raw:BigInteger;
  DecimalPlaces:Integer;
}

type NonZeroNumber implements IValue,IOrderable { Value:Number; }
type PositiveNumber implements IValue,IOrderable { Value:Number; }
type NonNegativeNumber implements IValue,IOrderable { Value:Number; }
```

Recommended naming choices:

- `BigInteger`, not `BigInt`, because it is clearer and familiar to C# users.
- `Rational`, not `Fraction`, for exact arithmetic.
- `Float16`, not `Half`, because `Half` can read like the value `0.5`.
- `FixedDecimal` for decimal fixed-point values.
- `Byte` for 0..255 color and binary data.

Use explicit construction helpers for constrained types:

```plato
TryCreateUnitAmount(x:Number):Option<UnitAmount>
ClampUnitAmount(x:Number):UnitAmount
```

### 5. Add `Tolerance` and approximate equality early

Geometry, graphics, and numerics need approximate comparison everywhere.

```plato
type Tolerance implements IValue {
  Absolute:Number;
  Relative:Number;
}

concept IApproximate {
  ApproxEquals(a:Self, b:Self, tolerance:Tolerance):Boolean;
}

concept IFinite {
  IsFinite(x:Self):Boolean;
  IsNaN(x:Self):Boolean;
}
```

This should be a first-class standard library feature, not a scattering of helper
functions.

### 6. Add semantic vector roles

`Normal3D` is useful and should exist, but it should be built on unit vectors.
Normals deserve their own type because they transform differently under
non-uniform scale.

```plato
type UnitVector2 implements IValue { X:Number; Y:Number; }
type UnitVector3 implements IValue { X:Number; Y:Number; Z:Number; }

type Direction2D implements IValue { Value:UnitVector2; }
type Direction3D implements IValue { Value:UnitVector3; }

type Normal2D implements IValue { Value:UnitVector2; }
type Normal3D implements IValue { Value:UnitVector3; }

type Tangent3D implements IValue { Value:UnitVector3; }
type Bitangent3D implements IValue { Value:UnitVector3; }

type TangentFrame3D implements IValue {
  Normal:Normal3D;
  Tangent:Tangent3D;
  Bitangent:Bitangent3D;
}
```

Avoid `NormalVector3D` as the preferred name. `Normal3D` is shorter and clearer.

### 7. Add UV and coordinate-space types

UV coordinates should usually be unconstrained because wrapping, tiling, atlases,
and procedural coordinates commonly go outside `[0,1]`.

```plato
type UVCoordinate implements ICoordinate {
  U:Number;
  V:Number;
}

type NormalizedUV implements ICoordinate {
  U:UnitAmount;
  V:UnitAmount;
}

type PixelCoordinate implements ICoordinate {
  X:Integer;
  Y:Integer;
}

type ImageCoordinate implements ICoordinate {
  X:Number;
  Y:Number;
}
```

Use `NormalizedUV` only when the invariant matters.

### 8. Improve color types with scalar semantics

Use `UnitAmount` for normalized color channels and `Byte` for byte color
channels.

```plato
type Color implements ICoordinate {
  R:UnitAmount;
  G:UnitAmount;
  B:UnitAmount;
  A:UnitAmount;
}

type ByteRGB implements ICoordinate {
  R:Byte;
  G:Byte;
  B:Byte;
}

type ByteRGBA implements ICoordinate {
  R:Byte;
  G:Byte;
  B:Byte;
  A:Byte;
}

type Alpha implements IValue { Value:UnitAmount; }
type Opacity implements IValue { Value:UnitAmount; }
```

`Alpha` is best for compositing math. `Opacity` is best for user-facing rendering
properties.

### 9. Add common result and safety containers

These make constrained construction, parsing, and geometric queries easier to
express across C#, TypeScript, and Rust.

```plato
type Option<T> implements IValue {
  HasValue:Boolean;
  Value:T;
}

type Result<T,E> implements IValue {
  Ok:Boolean;
  Value:T;
  Error:E;
}
```

Use `Option<T>` for absent results such as no intersection. Use `Result<T,E>`
when failure has information.

### 10. Add universal geometry query concepts

The biggest usability win is to make all geometry types answer the same practical
questions.

```plato
concept IBounded<TBounds> {
  Bounds(x:Self):TBounds;
}

concept IContains<TPoint> {
  Contains(x:Self, p:TPoint):Boolean;
}

concept IDistanceTo<TPoint> {
  Distance(x:Self, p:TPoint):Number;
}

concept IClosestPoint<TPoint> {
  ClosestPoint(x:Self, p:TPoint):TPoint;
}

concept IIntersectable<TOther,THit> {
  Intersections(a:Self, b:TOther):IArray<THit>;
}
```

Recommended hit records:

```plato
type Hit2D implements IValue {
  Point:Point2D;
  Normal:Normal2D;
  Distance:Number;
  Parameter:Amount;
}

type Hit3D implements IValue {
  Point:Point3D;
  Normal:Normal3D;
  Distance:Number;
  U:Number;
  V:Number;
  Face:Integer;
}

type Projection2D implements IValue {
  Point:Point2D;
  Distance:Number;
  Parameter:Amount;
}

type Projection3D implements IValue {
  Point:Point3D;
  Distance:Number;
  Parameter:Amount;
}
```

### 11. Add curve, surface, and volume analysis concepts

Named curves are useful, but analysis contracts make every curve useful.

```plato
concept ICurveAnalysis<TPoint,TVector> {
  PointAt(c:Self, t:Amount):TPoint;
  TangentAt(c:Self, t:Amount):TVector;
  Length(c:Self):Number;
}

concept ISurfaceAnalysis {
  PointAt(s:Self, uv:UVCoordinate):Point3D;
  NormalAt(s:Self, uv:UVCoordinate):Normal3D;
  Area(s:Self):Number;
}
```

Recommended additions:

- `BSplineCurve2D`, `BSplineCurve3D`
- `NurbsCurve2D`, `NurbsCurve3D`
- `BezierPath2D`, `BezierPath3D`
- `PolyCurve2D`, `PolyCurve3D`
- `ParametricSurface`
- `NurbsSurface`
- `HeightField`
- `SignedDistanceField2D`, `SignedDistanceField3D`
- `VoxelGrid<T>`, `SparseVoxelGrid<T>`
- `VolumeTexture`
- `IsoSurface`

### 12. Add graphics-ready mesh and rendering types

Meshes need attributes, not only points and indices.

```plato
type Vertex3D implements IValue {
  Position:Point3D;
  Normal:Normal3D;
  UV:UVCoordinate;
  Color:Color;
}

type TriangleMeshAttributes3D implements IGeometry3D {
  Positions:IArray<Point3D>;
  Normals:IArray<Normal3D>;
  UVs:IArray<UVCoordinate>;
  Colors:IArray<Color>;
  Indices:IArray<Integer3>;
}
```

Recommended graphics types:

- `Material`
- `PBRMaterial`
- `Texture2D`
- `Image<TPixel>`
- `Camera3D`
- `PerspectiveCamera`
- `OrthographicCamera`
- `Frustum`
- `ViewRay`
- `RayHit3D`
- `SceneNode3D`
- `Instance3D`
- `BoundingVolumeHierarchy`

### 13. Add units and measures for engineering and science

The existing `Time` and `Angle` types point in the right direction. Expand this
into a useful measures layer.

```plato
type Length implements IMeasure { Meters:Number; }
type Area implements IMeasure { SquareMeters:Number; }
type Volume implements IMeasure { CubicMeters:Number; }
type Mass implements IMeasure { Kilograms:Number; }
type Frequency implements IMeasure { Hertz:Number; }
type Speed implements IMeasure { MetersPerSecond:Number; }
type Acceleration implements IMeasure { MetersPerSecondSquared:Number; }
type Force implements IMeasure { Newtons:Number; }
type Pressure implements IMeasure { Pascals:Number; }
type Energy implements IMeasure { Joules:Number; }
type Power implements IMeasure { Watts:Number; }
```

Also make `Angle` explicit about radians/degrees conversion.

### 14. Add domain packages after the core is stable

These are valuable, but should come after the scalar, geometry, and query
foundations.

DSP and audio:

- `SampleRate`
- `Frequency`
- `Phase`
- `Amplitude`
- `Decibel`
- `Signal<T>`
- `AudioBuffer`
- `Spectrum`
- `FFTFrame`
- `WindowFunction`
- `FIRFilter`
- `IIRFilter`
- `BiquadFilter`

Electronics:

- `Voltage`
- `Current`
- `Resistance`
- `Capacitance`
- `Inductance`
- `Impedance`
- `Phasor`
- `TransferFunctionS`
- `TransferFunctionZ`
- `BodePoint`
- `FrequencyResponse`

Machine learning and statistics:

- `Tensor<T>`
- `TensorShape`
- `FeatureVector`
- `Embedding`
- `Logit`
- `ProbabilityVector`
- `SummaryStatistics`
- `Histogram`
- `NormalDistribution`
- `UniformDistribution`
- `ConfusionMatrix`

Physics:

- `Velocity3D`
- `Acceleration3D`
- `AngularVelocity`
- `Force`
- `Torque`
- `Momentum`
- `RigidBodyState`
- `MassProperties`
- `InertiaTensor`
- `StressTensor`
- `StrainTensor`

Fuzzy math:

- `TruthValue`
- `Membership`
- `FuzzySet<T>`
- `MembershipFunction<T>`
- `FuzzyNumber`
- `FuzzyInterval`
- `TriangularFuzzyNumber`
- `TrapezoidalFuzzyNumber`

Genetics and bioinformatics:

- `Nucleotide`
- `Codon`
- `AminoAcid`
- `DNASequence`
- `RNASequence`
- `ProteinSequence`
- `GenomeCoordinate`
- `GenomeInterval`
- `Variant`
- `Kmer`
- `Motif`
- `PositionWeightMatrix`

## Recommended Implementation Order

1. Fix existing generic typos and concept implementation mismatches.
2. Keep `Vector2`, `Vector3`, `Vector4` canonical; document the naming rule.
3. Separate point and vector semantics.
4. Add `Amount`, `UnitAmount`, `Percent`, `Probability`, and `UnitInterval`.
5. Add `Byte`, `BigInteger`, `Rational`, `Float16`, and `FixedDecimal`.
6. Add `Tolerance`, `IApproximate`, `Option<T>`, and `Result<T,E>`.
7. Add `UnitVector3`, `Direction3D`, `Normal3D`, `UVCoordinate`, and `NormalizedUV`.
8. Add geometry query concepts: bounds, contains, distance, closest point, and intersections.
9. Add curve, surface, mesh, and volume analysis contracts.
10. Add domain modules for graphics, DSP, electronics, physics, ML/statistics, fuzzy math, and genetics.

## Summary

The best near-term direction is not to add many isolated shape names. It is to
add the semantic scalar types and cross-cutting geometry contracts that make all
shapes, curves, surfaces, meshes, and fields easier to use.

For C# consumers, keep the familiar names where they matter: `Vector3`,
`Matrix4x4`, and `Quaternion`. Add semantic names where they prevent mistakes:
`Amount`, `UnitAmount`, `Normal3D`, `UVCoordinate`, `Tolerance`, `Probability`,
`Rational`, and `FixedDecimal`.
