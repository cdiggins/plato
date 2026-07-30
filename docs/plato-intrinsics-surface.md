# Plato intrinsic surface — every function a backend must supply

An **intrinsic** is a function the language declares but does not define: a signature
terminated by `;` with no `=>` body, inside a `library` block. The host runtime supplies the
implementation. This file is the complete contract a backend must satisfy — for C# that is the
handwritten `Plato.Intrinsics.V2`; for a new backend (C++, GLSL, Rust) it is the porting
checklist.

**328 intrinsics in one file** (stdlib as of 2026-07-30): `foundation/intrinsics.library.plato`
is the only file in the library that contains bodiless declarations, and it stays that way.
Because a tier folder may reference only itself and the folders before it, a host function on a
non-foundation type cannot be declared there at all — so it must not be a host function. `Plane`
was the last such case; its ten operations are now ordinary Plato in
`geometry/lines-planes.library.plato`.

Bodiless declarations inside a `concept` are NOT intrinsics — those are obligations on whoever
implements the concept, and the stdlib satisfies them in Plato. Only `library` blocks declare
host intrinsics.

Policy (from `intrinsics.library.plato`): a function may be declared intrinsic only if
every priority-1..4 backend — C#, C++, CUDA, TypeScript — can supply it natively or with a
trivial shim. Host-specific things (C# SIMD types, IEEE `nextafter`-grade functions,
midpoint-rounding variants) are deliberately excluded.

This list is GENERATED. Regenerate rather than hand-edit, so it cannot drift:

```bash
grep -rEc "^\s+[A-Za-z_][A-Za-z0-9_]*\(.*\)\s*:\s*[^;=]+;\s*$" stdlib/*/*.library.plato | grep -v ":0$"
```

## `foundation/intrinsics.library.plato` (328)

**Number**

- `Abs(self: Number): Number`
- `Acos(self: Number): Angle`
- `Acosh(self: Number): Angle`
- `Asin(self: Number): Angle`
- `Asinh(self: Number): Angle`
- `Atan(self: Number): Angle`
- `Atan2(self: Number, x: Number): Angle`
- `Atanh(self: Number): Angle`
- `Cbrt(self: Number): Number`
- `Ceiling(self: Number): Number`
- `Clamp(self: Number, min: Number, max: Number): Number`
- `CopySign(self: Number, y: Number): Number`
- `Exp(self: Number): Number`
- `Floor(self: Number): Number`
- `FusedMultiplyAdd(self: Number, y: Number, z: Number): Number`
- `Log(self: Number, newBase: Number): Number`
- `Log10(self: Number): Number`
- `Log2(self: Number): Number`
- `Min(self: Number, other: Number): Number`
- `Max(self: Number, other: Number): Number`
- `NaturalLog(self: Number): Number`
- `Pow(self: Number, power: Number): Number`
- `Reciprocal(self: Number): Number`
- `Round(self: Number, digits: Integer): Number`
- `Sign(self: Number): Integer`
- `Sqrt(self: Number): Number`
- `Square(self: Number): Number`
- `Truncate(self: Number): Number`
- `Add(a: Number, b: Number): Number`
- `Subtract(a: Number, b: Number): Number`
- `Multiply(a: Number, b: Number): Number`
- `Divide(a: Number, b: Number): Number`
- `Modulo(a: Number, b: Number): Number`
- `Negative(n: Number): Number`
- `Equals(a: Number, b: Number): Boolean`
- `NotEquals(a: Number, b: Number): Boolean`
- `LessThan(a: Number, b: Number): Boolean`
- `LessThanOrEquals(a: Number, b: Number): Boolean`
- `GreaterThan(a: Number, b: Number): Boolean`
- `GreaterThanOrEquals(a: Number, b: Number): Boolean`
- `Compare(a: Number, b: Number): Integer`
- `Hash(self: Number): Integer`
- `MinValue(_: Number): Number`
- `MaxValue(_: Number): Number`
- `Epsilon(_: Number): Number`
- `Zero(_: Number): Number`
- `One(_: Number): Number`
- `Pi(_: Number): Number`
- `Tau(_: Number): Number`
- `E(_: Number): Number`
- `IsNaN(self: Number): Boolean`
- `IsInfinite(self: Number): Boolean`
- `IsFinite(self: Number): Boolean`
- `ToInteger(self: Number): Integer`

**Integer**

- `Abs(self: Integer): Integer`
- `Add(a: Integer, b: Integer): Integer`
- `Subtract(a: Integer, b: Integer): Integer`
- `Multiply(a: Integer, b: Integer): Integer`
- `Divide(a: Integer, b: Integer): Integer`
- `Modulo(a: Integer, b: Integer): Integer`
- `Negative(n: Integer): Integer`
- `Sign(self: Integer): Integer`
- `Min(a: Integer, b: Integer): Integer`
- `Max(a: Integer, b: Integer): Integer`
- `Equals(a: Integer, b: Integer): Boolean`
- `NotEquals(a: Integer, b: Integer): Boolean`
- `LessThan(a: Integer, b: Integer): Boolean`
- `LessThanOrEquals(a: Integer, b: Integer): Boolean`
- `GreaterThan(a: Integer, b: Integer): Boolean`
- `GreaterThanOrEquals(a: Integer, b: Integer): Boolean`
- `Compare(a: Integer, b: Integer): Integer`
- `Hash(self: Integer): Integer`
- `BitwiseAnd(a: Integer, b: Integer): Integer`
- `BitwiseOr(a: Integer, b: Integer): Integer`
- `BitwiseXor(a: Integer, b: Integer): Integer`
- `BitwiseNot(x: Integer): Integer`
- `ShiftLeft(x: Integer, bits: Integer): Integer`
- `ShiftRight(x: Integer, bits: Integer): Integer`
- `MinValue(_: Integer): Integer`
- `MaxValue(_: Integer): Integer`
- `Zero(_: Integer): Integer`
- `One(_: Integer): Integer`
- `ToNumber(self: Integer): Number`
- `Range(self: Integer): Array<Integer>`

**Boolean**

- `And(a: Boolean, b: Boolean): Boolean`
- `Or(a: Boolean, b: Boolean): Boolean`
- `Not(b: Boolean): Boolean`
- `ExclusiveOr(a: Boolean, b: Boolean): Boolean`
- `Equals(a: Boolean, b: Boolean): Boolean`
- `Compare(a: Boolean, b: Boolean): Integer`
- `Hash(self: Boolean): Integer`

**Angle**

- `Angle(x: Number): Angle`
- `Cos(self: Angle): Number`
- `Sin(self: Angle): Number`
- `Tan(self: Angle): Number`
- `SinCos(self: Angle): Tuple2<Number, Number>`
- `Cosh(self: Angle): Number`
- `Sinh(self: Angle): Number`
- `Tanh(self: Angle): Number`
- `Add(a: Angle, b: Angle): Angle`
- `Subtract(a: Angle, b: Angle): Angle`
- `Multiply(a: Angle, x: Number): Angle`
- `Multiply(x: Number, a: Angle): Angle`
- `Divide(a: Angle, x: Number): Angle`
- `Negative(n: Angle): Angle`
- `Compare(a: Angle, b: Angle): Integer`
- `Hash(self: Angle): Integer`

**Number2 — low-level numeric 2-tuple (component-wise; backend-native vector)**

- `Length(self: Number2): Number`
- `LengthSquared(self: Number2): Number`
- `Abs(self: Number2): Number2`
- `Sqrt(self: Number2): Number2`
- `Add(left: Number2, right: Number2): Number2`
- `Subtract(left: Number2, right: Number2): Number2`
- `Multiply(left: Number2, right: Number2): Number2`
- `Multiply(left: Number2, scalar: Number): Number2`
- `Multiply(scalar: Number, right: Number2): Number2`
- `Divide(left: Number2, right: Number2): Number2`
- `Divide(left: Number2, scalar: Number): Number2`
- `Negative(value: Number2): Number2`
- `Dot(self: Number2, right: Number2): Number`
- `Clamp(self: Number2, min: Number2, max: Number2): Number2`
- `Max(self: Number2, value2: Number2): Number2`
- `Min(self: Number2, value2: Number2): Number2`
- `Hash(self: Number2): Integer`

**Number3 — low-level numeric 3-tuple**

- `Length(self: Number3): Number`
- `LengthSquared(self: Number3): Number`
- `Abs(self: Number3): Number3`
- `Sqrt(self: Number3): Number3`
- `Add(left: Number3, right: Number3): Number3`
- `Subtract(left: Number3, right: Number3): Number3`
- `Multiply(left: Number3, right: Number3): Number3`
- `Multiply(left: Number3, scalar: Number): Number3`
- `Multiply(scalar: Number, right: Number3): Number3`
- `Divide(left: Number3, right: Number3): Number3`
- `Divide(left: Number3, scalar: Number): Number3`
- `Negative(value: Number3): Number3`
- `Dot(self: Number3, right: Number3): Number`
- `Clamp(self: Number3, min: Number3, max: Number3): Number3`
- `Max(self: Number3, value2: Number3): Number3`
- `Min(self: Number3, value2: Number3): Number3`
- `Hash(self: Number3): Integer`

**Number4 — low-level numeric 4-tuple; homogeneous coordinates and RGBA math**

- `Length(self: Number4): Number`
- `LengthSquared(self: Number4): Number`
- `Abs(self: Number4): Number4`
- `Sqrt(self: Number4): Number4`
- `Add(left: Number4, right: Number4): Number4`
- `Subtract(left: Number4, right: Number4): Number4`
- `Multiply(left: Number4, right: Number4): Number4`
- `Multiply(left: Number4, scalar: Number): Number4`
- `Multiply(scalar: Number, right: Number4): Number4`
- `Divide(left: Number4, right: Number4): Number4`
- `Divide(left: Number4, scalar: Number): Number4`
- `Negative(value: Number4): Number4`
- `Dot(self: Number4, right: Number4): Number`
- `Clamp(self: Number4, min: Number4, max: Number4): Number4`
- `Transform(self: Number4, matrix: Matrix4x4): Number4`
- `Multiply(matrix: Matrix4x4, self: Number4): Number4`
- `Transform(self: Number4, rotation: Quaternion): Number4`
- `Multiply(rotation: Quaternion, self: Number4): Number4`
- `Max(self: Number4, value2: Number4): Number4`
- `Min(self: Number4, value2: Number4): Number4`
- `Hash(self: Number4): Integer`

**Number8 — low-level numeric 8-tuple (SIMD-width lane math). Priority-1..4**

- `Add(left: Number8, right: Number8): Number8`
- `Subtract(left: Number8, right: Number8): Number8`
- `Multiply(left: Number8, right: Number8): Number8`
- `Multiply(left: Number8, scalar: Number): Number8`
- `Multiply(scalar: Number, right: Number8): Number8`
- `Divide(left: Number8, right: Number8): Number8`
- `Divide(left: Number8, scalar: Number): Number8`
- `Negative(value: Number8): Number8`
- `Abs(self: Number8): Number8`
- `Sqrt(self: Number8): Number8`
- `Dot(self: Number8, right: Number8): Number`
- `Max(self: Number8, value2: Number8): Number8`
- `Min(self: Number8, value2: Number8): Number8`
- `Hash(self: Number8): Integer`

**Vector2D — geometric displacement in 2D. Displacement algebra only:**

- `Normalize(self: Vector2D): Vector2D`
- `Length(self: Vector2D): Number`
- `LengthSquared(self: Vector2D): Number`
- `Add(left: Vector2D, right: Vector2D): Vector2D`
- `Subtract(left: Vector2D, right: Vector2D): Vector2D`
- `Multiply(left: Vector2D, scalar: Number): Vector2D`
- `Multiply(scalar: Number, right: Vector2D): Vector2D`
- `Multiply(left: Vector2D, factors: Number2): Vector2D`
- `Divide(left: Vector2D, scalar: Number): Vector2D`
- `Negative(value: Vector2D): Vector2D`
- `Dot(self: Vector2D, right: Vector2D): Number`
- `Reflect(self: Vector2D, normal: Vector2D): Vector2D`
- `Transform(self: Vector2D, matrix: Matrix3x2): Vector2D`
- `Multiply(matrix: Matrix3x2, self: Vector2D): Vector2D`
- `Transform(self: Vector2D, matrix: Matrix4x4): Vector2D`
- `Multiply(matrix: Matrix4x4, self: Vector2D): Vector2D`
- `Transform(self: Vector2D, rotation: Quaternion): Vector2D`
- `Multiply(rotation: Quaternion, self: Vector2D): Vector2D`
- `TransformNormal(self: Vector2D, matrix: Matrix3x2): Vector2D`
- `TransformNormal(self: Vector2D, matrix: Matrix4x4): Vector2D`
- `Hash(self: Vector2D): Integer`

**Vector3D — geometric displacement in 3D. Displacement algebra only:**

- `Normalize(self: Vector3D): Vector3D`
- `Length(self: Vector3D): Number`
- `LengthSquared(self: Vector3D): Number`
- `Add(left: Vector3D, right: Vector3D): Vector3D`
- `Subtract(left: Vector3D, right: Vector3D): Vector3D`
- `Multiply(left: Vector3D, scalar: Number): Vector3D`
- `Multiply(scalar: Number, right: Vector3D): Vector3D`
- `Multiply(left: Vector3D, factors: Number3): Vector3D`
- `Divide(left: Vector3D, scalar: Number): Vector3D`
- `Negative(value: Vector3D): Vector3D`
- `Dot(self: Vector3D, right: Vector3D): Number`
- `Cross(self: Vector3D, right: Vector3D): Vector3D`
- `Reflect(self: Vector3D, normal: Vector3D): Vector3D`
- `Transform(self: Vector3D, matrix: Matrix4x4): Vector3D`
- `Multiply(matrix: Matrix4x4, self: Vector3D): Vector3D`
- `Transform(self: Vector3D, rotation: Quaternion): Vector3D`
- `Multiply(rotation: Quaternion, self: Vector3D): Vector3D`
- `TransformNormal(self: Vector3D, matrix: Matrix4x4): Vector3D`
- `Hash(self: Vector3D): Integer`

**Matrix3x2**

- `Determinant(self: Matrix3x2): Number`
- `Invert(self: Matrix3x2): Tuple2<Matrix3x2, Boolean>`
- `Add(value1: Matrix3x2, value2: Matrix3x2): Matrix3x2`
- `Subtract(value1: Matrix3x2, value2: Matrix3x2): Matrix3x2`
- `Multiply(value1: Matrix3x2, value2: Matrix3x2): Matrix3x2`
- `Multiply(value1: Matrix3x2, scalar: Number): Matrix3x2`
- `Multiply(scalar: Number, value1: Matrix3x2): Matrix3x2`
- `Divide(value1: Matrix3x2, scalar: Number): Matrix3x2`
- `Equals(a: Matrix3x2, b: Matrix3x2): Boolean`
- `Lerp(self: Matrix3x2, matrix2: Matrix3x2, amount: Number): Matrix3x2`
- `Hash(self: Matrix3x2): Integer`
- `CreateTranslation(_: Matrix3x2, position: Vector2D): Matrix3x2`
- `CreateScale(_: Matrix3x2, scale: Number): Matrix3x2`
- `CreateScale(_: Matrix3x2, scales: Number2): Matrix3x2`
- `CreateScale(_: Matrix3x2, scales: Number2, centerPoint: Point2D): Matrix3x2`
- `CreateRotation(_: Matrix3x2, angle: Angle): Matrix3x2`
- `CreateRotation(_: Matrix3x2, angle: Angle, centerPoint: Point2D): Matrix3x2`

**Matrix4x4**

- `Determinant(self: Matrix4x4): Number`
- `Transpose(self: Matrix4x4): Matrix4x4`
- `Add(value1: Matrix4x4, value2: Matrix4x4): Matrix4x4`
- `Subtract(value1: Matrix4x4, value2: Matrix4x4): Matrix4x4`
- `Multiply(value1: Matrix4x4, value2: Matrix4x4): Matrix4x4`
- `Multiply(value1: Matrix4x4, f: Number): Matrix4x4`
- `Multiply(f: Number, value1: Matrix4x4): Matrix4x4`
- `Divide(value1: Matrix4x4, f: Number): Matrix4x4`
- `Decompose(self: Matrix4x4): Tuple4<Number3, Quaternion, Vector3D, Boolean>`
- `Lerp(self: Matrix4x4, matrix2: Matrix4x4, amount: Number): Matrix4x4`
- `Invert(self: Matrix4x4): Matrix4x4`
- `CanInvert(self: Matrix4x4): Boolean`
- `Hash(self: Matrix4x4): Integer`
- `Rotation(self: Matrix4x4): Quaternion`
- `Translation(self: Matrix4x4): Vector3D`
- `WithTranslation(self: Matrix4x4, translation: Vector3D): Matrix4x4`
- `CreateTranslation(_: Matrix4x4, position: Vector3D): Matrix4x4`
- `CreateScale(_: Matrix4x4, scale: Number): Matrix4x4`
- `CreateScale(_: Matrix4x4, scales: Number3): Matrix4x4`
- `CreateRotationX(_: Matrix4x4, angle: Angle): Matrix4x4`
- `CreateRotationY(_: Matrix4x4, angle: Angle): Matrix4x4`
- `CreateRotationZ(_: Matrix4x4, angle: Angle): Matrix4x4`
- `CreateFromAxisAngle(_: Matrix4x4, axis: Vector3D, angle: Angle): Matrix4x4`
- `CreateFromQuaternion(_: Matrix4x4, quaternion: Quaternion): Matrix4x4`
- `CreateFromYawPitchRoll(_: Matrix4x4, yaw: Angle, pitch: Angle, roll: Angle): Matrix4x4`
- `CreateLookAt(_: Matrix4x4, cameraPosition: Point3D, cameraTarget: Point3D, cameraUpVector: Vector3D): Matrix4x4`
- `CreateWorld(_: Matrix4x4, position: Point3D, forward: Vector3D, up: Vector3D): Matrix4x4`
- `CreatePerspectiveFieldOfView(_: Matrix4x4, fieldOfView: Angle, aspectRatio: Number, nearPlane: Number, farPlane: Number): Matrix4x4`
- `CreatePerspective(_: Matrix4x4, width: Number, height: Number, nearPlaneDistance: Number, farPlaneDistance: Number): Matrix4x4`
- `CreatePerspectiveOffCenter(_: Matrix4x4, left: Number, right: Number, bottom: Number, top: Number, nearPlaneDistance: Number, farPlaneDistance: Number): Matrix4x4`
- `CreateOrthographic(_: Matrix4x4, width: Number, height: Number, zNearPlane: Number, zFarPlane: Number): Matrix4x4`
- `CreateOrthographicOffCenter(_: Matrix4x4, left: Number, right: Number, bottom: Number, top: Number, zNearPlane: Number, zFarPlane: Number): Matrix4x4`
- `CreateBillboard(_: Matrix4x4, objectPosition: Point3D, cameraPosition: Point3D, cameraUpVector: Vector3D, cameraForwardVector: Vector3D): Matrix4x4`
- `CreateConstrainedBillboard(_: Matrix4x4, objectPosition: Point3D, cameraPosition: Point3D, rotateAxis: Vector3D, cameraForwardVector: Vector3D, objectForwardVector: Vector3D): Matrix4x4`

**Quaternion**

- `Length(self: Quaternion): Number`
- `LengthSquared(self: Quaternion): Number`
- `Normalize(self: Quaternion): Quaternion`
- `Conjugate(self: Quaternion): Quaternion`
- `Inverse(self: Quaternion): Quaternion`
- `Add(a: Quaternion, b: Quaternion): Quaternion`
- `Subtract(a: Quaternion, b: Quaternion): Quaternion`
- `Negative(a: Quaternion): Quaternion`
- `Multiply(a: Quaternion, b: Quaternion): Quaternion`
- `Multiply(a: Quaternion, scalar: Number): Quaternion`
- `Divide(a: Quaternion, b: Quaternion): Quaternion`
- `Concatenate(self: Quaternion, value2: Quaternion): Quaternion`
- `Dot(self: Quaternion, quaternion2: Quaternion): Number`
- `Lerp(self: Quaternion, quaternion2: Quaternion, amount: Number): Quaternion`
- `Slerp(self: Quaternion, quaternion2: Quaternion, amount: Number): Quaternion`
- `Hash(self: Quaternion): Integer`
- `CreateFromAxisAngle(_: Quaternion, axis: Vector3D, angle: Angle): Quaternion`
- `CreateFromYawPitchRoll(_: Quaternion, yaw: Angle, pitch: Angle, roll: Angle): Quaternion`
- `CreateFromRotationMatrix(_: Quaternion, matrix: Matrix4x4): Quaternion`

**Array**

- `All(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean`
- `Any(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean`
- `Append(self: Array<$T>, value: $T): Array<$T>`
- `AtModulo(xs: Array<$T>, n: Integer): $T`
- `CartesianProduct(columns: Array<$TColumn>, rows: Array<$TRow>, func: Function2<$TColumn, $TRow, $TResult>): Array2D<$TResult>`
- `Concatenate(xs: Array<$T>, ys: Array<$T>): Array<$T>`
- `Drop(xs: Array<$T>, n: Integer): Array<$T>`
- `EveryNth(self: Array<$T>, n: Integer): Array<$T>`
- `FlatMap(xs: Array<$T1>, f: Function1<$T1, Array<$T2>>): Array<$T2>`
- `MakeArray2D(columns: Integer, rows: Integer, f: Function2<Integer, Integer, $T>): Array2D<$T>`
- `Map(xs: Array<$T1>, f: Function1<$T1, $T2>): Array<$T2>`
- `MapPairs(xs: Array<$T>, f: Function2<$T, $T, $TR>): Array<$TR>`
- `MapTriplets(xs: Array<$T>, f: Function3<$T, $T, $T, $TR>): Array<$TR>`
- `MapQuartets(xs: Array<$T>, f: Function4<$T, $T, $T, $T, $TR>): Array<$TR>`
- `MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>`
- `Prepend(self: Array<$T>, value: $T): Array<$T>`
- `Reduce(xs: Array<$T>, acc: $U, f: Function2<$U, $T, $U>): $U`
- `Repeat(n: Integer, x: $T): Array<$T>`
- `Skip(xs: Array<$T>, n: Integer): Array<$T>`
- `Slice(xs: Array<$T>, from: Integer, to: Integer): Array<$T>`
- `Slices(xs: Array<$T>, size: Integer): Array2D<$T>`
- `SubArray(xs: Array<$T>, from: Integer, count: Integer): Array<$T>`
- `Take(xs: Array<$T>, n: Integer): Array<$T>`
- `TakeLast(xs: Array<$T>, n: Integer): Array<$T>`
- `Zip(xs: Array<$T1>, ys: Array<$T2>, f: Function2<$T1, $T2, $T3>): Array<$T3>`
- `Zip(xs: Array<$T1>, ys: Array<$T2>, zs: Array<$T3>, f: Function3<$T1, $T2, $T3, $T4>): Array<$T4>`

**Array2D and Array3D**

- `At(xs: Array2D<$T>, i: Integer): $T`
- `At(xs: Array3D<$T>, i: Integer): $T`
- `Column(self: Array2D<$T>, col: Integer): Array<$T>`
- `Row(self: Array2D<$T>, row: Integer): Array<$T>`
- `Map(xs: Array2D<$T1>, f: Function1<$T1, $T2>): Array2D<$T2>`

**Growable builder: unique type List<T>.**

- `Count(xs: List<$T>): Integer`
- `At(xs: List<$T>, n: Integer): $T`
- `Add(xs: List<$T>, x: $T): List<$T>`
- `AddRange(xs: List<$T>, values: Array<$T>): List<$T>`
- `Set(xs: List<$T>, i: Integer, x: $T): List<$T>`
- `Freeze(xs: List<$T>): Array<$T>`
- `EmptyList(xs: Array<$T>): List<$T>`

**Fixed-size builder: unique type Buffer<T>.**

- `Count(xs: Buffer<$T>): Integer`
- `At(xs: Buffer<$T>, n: Integer): $T`
- `Set(xs: Buffer<$T>, i: Integer, x: $T): Buffer<$T>`
- `Freeze(xs: Buffer<$T>): Array<$T>`

