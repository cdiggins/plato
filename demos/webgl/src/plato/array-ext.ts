// Hand-written prelude for the generated stdlib TypeScript.
//
// `Plato.TypeScriptWriter` emits calls to the Plato `Array<T>` library functions
// in extension-method position (`points.Concatenate(more)`,
// `positions.PolygonMeshOfFaces(faces)`) but does not emit the functions
// themselves — `IArray<T>` in `plato.g.ts` declares only At/Count/Map/Reduce.
// Scalar and Point paths (what the SDF demo exercises) are unaffected; the mesh,
// polygon and CSG paths are not reachable without these.
//
// The file has since grown to cover four more shapes of the same problem, each
// in its own section below: dropped OVERLOADS (only the first member of a group
// survives), sum types the writer reports as CHK320 and then keeps calling,
// TUPLE LITERALS emitted as `Tuple2`/`Tuple3` where a vector was meant, and
// Plato `Integer` DIVISION emitted as float division.
//
// Every body here mirrors the Plato source it is named after, and says which
// file that is:
//   stdlib/foundation/collections.library.plato
//   stdlib/foundation/collections-jagged.types.plato
//   stdlib/foundation/primitives.library.plato
//   stdlib/foundation/numeric-structures.library.plato
//   stdlib/foundation/algebra.library.plato
//   stdlib/geometry/meshes-polygon.library.plato
//   stdlib/geometry/triangulation.library.plato
//   stdlib/geometry/splines.library.plato
//   stdlib/geometry/noise.library.plato
//   stdlib/geometry/voxels.library.plato
//
// Import this module once, before touching the generated types.
// Delete it when the writer emits array libraries (tracker: TS array-library gap).

import {
  Arr,
  JaggedArray,
  PolygonMesh3D,
  VertexIndex,
  Point2D,
  Point3D,
  Vector2D,
  Vector3D,
  Quaternion,
  Rotation2D,
  Tuple2,
  Tuple3,
  Tuple4,
  Number2,
  Number3,
  Number4,
  Number8,
  Bounds2D,
  Bounds3D,
  Triangle2D,
  Triangle3D,
  TriangleArray3D,
  TriangleFace,
  EarClipNode,
  IntegerVector2,
  IntegerVector3,
  Direction3D,
  Line3D,
  Plane,
  Sphere,
  SparseMatrixEntry,
  StrutLattice3D,
  StrutSdf3D,
  GradedStrutSdf3D,
  BodyIndex,
  SolverBody3D,
  ClothGrid3D,
  Beam,
  DofIndex,
  DofLoad,
  Length,
  CornerIndex,
  UndirectedEdgeIndex,
  VertexRemap,
  WhiteNoise2D,
  WhiteNoise3D,
  ValueNoise2D,
  ValueNoise3D,
  PerlinNoise2D,
  PerlinNoise3D,
  SimplexNoise2D,
  SimplexNoise3D,
  WorleyNoise2D,
  WorleyNoise3D,
  GaborNoise2D,
  GaborNoise3D,
  type IArray,
} from './plato.g.js';

type Any = any;

function install(name: string, fn: unknown): void {
  Object.defineProperty(Arr.prototype, name, {
    value: fn,
    writable: true,
    configurable: true,
    enumerable: false,
  });
}

function arr<T>(count: number, f: (i: number) => T): IArray<T> {
  return new Arr<T>(count, f) as unknown as IArray<T>;
}

// The eager form, for the rebuild-one-element-per-step FOLDS the simulation
// tracks are written as (`ReplacedAt` in the rigid-body solver, `WithTwoVertices`
// in the cloth sweep). Each step reads two elements of the previous array, so a
// lazy chain n steps deep costs 2^n to read one element — a hang, not a slow
// path. Materializing each step makes the fold linear in the array's length,
// which is the cost the Plato sources state.
function eager<T>(count: number, f: (i: number) => T): IArray<T> {
  const values: T[] = new Array(count);
  for (let i = 0; i < count; i++) values[i] = f(i);
  return arr(count, i => values[i]);
}

// ---- collections.library.plato — indexable basics -------------------------

install('LastIndex', function (this: Any): number {
  return this.Count() - 1;
});
install('IsValidIndex', function (this: Any, i: Any): boolean {
  const n = typeof i === 'number' ? i : i.Value;
  return n >= 0 && n < this.Count();
});
install('First', function (this: Any) {
  return this.At(0);
});
install('Last', function (this: Any) {
  return this.At(this.Count() - 1);
});
install('Middle', function (this: Any) {
  return this.At(Math.floor(this.Count() / 2));
});
install('IsEmpty', function (this: Any): boolean {
  return this.Count() === 0;
});
install('IsNotEmpty', function (this: Any): boolean {
  return this.Count() !== 0;
});
install('IsSingleton', function (this: Any): boolean {
  return this.Count() === 1;
});

// ---- slicing and concatenation -------------------------------------------

install('SubArray', function (this: Any, start: number, count: number) {
  return arr(count, i => this.At(start + i));
});
// Slice is the half-open window [from, exclusiveTo); SubArray takes a count.
install('Slice', function (this: Any, from: number, exclusiveTo: number) {
  return arr(Math.max(0, exclusiveTo - from), i => this.At(from + i));
});
install('Take', function (this: Any, count: number) {
  const n = Math.max(0, Math.min(count, this.Count()));
  return arr(n, i => this.At(i));
});
install('Skip', function (this: Any, count: number) {
  const n = Math.max(0, this.Count() - count);
  return arr(n, i => this.At(count + i));
});
install('TakeLast', function (this: Any, count: number) {
  const n = Math.max(0, Math.min(count, this.Count()));
  const start = this.Count() - n;
  return arr(n, i => this.At(start + i));
});
install('DropLast', function (this: Any, count: number) {
  const n = Math.max(0, this.Count() - count);
  return arr(n, i => this.At(i));
});
install('Concatenate', function (this: Any, other: Any) {
  const a = this.Count();
  const b = other.Count();
  return arr(a + b, i => (i < a ? this.At(i) : other.At(i - a)));
});
install('Doubled', function (this: Any) {
  return this.Concatenate(this);
});
install('Reverse', function (this: Any) {
  const n = this.Count();
  return arr(n, i => this.At(n - 1 - i));
});
install('Rotate', function (this: Any, by: number) {
  const n = this.Count();
  return arr(n, i => this.At(((i + by) % n + n) % n));
});

// ---- higher-order --------------------------------------------------------

install('FlatMap', function (this: Any, f: (x: Any) => Any) {
  // Materialized: the row lengths are not known without evaluating f.
  const rows: Any[] = [];
  for (let i = 0; i < this.Count(); i++) {
    const row = f(this.At(i));
    for (let k = 0; k < row.Count(); k++) rows.push(row.At(k));
  }
  return arr(rows.length, i => rows[i]);
});
install('Where', function (this: Any, predicate: (x: Any) => boolean) {
  const kept: Any[] = [];
  for (let i = 0; i < this.Count(); i++) {
    const x = this.At(i);
    if (predicate(x)) kept.push(x);
  }
  return arr(kept.length, i => kept[i]);
});
install('Filter', function (this: Any, predicate: (x: Any) => boolean) {
  return this.Where(predicate);
});
install('All', function (this: Any, predicate: (x: Any) => boolean): boolean {
  for (let i = 0; i < this.Count(); i++) if (!predicate(this.At(i))) return false;
  return true;
});
install('Any', function (this: Any, predicate: (x: Any) => boolean): boolean {
  for (let i = 0; i < this.Count(); i++) if (predicate(this.At(i))) return true;
  return false;
});
install('Zip', function (this: Any, other: Any, f: (a: Any, b: Any) => Any) {
  const n = Math.min(this.Count(), other.Count());
  return arr(n, i => f(this.At(i), other.At(i)));
});
install('ZipWithNext', function (this: Any, f: (a: Any, b: Any) => Any) {
  const n = Math.max(0, this.Count() - 1);
  return arr(n, i => f(this.At(i), this.At(i + 1)));
});
install('MapIndices', function (this: Any, f: (x: Any, i: number) => Any) {
  return arr(this.Count(), i => f(this.At(i), i));
});
install('IndicesWhere', function (this: Any, predicate: (x: Any) => boolean) {
  const kept: number[] = [];
  for (let i = 0; i < this.Count(); i++) if (predicate(this.At(i))) kept.push(i);
  return arr(kept.length, i => kept[i]);
});
install('CountWhere', function (this: Any, predicate: (x: Any) => boolean): number {
  let n = 0;
  for (let i = 0; i < this.Count(); i++) if (predicate(this.At(i))) n++;
  return n;
});

// ---- numeric aggregates --------------------------------------------------

install('Sum', function (this: Any): number {
  let s = 0;
  for (let i = 0; i < this.Count(); i++) s += this.At(i);
  return s;
});
install('SumOf', function (this: Any): number {
  return this.Sum();
});
install('Average', function (this: Any): number {
  return this.Count() === 0 ? 0 : this.Sum() / this.Count();
});
install('Min', function (this: Any): number {
  let m = Infinity;
  for (let i = 0; i < this.Count(); i++) m = Math.min(m, this.At(i));
  return m;
});
install('Max', function (this: Any): number {
  let m = -Infinity;
  for (let i = 0; i < this.Count(); i++) m = Math.max(m, this.At(i));
  return m;
});
install('PrefixSums', function (this: Any) {
  const sums: number[] = [0];
  for (let i = 0; i < this.Count(); i++) sums.push(sums[i] + this.At(i));
  return arr(sums.length, i => sums[i]);
});

// ---- vector / point aggregates (vectors.library.plato) --------------------

install('SumOfVectors', function (this: Any) {
  let acc = this.At(0);
  for (let i = 1; i < this.Count(); i++) acc = acc.Add(this.At(i));
  return acc;
});
install('AverageOfVectors', function (this: Any) {
  return this.SumOfVectors().Divide(this.Count());
});
install('AverageOfPoints', function (this: Any) {
  let acc = this.At(0).PositionVector();
  for (let i = 1; i < this.Count(); i++) acc = acc.Add(this.At(i).PositionVector());
  return acc.Divide(this.Count()).ToPoint();
});

// ---- collections-jagged.types.plato --------------------------------------

install('FromRows', function (this: Any) {
  const rowCount = this.Count();
  const offsets: number[] = [0];
  const values: Any[] = [];
  for (let r = 0; r < rowCount; r++) {
    const row = this.At(r);
    for (let k = 0; k < row.Count(); k++) values.push(row.At(k));
    offsets.push(values.length);
  }
  return new JaggedArray(
    arr(offsets.length, i => offsets[i]),
    arr(values.length, i => values[i]),
  );
});

// ---- meshes-polygon.library.plato ----------------------------------------

install('PolygonMeshOfFaces', function (this: Any, faces: Any) {
  return new PolygonMesh3D(this as IArray<Point3D>, faces.FromRows());
});
install('PolygonMeshOfVertexNumbers', function (this: Any, faces: Any) {
  return this.PolygonMeshOfFaces(faces.Map((face: Any) => face.Map((v: number) => new VertexIndex(v))));
});

install('Append', function (this: Any, item: Any) {
  const n = this.Count();
  return arr(n + 1, i => (i < n ? this.At(i) : item));
});
install('Prepend', function (this: Any, item: Any) {
  const n = this.Count();
  return arr(n + 1, i => (i === 0 ? item : this.At(i - 1)));
});

// ---- statics the writer does not emit ------------------------------------
//
// `Point3D.Origin()` and friends are called by generated bodies but only
// `Default` / `Create` are emitted for the struct types.

function installStatic(target: object, name: string, fn: unknown): void {
  if ((target as Any)[name] === undefined) {
    Object.defineProperty(target, name, { value: fn, writable: true, configurable: true });
  }
}

installStatic(Point2D, 'Origin', () => new Point2D(0, 0));
installStatic(Point3D, 'Origin', () => new Point3D(0, 0, 0));
installStatic(Vector2D, 'Zero', () => new Vector2D(0, 0));
installStatic(Vector3D, 'Zero', () => new Vector3D(0, 0, 0));

// ---- overload dispatch ---------------------------------------------------
//
// The writer keeps the first `Transform` overload of each type and comments the
// rest out ("Skipped: overload or duplicate member"), so a quaternion argument
// lands in the AffineTransform3D body and dereferences a missing `.Matrix`.
// Re-dispatch on the runtime argument type; the affine body stays the fallback.

function rotateByQuaternion(v: Any, q: Any): Vector3D {
  // v + 2 * cross(q.XYZ, cross(q.XYZ, v) + q.W * v) — the standard expansion of
  // q * v * conj(q), matching rotations-ops.library.plato.
  const qx = q.X, qy = q.Y, qz = q.Z, qw = q.W;
  const tx = 2 * (qy * v.Z - qz * v.Y);
  const ty = 2 * (qz * v.X - qx * v.Z);
  const tz = 2 * (qx * v.Y - qy * v.X);
  return new Vector3D(
    v.X + qw * tx + (qy * tz - qz * ty),
    v.Y + qw * ty + (qz * tx - qx * tz),
    v.Z + qw * tz + (qx * ty - qy * tx),
  );
}

const vectorTransform = Vector3D.prototype.Transform;
Object.defineProperty(Vector3D.prototype, 'Transform', {
  value: function (this: Any, t: Any) {
    if (t instanceof Quaternion) return rotateByQuaternion(this, t);
    return vectorTransform.call(this, t);
  },
  writable: true,
  configurable: true,
});

// Same defect one dimension down: `Vector2D.Transform` keeps only the
// AffineTransform2D body, so the Rotation2D that `Twist2D.Eval` passes reaches
// `TransformNormal(Matrix3x2)` and finds no `.Row1`.
function rotateByAngle(v: Any, rotation: Any): Vector2D {
  const radians = rotation.Angle.Radians ?? rotation.Angle;
  const c = Math.cos(radians);
  const s = Math.sin(radians);
  return new Vector2D(v.X * c - v.Y * s, v.X * s + v.Y * c);
}

// `ScaleX/Y/Z` call `Scale(new Number3(...))`, but the non-uniform `Scale` and
// `ScaleAbout` overloads were skipped, so the componentwise factor reaches the
// scalar body and multiplies a vector by an object — NaN, with no error. Making
// the vector's own Multiply componentwise for an N-tuple factor repairs every
// non-uniform scale at once.
function componentwise(v: Any, factor: Any, keys: string[]): Any {
  return keys.map(k => v[k] * factor[k]);
}

const vector3Multiply = Vector3D.prototype.Multiply;
Object.defineProperty(Vector3D.prototype, 'Multiply', {
  value: function (this: Any, factor: Any) {
    if (typeof factor === 'object' && factor !== null && 'Z' in factor) {
      const [x, y, z] = componentwise(this, factor, ['X', 'Y', 'Z']);
      return new Vector3D(x, y, z);
    }
    return vector3Multiply.call(this, factor);
  },
  writable: true,
  configurable: true,
});

const vector2Multiply = Vector2D.prototype.Multiply;
Object.defineProperty(Vector2D.prototype, 'Multiply', {
  value: function (this: Any, factor: Any) {
    if (typeof factor === 'object' && factor !== null && 'Y' in factor) {
      const [x, y] = componentwise(this, factor, ['X', 'Y']);
      return new Vector2D(x, y);
    }
    return vector2Multiply.call(this, factor);
  },
  writable: true,
  configurable: true,
});

// The N-tuple numeric types lose the mirror image of that overload: the
// COMPONENTWISE `Multiply`/`Divide`/`Modulo` survives and the SCALAR one is
// skipped, so `Row1.Multiply(this.X)` in `Point3D.Transform(AffineTransform3D)`
// reaches the componentwise body and reads `.X` off a number — NaN, silently,
// for every affine transform of a point or vector.
//
// numeric-structures.library.plato defines the scalar overloads as
//   Multiply(self: IVector, scalar: Number) => self.MapComponents(x => x * scalar)
// and likewise for Divide and Modulo, so dispatch on the runtime argument and
// route a bare number through `MapComponents`.

const scalarLifts: Array<[string, (x: number, s: number) => number]> = [
  ['Multiply', (x, s) => x * s],
  ['Divide', (x, s) => x / s],
  ['Modulo', (x, s) => x % s],
];

for (const numberN of [Number2, Number3, Number4, Number8]) {
  for (const [name, op] of scalarLifts) {
    const componentwise = (numberN.prototype as Any)[name];
    if (componentwise === undefined) continue;
    Object.defineProperty(numberN.prototype, name, {
      value: function (this: Any, right: Any) {
        if (typeof right === 'number') return this.MapComponents((x: number) => op(x, right));
        return componentwise.call(this, right);
      },
      writable: true,
      configurable: true,
    });
  }
}

const vector2Transform = Vector2D.prototype.Transform;
Object.defineProperty(Vector2D.prototype, 'Transform', {
  value: function (this: Any, t: Any) {
    if (t instanceof Rotation2D) return rotateByAngle(this, t);
    return vector2Transform.call(this, t);
  },
  writable: true,
  configurable: true,
});

const point2Transform = Point2D.prototype.Transform;
Object.defineProperty(Point2D.prototype, 'Transform', {
  value: function (this: Any, t: Any) {
    if (t instanceof Rotation2D) return rotateByAngle(this.PositionVector(), t).ToPoint();
    return point2Transform.call(this, t);
  },
  writable: true,
  configurable: true,
});

const pointTransform = Point3D.prototype.Transform;
Object.defineProperty(Point3D.prototype, 'Transform', {
  value: function (this: Any, t: Any) {
    if (t instanceof Quaternion) return rotateByQuaternion(this.PositionVector(), t).ToPoint();
    return pointTransform.call(this, t);
  },
  writable: true,
  configurable: true,
});

// ---- geometry.library.plato — point-array helpers -------------------------

install('AtModulo', function (this: Any, n: number) {
  const c = this.Count();
  return this.At(((n % c) + c) % c);
});

install('ChainLength', function (this: Any, closed: boolean): number {
  const n = closed ? this.Count() : this.Count() - 1;
  let total = 0;
  for (let i = 0; i < n; i++) total += this.At(i).Between(this.AtModulo(i + 1)).Magnitude();
  return total;
});

install('ShoelaceArea', function (this: Any): number {
  let sum = 0;
  for (let i = 0; i < this.Count(); i++) {
    const a = this.At(i);
    const b = this.AtModulo(i + 1);
    sum += a.X * b.Y - b.X * a.Y;
  }
  return 0.5 * sum;
});

install('PolygonAreaCentroid', function (this: Any) {
  const n = this.Count();
  const cross: number[] = [];
  for (let i = 0; i < n; i++) {
    const a = this.At(i);
    const b = this.AtModulo(i + 1);
    cross.push(a.X * b.Y - b.X * a.Y);
  }
  const twiceArea = cross.reduce((s, c) => s + c, 0);
  let cx = 0;
  let cy = 0;
  for (let i = 0; i < n; i++) {
    const a = this.At(i);
    const b = this.AtModulo(i + 1);
    cx += (a.X + b.X) * cross[i];
    cy += (a.Y + b.Y) * cross[i];
  }
  return new Point2D(cx / (3 * twiceArea), cy / (3 * twiceArea));
});

function edgeCrossesRay(a: Any, b: Any, point: Any): boolean {
  return (
    (a.Y > point.Y) !== (b.Y > point.Y) &&
    point.X < ((b.X - a.X) * (point.Y - a.Y)) / (b.Y - a.Y) + a.X
  );
}

install('PolygonContainsPoint', function (this: Any, point: Any): boolean {
  let crossings = 0;
  for (let i = 0; i < this.Count(); i++) {
    if (edgeCrossesRay(this.At(i), this.AtModulo(i + 1), point)) crossings++;
  }
  return crossings % 2 === 1;
});

install('BoundsOfPoints', function (this: Any) {
  const first = this.At(0);
  const is3D = first.Z !== undefined;
  let loX = first.X, loY = first.Y, loZ = is3D ? first.Z : 0;
  let hiX = first.X, hiY = first.Y, hiZ = is3D ? first.Z : 0;
  for (let i = 1; i < this.Count(); i++) {
    const p = this.At(i);
    loX = Math.min(loX, p.X); hiX = Math.max(hiX, p.X);
    loY = Math.min(loY, p.Y); hiY = Math.max(hiY, p.Y);
    if (is3D) { loZ = Math.min(loZ, p.Z); hiZ = Math.max(hiZ, p.Z); }
  }
  return is3D
    ? new Bounds3D(new Point3D(loX, loY, loZ), new Point3D(hiX, hiY, hiZ))
    : new Bounds2D(new Point2D(loX, loY), new Point2D(hiX, hiY));
});

install('ClosestPointOnChain', function (this: Any, closed: boolean, point: Any) {
  const n = closed ? this.Count() : this.Count() - 1;
  let best = this.At(0);
  let bestDistance = Infinity;
  for (let i = 0; i < n; i++) {
    const candidate = this.At(i).ClosestPointOnSegment(this.AtModulo(i + 1), point);
    const distance = candidate.Between(point).MagnitudeSquared();
    if (distance < bestDistance) {
      bestDistance = distance;
      best = candidate;
    }
  }
  return best;
});

// ---- polygons.library.plato — ring predicates and repair ------------------

install('RingEdgesInterfere', function (this: Any, i: number, j: number): boolean {
  const a = this.At(i);
  const b = this.AtModulo(i + 1);
  const c = this.At(j);
  const d = this.AtModulo(j + 1);
  if (j === i + 1) return a.IsOnSegment(c, d) || d.IsOnSegment(a, b);
  if (i === 0 && j === this.Count() - 1) return b.IsOnSegment(c, d) || c.IsOnSegment(a, b);
  return a.SegmentsIntersect(b, c, d);
});

install('RingSelfIntersectionCount', function (this: Any): number {
  let total = 0;
  for (let i = 0; i < this.Count(); i++) {
    for (let j = i + 1; j < this.Count(); j++) {
      if (this.RingEdgesInterfere(i, j)) total++;
    }
  }
  return total;
});

install('IsSimpleRing', function (this: Any): boolean {
  return this.Count() >= 3 && this.RingSelfIntersectionCount() === 0;
});

install('RingsAreDisjoint', function (this: Any, other: Any): boolean {
  for (let i = 0; i < this.Count(); i++) {
    for (let j = 0; j < other.Count(); j++) {
      const hit = this.At(i).SegmentsIntersect(
        this.AtModulo(i + 1),
        other.At(j),
        other.AtModulo(j + 1),
      );
      if (hit) return false;
    }
  }
  return true;
});

install('ReversedRing', function (this: Any) {
  const n = this.Count();
  return arr(n, i => this.At(n - 1 - i));
});

install('CounterClockwiseRing', function (this: Any) {
  return this.ShoelaceArea() < 0 ? this.ReversedRing() : this;
});

install('RingWithoutDuplicateVertices', function (this: Any) {
  const kept: Any[] = [];
  for (let i = 0; i < this.Count(); i++) {
    if (!this.At(i).IsSamePoint(this.AtModulo(i + 1))) kept.push(this.At(i));
  }
  return arr(kept.length, i => kept[i]);
});

install('RingWithout', function (this: Any, i: number) {
  return this.Take(i).Concatenate(this.Skip(i + 1));
});

install('FirstFlatVertex', function (this: Any): number {
  if (this.Count() < 3) return -1;
  for (let i = 0; i < this.Count(); i++) {
    const previous = this.AtModulo(i + this.Count() - 1);
    if (previous.TwiceSignedArea(this.At(i), this.AtModulo(i + 1)) === 0) return i;
  }
  return -1;
});

// One at a time, re-examining after each removal: a sweep would delete a
// repeated vertex's corner outright rather than reducing it to one vertex.
install('RingWithoutCollinearVertices', function (this: Any) {
  let result: Any = this;
  let flat = result.FirstFlatVertex();
  while (flat >= 0) {
    result = result.RingWithout(flat);
    flat = result.FirstFlatVertex();
  }
  return result;
});

// ---- polygons.library.plato — 3D polygon helpers --------------------------

install('VectorArea', function (this: Any): Vector3D {
  let acc = new Vector3D(0, 0, 0);
  for (let i = 0; i < this.Count(); i++) {
    acc = acc.Add(this.At(i).PositionVector().Cross(this.AtModulo(i + 1).PositionVector()));
  }
  return acc.Multiply(0.5);
});

install('PlanarPolygonCentroid', function (this: Any): Point3D {
  const normal = this.VectorArea().Normalize();
  const origin = this.At(0);
  const count = this.Count();
  const areas: number[] = [];
  for (let i = 0; i < count; i++) {
    areas.push(
      0.5 * origin.Between(this.At(i)).Cross(origin.Between(this.AtModulo(i + 1))).Dot(normal),
    );
  }
  const total = areas.reduce((s, a) => s + a, 0);
  let cx = 0;
  let cy = 0;
  let cz = 0;
  for (let i = 0; i < count; i++) {
    const a = this.At(i);
    const b = this.AtModulo(i + 1);
    cx += (areas[i] * (origin.X + a.X + b.X)) / 3;
    cy += (areas[i] * (origin.Y + a.Y + b.Y)) / 3;
    cz += (areas[i] * (origin.Z + a.Z + b.Z)) / 3;
  }
  return new Point3D(cx / total, cy / total, cz / total);
});

// ---- tuple-literal returns -----------------------------------------------
//
// A body whose return type is a record but whose expression is a tuple literal
// (`(false, r.Origin, 0.0)` in `Intersect(r: Ray3D, pl: Plane): PlaneHit3D`)
// is emitted as a bare `Tuple3`, so consumers reading `.Hit` / `.Point` see
// undefined. Naming the three slots on Tuple3 covers every such record.

const tuple3Aliases: Record<string, 'X0' | 'X1' | 'X2'> = {
  Hit: 'X0',
  Point: 'X1',
  Parameter: 'X2',
};

for (const [name, slot] of Object.entries(tuple3Aliases)) {
  Object.defineProperty(Tuple3.prototype, name, {
    get(this: Any) {
      return this[slot];
    },
    configurable: true,
  });
}

// The same defect in ARGUMENT position, and it is the more common one. A tuple
// literal coerced to a vector — `LatticeGradient2D(seed, i, j).Dot((dx, dy))` in
// noise.library.plato, `Matrix4x3.Zero`'s `(0.0, 0.0, 0.0)` rows in
// matrices.library.plato — is emitted as `new Tuple2(dx, dy)` / `new Tuple3(...)`
// rather than the Vector2D / Number3 the signature asks for, so the callee reads
// `.X` and gets undefined. Every consumer of a coerced literal reads the slots
// positionally, so naming the slots X/Y/Z/W covers the whole class. This is what
// made `PerlinNoise2D.Eval` and `PerlinNoise3D.Eval` return NaN.

const positionalSlots = ['X', 'Y', 'Z', 'W'] as const;

for (const tuple of [Tuple2, Tuple3, Tuple4]) {
  const width = tuple === Tuple2 ? 2 : tuple === Tuple3 ? 3 : 4;
  for (let i = 0; i < width; i++) {
    const slot = `X${i}`;
    Object.defineProperty(tuple.prototype, positionalSlots[i], {
      get(this: Any) {
        return this[slot];
      },
      configurable: true,
    });
  }
}

// ---- sum types -----------------------------------------------------------
//
// `plato.g.ts` carries "CHK320: sum type 'PlaneRelation3D' cannot be emitted to
// the TypeScript target; sum types are C#-only in v1", yet `Polygon3D.RelationTo`
// still refers to it. The reference is free, so it resolves against globalThis;
// the generated consumers only ever ask IsFront / IsBack / IsCoplanar.

class PlaneRelation {
  constructor(readonly Tag: 'Front' | 'Back' | 'Spanning' | 'Coplanar') {}
  IsFront(): boolean { return this.Tag === 'Front'; }
  IsBack(): boolean { return this.Tag === 'Back'; }
  IsSpanning(): boolean { return this.Tag === 'Spanning'; }
  IsCoplanar(): boolean { return this.Tag === 'Coplanar'; }
  toString(): string { return this.Tag; }
}

(globalThis as Any).PlaneRelation3D = {
  Front: () => new PlaneRelation('Front'),
  Back: () => new PlaneRelation('Back'),
  Spanning: () => new PlaneRelation('Spanning'),
  Coplanar: () => new PlaneRelation('Coplanar'),
};

// The same for `WindingOrder`, which `Polygon2D.Winding` returns.
class Winding {
  constructor(readonly Tag: 'Clockwise' | 'CounterClockwise') {}
  IsClockwise(): boolean { return this.Tag === 'Clockwise'; }
  IsCounterClockwise(): boolean { return this.Tag === 'CounterClockwise'; }
  toString(): string { return this.Tag; }
}

(globalThis as Any).WindingOrder = {
  Clockwise: () => new Winding('Clockwise'),
  CounterClockwise: () => new Winding('CounterClockwise'),
};

// `WorleyDistance` and `WorleyFeature` (noise.types.plato) are dropped the same
// way, and `WorleyNoise2D/3D.Eval` — which IS emitted — asks them only the
// `Is<Case>` questions, so the tag object is enough.

function sumType(name: string, cases: string[]): void {
  class Case {
    constructor(readonly Tag: string) {}
    toString(): string {
      return this.Tag;
    }
  }
  for (const c of cases) {
    (Case.prototype as Any)[`Is${c}`] = function (this: Any): boolean {
      return this.Tag === c;
    };
  }
  const factory: Any = {};
  for (const c of cases) factory[c] = () => new Case(c);
  Object.defineProperty(factory, 'Default', { get: () => new Case(cases[0]) });
  (globalThis as Any)[name] = factory;
}

sumType('WorleyDistance', ['Euclidean', 'Manhattan', 'Chebyshev', 'Minkowski']);
sumType('WorleyFeature', ['F1', 'F2', 'F2MinusF1', 'F1PlusF2']);

// `NoiseBasis` is the sum type the whole fractal family is parameterized by, and
// every function that DISPATCHES on it — `BasisValue2D/3D`, the three octave
// contributions, `WarpPoint` — has a NoiseBasis receiver, so none of them is
// emitted either and `FbmNoise2D/3D`, `TurbulenceNoise2D/3D`, `RidgedNoise2D/3D`
// and `DomainWarpNoise2D/3D` are unreachable without them. This whole block is
// the "Basis dispatch" and "Fractal sums" sections of
// stdlib/geometry/noise.library.plato.

class NoiseBasisCase {
  constructor(readonly Tag: string) {}
  toString(): string {
    return this.Tag;
  }

  // BasisValue2D(basis, seed, frequency, p) — every case remapped to a nominal
  // [-1, 1] so octaves of different bases sum sensibly.
  BasisValue2D(seed: number, frequency: number, p: Any): number {
    switch (this.Tag) {
      case 'White':
        return new WhiteNoise2D(seed).Eval(p) * 2 - 1;
      case 'Value':
        return new ValueNoise2D(seed, frequency).Eval(p);
      case 'Perlin':
        return new PerlinNoise2D(seed, frequency).Eval(p);
      case 'Simplex':
        return new SimplexNoise2D(seed, frequency).Eval(p);
      case 'Worley':
        return (
          1 -
          2 *
            new WorleyNoise2D(
              seed,
              frequency,
              1,
              (globalThis as Any).WorleyDistance.Euclidean(),
              (globalThis as Any).WorleyFeature.F1(),
            ).Eval(p)
        );
      default:
        return new GaborNoise2D(seed, frequency, (0 as Any).Turns(), 1, 0).Eval(p);
    }
  }

  BasisValue3D(seed: number, frequency: number, p: Any): number {
    switch (this.Tag) {
      case 'White':
        return new WhiteNoise3D(seed).Eval(p) * 2 - 1;
      case 'Value':
        return new ValueNoise3D(seed, frequency).Eval(p);
      case 'Perlin':
        return new PerlinNoise3D(seed, frequency).Eval(p);
      case 'Simplex':
        return new SimplexNoise3D(seed, frequency).Eval(p);
      case 'Worley':
        return (
          1 -
          2 *
            new WorleyNoise3D(
              seed,
              frequency,
              1,
              (globalThis as Any).WorleyDistance.Euclidean(),
              (globalThis as Any).WorleyFeature.F1(),
            ).Eval(p)
        );
      default:
        return new GaborNoise3D(
          seed,
          frequency,
          new Direction3D(Vector3D.UnitZ()),
          1,
          0,
        ).Eval(p);
    }
  }

  // The 2D and 3D overloads of each octave differ only in which BasisValue they
  // call; the argument tells them apart.
  private basisValue(seed: number, frequency: number, p: Any): number {
    return p instanceof Point3D
      ? this.BasisValue3D(seed, frequency, p)
      : this.BasisValue2D(seed, frequency, p);
  }

  // FbmOctave: the signed contribution of octave k.
  FbmOctave(
    seed: number,
    frequency: number,
    lacunarity: number,
    gain: number,
    p: Any,
    k: number,
  ): number {
    return (
      this.basisValue(seed + k, frequency * Math.pow(lacunarity, k), p) *
      (gain as Any).OctaveWeight(k)
    );
  }

  // TurbulenceOctave: the same sum over the absolute value of each octave.
  TurbulenceOctave(
    seed: number,
    frequency: number,
    lacunarity: number,
    gain: number,
    p: Any,
    k: number,
  ): number {
    return (
      Math.abs(this.basisValue(seed + k, frequency * Math.pow(lacunarity, k), p)) *
      (gain as Any).OctaveWeight(k)
    );
  }

  // RidgedOctave: each octave inverted about `offset` and squared.
  RidgedOctave(
    seed: number,
    frequency: number,
    lacunarity: number,
    gain: number,
    offset: number,
    p: Any,
    k: number,
  ): number {
    const signal =
      offset - Math.abs(this.basisValue(seed + k, frequency * Math.pow(lacunarity, k), p));
    return signal * signal * (gain as Any).OctaveWeight(k);
  }

  // WarpPoint: the point after `remaining` further warp iterations. The two (or
  // three) displacement components come from decorrelated seeds.
  WarpPoint(
    seed: number,
    strength: number,
    frequency: number,
    p: Any,
    remaining: number,
  ): Any {
    if (remaining <= 0) return p;
    const next =
      p instanceof Point3D
        ? new Point3D(
            p.X + strength * this.BasisValue3D(seed + 101, frequency, p),
            p.Y + strength * this.BasisValue3D(seed + 227, frequency, p),
            p.Z + strength * this.BasisValue3D(seed + 373, frequency, p),
          )
        : new Point2D(
            p.X + strength * this.BasisValue2D(seed + 101, frequency, p),
            p.Y + strength * this.BasisValue2D(seed + 227, frequency, p),
          );
    return this.WarpPoint(seed, strength, frequency, next, remaining - 1);
  }
}

const noiseBasis: Any = {};
for (const c of ['White', 'Value', 'Perlin', 'Simplex', 'Worley', 'Gabor']) {
  noiseBasis[c] = () => new NoiseBasisCase(c);
}
Object.defineProperty(noiseBasis, 'Default', { get: () => new NoiseBasisCase('Perlin') });
(globalThis as Any).NoiseBasis = noiseBasis;

// The two Array<Number> members `WorleyNoise2D/3D.Eval` reads its neighbourhood
// through (noise.library.plato, "Feature combination").
install('NearestDistance', function (this: Any): number {
  return this.Reduce(globalThis.Number.MAX_VALUE, (acc: number, d: number) => Math.min(acc, d));
});
install('SecondNearestDistance', function (this: Any, nearest: number): number {
  return this.Reduce(globalThis.Number.MAX_VALUE, (acc: number, d: number) =>
    d <= nearest ? acc : Math.min(acc, d),
  );
});

// `WorleyNeighbour2D`'s `k / 3 - 1` and `WorleyNeighbour3D`'s `n / 3 % 3 - 1`
// are Integer divisions, emitted as float division, so the 3x3 (3x3x3)
// neighbourhood offsets came out fractional and every neighbour was sampled off
// its own lattice cell. Both bodies are restated with the truncation.

installOn(WorleyNoise2D.prototype, 'WorleyNeighbour2D', function (
  this: Any,
  i: number,
  j: number,
  fx: number,
  fy: number,
  k: number,
): number {
  const dx = (k % 3) - 1;
  const dy = Math.trunc(k / 3) - 1;
  const feature = (this.Seed as Any).FeatureOffset2D(i + dx, j + dy, this.Jitter);
  const offset = new Vector2D(dx + feature.X - fx, dy + feature.Y - fy);
  return offset.WorleyLength(this.Distance);
});

installOn(WorleyNoise3D.prototype, 'WorleyNeighbour3D', function (
  this: Any,
  i: number,
  j: number,
  k: number,
  fx: number,
  fy: number,
  fz: number,
  n: number,
): number {
  const dx = (n % 3) - 1;
  const dy = (Math.trunc(n / 3) % 3) - 1;
  const dz = Math.trunc(n / 9) - 1;
  const feature = (this.Seed as Any).FeatureOffset3D(i + dx, j + dy, k + dz, this.Jitter);
  const offset = new Vector3D(
    dx + feature.X - fx,
    dy + feature.Y - fy,
    dz + feature.Z - fz,
  );
  return offset.WorleyLength(this.Distance);
});

// ---- solids-csg.library.plato --------------------------------------------

install('CutByPlane', function (this: Any, plane: Any, tolerance: number) {
  return this.FlatMap((f: Any) => f.SplitByPlane(plane, tolerance));
});

// ---- polygons.library.plato — plane basis and planar containment ----------

function installOn(proto: object, name: string, fn: unknown): void {
  Object.defineProperty(proto, name, {
    value: fn,
    writable: true,
    configurable: true,
    enumerable: false,
  });
}

installOn(Vector3D.prototype, 'PlaneTangent', function (this: Any): Vector3D {
  const seed = Math.abs(this.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0);
  return seed.Cross(this).Normalize();
});

installOn(Point3D.prototype, 'PlaneCoordinates', function (this: Any, u: Any, v: Any, p: Any) {
  return new Point2D(this.Between(p).Dot(u), this.Between(p).Dot(v));
});

install('PlanarPolygonContains', function (this: Any, normal: Any, point: Any): boolean {
  const u = normal.PlaneTangent();
  const v = normal.Normalize().Cross(u);
  const origin = this.At(0);
  const flat = this.Map((q: Any) => origin.PlaneCoordinates(u, v, q));
  return flat.PolygonContainsPoint(origin.PlaneCoordinates(u, v, point));
});

// ---- numeric identity elements -------------------------------------------
//
// `IsOdd`, `Saturate`, `OneMinus` and friends reach for `this.Zero()` /
// `this.One()` on a Number — the IArithmetic obligations, never emitted for the
// native number mapping.

installOn(globalThis.Number.prototype, 'Zero', function (): number {
  return 0;
});
installOn(globalThis.Number.prototype, 'One', function (): number {
  return 1;
});
installOn(globalThis.Number.prototype, 'Half', function (this: Any): number {
  return this / 2;
});
// `IsoperimetricQuotient` reaches for Pi in instance position on a Number.
installOn(globalThis.Number.prototype, 'Pi', function (): number {
  return Math.PI;
});

// ---- scalar on the LEFT of Multiply --------------------------------------
//
// `algebra.library.plato` closes multiplication on the other side once for every
// scalable type — `Multiply(scalar: Number, x: IScalable) => x * scalar` — with
// named specializations for Angle, the matrices and Duration. Only the intrinsic
// `Multiply(Number, Number)` survives emission, so `x * anAngle` reaches
// `this * b` with an object on the right and produces NaN with no error. That is
// what made `Hypocycloid2D.Eval` and its three relatives NaN, and what then made
// `Angle.CirclePoint` "not a function" on the bare NaN that followed.
//
// Commuting is the stdlib's own definition, so it repairs every scalable at once
// rather than Angle alone.

const numberMultiply = (globalThis.Number.prototype as Any).Multiply;
installOn(globalThis.Number.prototype, 'Multiply', function (this: Any, b: Any) {
  if (typeof b === 'object' && b !== null && typeof b.Multiply === 'function') {
    return b.Multiply(this.valueOf());
  }
  return numberMultiply.call(this, b);
});

// ---- numeric constants the writer leaves unimplemented --------------------

Object.defineProperty(Number, 'Pi', { value: () => Math.PI, writable: true, configurable: true });
Object.defineProperty(Number, 'Epsilon', {
  value: () => Number.EPSILON,
  writable: true,
  configurable: true,
});
Object.defineProperty(Number, 'MinValue', {
  value: () => -globalThis.Number.MAX_VALUE,
  writable: true,
  configurable: true,
});
Object.defineProperty(Number, 'MaxValue', {
  value: () => globalThis.Number.MAX_VALUE,
  writable: true,
  configurable: true,
});

// ---- primitives — Array3D has no runtime ----------------------------------
//
// `Array3D<T>` (primitives.types.plato) is mapped onto the `IArray3D<T>`
// INTERFACE, which carries no constructor, yet `MakeArray3D` is emitted as
// `return new IArray3D<_T0>(elements, this, rows, layers)` — a `new` on a
// type-only declaration. The identifier is free in the emitted module, so it
// resolves against globalThis and a class there supplies the missing runtime,
// as with the sum types below. Every emitted consumer reads `ColumnCount` /
// `RowCount` / `LayerCount` as FIELDS (the interface's method form is used only
// by the matrix types), so they are plain properties here.

class Array3D {
  constructor(
    readonly Elements: Any,
    readonly ColumnCount: number,
    readonly RowCount: number,
    readonly LayerCount: number,
  ) {}
  Count(): number {
    return this.Elements.Count();
  }
  // Both `At` overloads of primitives.library.plato: the flat index, and
  // (column, row, layer) in row-major order.
  At(column: number, row?: number, layer?: number): Any {
    if (row === undefined) return this.Elements.At(column);
    return this.Elements.At((layer! * this.RowCount + row) * this.ColumnCount + column);
  }
  toString(): string {
    return `{ ${this.ColumnCount} x ${this.RowCount} x ${this.LayerCount} }`;
  }
}

(globalThis as Any).IArray3D = Array3D;

// `MakeArray3D`'s own `(i / columns) % rows` and `i / plane` are Integer
// divisions emitted as float division, so the body is restated rather than left
// to build a volume whose cells are indexed between slots.
installOn(globalThis.Number.prototype, 'MakeArray3D', function (
  this: Any,
  rows: number,
  layers: number,
  f: (i: number, j: number, k: number) => Any,
) {
  const columns = this.valueOf();
  const plane = columns * rows;
  const elements = arr(plane * layers, i =>
    f(i % columns, Math.trunc(i / columns) % rows, Math.trunc(i / plane)),
  );
  return new Array3D(elements, columns, rows, layers);
});

// ---- triangulation.library.plato — the ear-clipping kernel ----------------
//
// The writer emits everything in that library whose receiver is a Number or an
// EarClipNode — FirstLiveSlot, RingLength, LinkRing, Unlink, Bridge, CornerFace
// — and nothing whose receiver is `Array<Point2D>` or `Array<Integer>`, which is
// most of it, including `TriangulateRings` itself. Every body below is that
// file's, in its order, with its parameter convention: the point pool (or the
// slot) leads and the node buffer comes second.
//
// `Buffer<T>` has no runtime in `plato.g.ts` either. It is an AFFINE type in
// Plato — a value used exactly once — so `Set` returning the same object is the
// intended semantics, not an optimization, and every emitted body above reads
// the slots it needs before it writes any.

class NodeBuffer {
  private readonly slots: Any[];
  constructor(capacity: number) {
    this.slots = new Array(capacity);
    for (let i = 0; i < capacity; i++) {
      this.slots[i] = new EarClipNode(new VertexIndex(i), i, i, false);
    }
  }
  At(i: number): Any {
    return this.slots[i];
  }
  Set(i: number, node: Any): NodeBuffer {
    this.slots[i] = node;
    return this;
  }
}

// -- Rings over a point pool (receiver: holeStarts) --

install('RingCount', function (this: Any): number {
  return this.Count() + 1;
});
install('RingStart', function (this: Any, r: number): number {
  return r === 0 ? 0 : this.At(r - 1);
});
install('RingEnd', function (this: Any, r: number, poolCount: number): number {
  return r === this.Count() ? poolCount : this.At(r);
});

// -- Rings over a point pool (receiver: points) --

install('RangeSignedArea', function (this: Any, start: number, end: number): number {
  return this.Slice(start, end).ShoelaceArea();
});

install('LeftmostOfRange', function (this: Any, start: number, end: number): Any {
  return this.Slice(start, end).Reduce(this.At(start), (best: Any, p: Any) =>
    p.IsFurtherLeft(best) ? p : best,
  );
});

// PrecedesHole's receiver is holeStarts; HolesBefore's and HolesLeftToRight's is
// the point pool.
install('PrecedesHole', function (
  this: Any,
  points: Any,
  g: number,
  h: number,
  leftmostOfH: Any,
): boolean {
  const theirs = points.LeftmostOfRange(
    this.RingStart(g + 1),
    this.RingEnd(g + 1, points.Count()),
  );
  return (
    theirs.IsFurtherLeft(leftmostOfH) || (theirs.IsSamePoint(leftmostOfH) && g < h)
  );
});

install('HolesBefore', function (this: Any, holeStarts: Any, h: number): number {
  const mine = this.LeftmostOfRange(
    holeStarts.RingStart(h + 1),
    holeStarts.RingEnd(h + 1, this.Count()),
  );
  let count = 0;
  for (let g = 0; g < holeStarts.Count(); g++) {
    if (holeStarts.PrecedesHole(this, g, h, mine)) count++;
  }
  return count;
});

install('HolesLeftToRight', function (this: Any, holeStarts: Any) {
  const points = this;
  const ranks: number[] = [];
  for (let rank = 0; rank < holeStarts.Count(); rank++) {
    let found = 0;
    for (let h = 0; h < holeStarts.Count(); h++) {
      if (points.HolesBefore(holeStarts, h) === rank) found = h;
    }
    ranks.push(found);
  }
  return arr(ranks.length, i => ranks[i]);
});

// -- Nodes and the node buffer (receiver: points) --

install('NodePoint', function (this: Any, nodes: Any, slot: number): Any {
  return this.At(nodes.At(slot).Vertex.Value);
});

install('CornerTurn', function (this: Any, nodes: Any, slot: number): number {
  return this.NodePoint(nodes, nodes.At(slot).Prev).TwiceSignedArea(
    this.NodePoint(nodes, slot),
    this.NodePoint(nodes, nodes.At(slot).Next),
  );
});

install('BlocksEar', function (
  this: Any,
  nodes: Any,
  slot: number,
  a: number,
  b: number,
  c: number,
): boolean {
  const q = this.NodePoint(nodes, slot);
  const pa = this.NodePoint(nodes, a);
  const pb = this.NodePoint(nodes, b);
  const pc = this.NodePoint(nodes, c);
  return (
    !q.IsSamePoint(pa) &&
    !q.IsSamePoint(pb) &&
    !q.IsSamePoint(pc) &&
    new Triangle2D(pa, pb, pc).Contains(q)
  );
});

install('LocallyInside', function (this: Any, nodes: Any, a: number, b: number): boolean {
  const previous = this.NodePoint(nodes, nodes.At(a).Prev);
  const here = this.NodePoint(nodes, a);
  const next = this.NodePoint(nodes, nodes.At(a).Next);
  const there = this.NodePoint(nodes, b);
  return previous.TwiceSignedArea(here, next) > 0
    ? here.TwiceSignedArea(there, next) <= 0 && here.TwiceSignedArea(previous, there) <= 0
    : here.TwiceSignedArea(there, previous) > 0 || here.TwiceSignedArea(next, there) > 0;
});

install('LeftmostSlot', function (this: Any, nodes: Any, head: number): number {
  let best = head;
  let slot = nodes.At(head).Next;
  while (slot !== head) {
    if (this.NodePoint(nodes, slot).IsFurtherLeft(this.NodePoint(nodes, best))) best = slot;
    slot = nodes.At(slot).Next;
  }
  return best;
});

// -- Ring construction and cleanup --

install('IsFlatCorner', function (this: Any, nodes: Any, slot: number): boolean {
  return (
    this.NodePoint(nodes, slot).IsSamePoint(this.NodePoint(nodes, nodes.At(slot).Next)) ||
    this.CornerTurn(nodes, slot) === 0
  );
});

install('FilterRing', function (this: Any, nodes: Any, start: number, end: number): Any {
  let result = nodes;
  let changed = true;
  while (changed) {
    changed = false;
    const head = (start as Any).FirstLiveSlot(result, end);
    let length = head < 0 ? 0 : (head as Any).RingLength(result);
    let slot = head;
    let visited = 0;
    while (visited < length) {
      const next = result.At(slot).Next;
      if (length < 3 || this.IsFlatCorner(result, slot)) {
        result = (slot as Any).Unlink(result);
        length = length - 1;
        changed = true;
      }
      slot = next;
      visited = visited + 1;
    }
  }
  return result;
});

// -- Hole bridging --

install('LeftwardHit', function (this: Any, nodes: Any, slot: number, p: Any): number {
  const a = this.NodePoint(nodes, slot);
  const b = this.NodePoint(nodes, nodes.At(slot).Next);
  const crosses = a.Y >= p.Y && p.Y >= b.Y && a.Y !== b.Y;
  const x = crosses ? a.X + ((p.Y - a.Y) * (b.X - a.X)) / (b.Y - a.Y) : p.X + 1;
  return crosses && x <= p.X ? x : -globalThis.Number.MAX_VALUE;
});

install('VisibleSlot', function (
  this: Any,
  nodes: Any,
  head: number,
  seed: number,
  holeSlot: number,
  rayX: number,
): number {
  const hole = this.NodePoint(nodes, holeSlot);
  const corner = this.NodePoint(nodes, seed);
  const wedgeNear = new Point2D(hole.Y < corner.Y ? hole.X : rayX, hole.Y);
  const wedgeFar = new Point2D(hole.Y < corner.Y ? rayX : hole.X, hole.Y);
  const wedge = new Triangle2D(wedgeNear, corner, wedgeFar);
  let best = seed;
  let bestTangent = globalThis.Number.MAX_VALUE;
  let slot = head;
  let stepped = false;
  while (!stepped || slot !== head) {
    stepped = true;
    const c = this.NodePoint(nodes, slot);
    if (hole.X >= c.X && c.X >= corner.X && hole.X !== c.X && wedge.Contains(c)) {
      const tangent = Math.abs(hole.Y - c.Y) / (hole.X - c.X);
      if (
        this.LocallyInside(nodes, slot, holeSlot) &&
        (tangent < bestTangent ||
          (tangent === bestTangent && c.X > this.NodePoint(nodes, best).X))
      ) {
        best = slot;
        bestTangent = tangent;
      }
    }
    slot = nodes.At(slot).Next;
  }
  return best;
});

install('BridgeSlot', function (this: Any, nodes: Any, head: number, holeSlot: number): number {
  const hole = this.NodePoint(nodes, holeSlot);
  let seed = -1;
  let rayX = -globalThis.Number.MAX_VALUE;
  let slot = head;
  let stepped = false;
  while (!stepped || slot !== head) {
    stepped = true;
    const next = nodes.At(slot).Next;
    const x = this.LeftwardHit(nodes, slot, hole);
    if (x > rayX) {
      rayX = x;
      seed = this.NodePoint(nodes, slot).X < this.NodePoint(nodes, next).X ? slot : next;
    }
    slot = next;
  }
  return seed < 0 ? -1 : this.VisibleSlot(nodes, head, seed, holeSlot, rayX);
});

install('BridgeHoles', function (this: Any, nodes: Any, holeStarts: Any): Any {
  let result = nodes;
  const order = this.HolesLeftToRight(holeStarts);
  let free = this.Count();
  let k = 0;
  while (k < order.Count()) {
    const hole = order.At(k);
    const holeHead = (holeStarts.RingStart(hole + 1) as Any).FirstLiveSlot(
      result,
      holeStarts.RingEnd(hole + 1, this.Count()),
    );
    if (holeHead >= 0) {
      const outerHead = (0 as Any).FirstLiveSlot(result, holeStarts.RingEnd(0, this.Count()));
      const holeSlot = this.LeftmostSlot(result, holeHead);
      const bridgeSlot = this.BridgeSlot(result, outerHead, holeSlot);
      if (bridgeSlot >= 0) {
        result = (bridgeSlot as Any).Bridge(result, holeSlot, free);
        free = free + 2;
      }
    }
    k = k + 1;
  }
  return result;
});

// -- Ear clipping --

install('HasNoBlocker', function (this: Any, nodes: Any, slot: number): boolean {
  const a = nodes.At(slot).Prev;
  const c = nodes.At(slot).Next;
  let q = nodes.At(c).Next;
  let blocked = false;
  while (q !== a && !blocked) {
    blocked = this.CornerTurn(nodes, q) <= 0 && this.BlocksEar(nodes, q, a, slot, c);
    q = nodes.At(q).Next;
  }
  return !blocked;
});

install('IsEar', function (this: Any, nodes: Any, slot: number): boolean {
  return this.CornerTurn(nodes, slot) > 0 && this.HasNoBlocker(nodes, slot);
});

install('FindEar', function (this: Any, nodes: Any, cursor: number, remaining: number): number {
  let ear = -1;
  let slot = cursor;
  let visited = 0;
  while (visited < remaining && ear < 0) {
    if (this.IsEar(nodes, slot)) ear = slot;
    slot = nodes.At(slot).Next;
    visited = visited + 1;
  }
  if (ear < 0) {
    slot = cursor;
    visited = 0;
    while (visited < remaining && ear < 0) {
      if (this.CornerTurn(nodes, slot) > 0) ear = slot;
      slot = nodes.At(slot).Next;
      visited = visited + 1;
    }
  }
  return ear;
});

install('ClipEars', function (this: Any, nodes: Any, head: number) {
  const faces: Any[] = [];
  let live = nodes;
  let remaining = head < 0 ? 0 : (head as Any).RingLength(live);
  let cursor = head;
  while (remaining > 3) {
    const ear = this.FindEar(live, cursor, remaining);
    if (ear < 0) {
      remaining = 0;
    } else {
      const next = live.At(ear).Next;
      faces.push((ear as Any).CornerFace(live));
      live = (ear as Any).Unlink(live);
      remaining = remaining - 1;
      cursor = next;
    }
  }
  if (remaining === 3) {
    const last = live.At(cursor).Next;
    if (this.CornerTurn(live, last) > 0) faces.push((last as Any).CornerFace(live));
  }
  return arr(faces.length, i => faces[i]);
});

// The kernel every triangulation entry point is written against.
install('TriangulateRings', function (this: Any, holeStarts: Any) {
  const points = this;
  const capacity = points.Count() + holeStarts.Count() * 2;
  let nodes: Any = new NodeBuffer(capacity);
  let r = 0;
  while (r < holeStarts.RingCount()) {
    const start = holeStarts.RingStart(r);
    const end = holeStarts.RingEnd(r, points.Count());
    const area = points.RangeSignedArea(start, end);
    nodes = (start as Any).LinkRing(nodes, end - start, r === 0 ? area > 0 : area < 0);
    nodes = points.FilterRing(nodes, start, end);
    r = r + 1;
  }
  nodes = points.BridgeHoles(nodes, holeStarts);
  nodes = points.FilterRing(nodes, 0, capacity);
  const head = (0 as Any).FirstLiveSlot(nodes, capacity);
  return points.ClipEars(nodes, head);
});

// The faces carried from a component's own pool into a PolygonSet2D's shared
// one — the last Array<T> member `PolygonSet2D.Triangulate` reaches for.
install('ShiftedFaces', function (this: Any, offset: number) {
  return this.Map(
    (f: Any) =>
      new TriangleFace(
        new VertexIndex(f.A.Value + offset),
        new VertexIndex(f.B.Value + offset),
        new VertexIndex(f.C.Value + offset),
      ),
  );
});

// ---- splines.library.plato — the two Array<T> members ---------------------

// DeCasteljau(xs, t): the Bezier point of arbitrary degree, by repeated
// interpolation of the control polygon. At t = 0 it is the first control point
// and at t = 1 the last, exactly.
install('DeCasteljau', function (this: Any, t: number): Any {
  if (this.Count() <= 1) return this.At(0);
  const next = arr(this.Count() - 1, (i: number) => this.At(i).Lerp(this.At(i + 1), t));
  return (next as Any).DeCasteljau(t);
});

// AtWrapped(xs, i, closed): the index wrapped on a closed sequence and clamped
// on an open one — what the Catmull-Rom and TCB splines read their neighbours
// through.
install('AtWrapped', function (this: Any, i: number, closed: boolean): Any {
  const n = this.Count();
  if (closed) return this.At(((i % n) + n) % n);
  return this.At(Math.min(Math.max(i, 0), n - 1));
});

// ---- noise.library.plato — the dropped LatticeHash overload ---------------
//
// `LatticeHash(seed, ix, iy, iz)` is the second overload of the pair and was
// skipped, so every spatial noise type reaches the PLANAR body and silently
// drops iz: WhiteNoise3D, ValueNoise3D and the gradient lattice were all
// constant along z. Dispatch on arity; the emitted planar body stays as-is.

const latticeHash2 = (globalThis.Number.prototype as Any).LatticeHash;
installOn(globalThis.Number.prototype, 'LatticeHash', function (
  this: Any,
  ix: number,
  iy: number,
  iz?: number,
): number {
  if (iz === undefined) return latticeHash2.call(this, ix, iy);
  return this.CombineHash(ix.NoiseMix())
    .CombineHash(iy.NoiseMix())
    .CombineHash(iz.NoiseMix())
    .NoiseMix();
});

// ---- voxels.library.plato — marching cubes --------------------------------
//
// The two members whose RECEIVER is the case table, `Array<Integer>`, are the
// array-library gap: everything else the kernel needs (the corner offsets, the
// configuration index, the edge point) is emitted on Number and IntegerVector3.

// MarchingCubesTriangleCount(table, c): how many triangles configuration c
// lists, found by the position of the terminating -1.
install('MarchingCubesTriangleCount', function (this: Any, c: number): number {
  const row = c * 16;
  if (this.At(row) < 0) return 0;
  if (this.At(row + 3) < 0) return 1;
  if (this.At(row + 6) < 0) return 2;
  if (this.At(row + 9) < 0) return 3;
  if (this.At(row + 12) < 0) return 4;
  return 5;
});

// MarchingCubesCell(table, cell, valueAt, pointAt, isoLevel): the triangles of
// one cube, wound counter-clockwise seen from outside the extracted region.
install('MarchingCubesCell', function (
  this: Any,
  cell: Any,
  valueAt: (i: number, j: number, k: number) => Any,
  pointAt: (i: number, j: number, k: number) => Any,
  isoLevel: number,
) {
  const table = this;
  const c = cell.MarchingCubesCase(valueAt, isoLevel);
  const row = c * 16;
  const edgePoint = (slot: number) =>
    (table.At(slot) as Any).MarchingCubesEdgePoint(cell, valueAt, pointAt, isoLevel);
  return arr(table.MarchingCubesTriangleCount(c), t =>
    new Triangle3D(
      edgePoint(row + t * 3),
      edgePoint(row + t * 3 + 1),
      edgePoint(row + t * 3 + 2),
    ),
  );
});

// ---- integer division ----------------------------------------------------
//
// Plato `Integer` division truncates; the writer emits it as `Number.Divide`,
// so `TruncateFaceOfFace`'s `k / 2` reaches `FaceCorner` as 1.5 and indexes
// between slots. Truncating the slot restores the intended semantics.

const faceCorner = PolygonMesh3D.prototype.FaceCorner;
Object.defineProperty(PolygonMesh3D.prototype, 'FaceCorner', {
  value: function (this: Any, face: number, slot: number) {
    return faceCorner.call(this, Math.trunc(face), Math.trunc(slot));
  },
  writable: true,
  configurable: true,
});

// The same defect throughout the marching-cubes kernel, where it is fatal
// rather than merely wrong: voxels.library.plato writes the corner offsets as
// `(c % 4) / 2` and `c / 4` over Integers, so corner 1 must sit at y = 0, and
// the emitted float division puts it at y = 0.5 — a sample between lattice
// nodes. Re-truncating restores the arithmetic the source means.

installOn(globalThis.Number.prototype, 'MarchingCubesCornerOffsetY', function (this: Any): number {
  return Math.trunc((this % 4) / 2);
});
installOn(globalThis.Number.prototype, 'MarchingCubesCornerOffsetZ', function (this: Any): number {
  return Math.trunc(this / 4);
});

// `MarchingCubesLattice`'s cell enumeration divides the flat cell number by the
// column count and by the plane size, both Integer divisions, so the whole body
// is re-stated here rather than patched: this is voxels.library.plato's
// `MarchingCubesLattice(nodeCounts, valueAt, pointAt, isoLevel)`, unchanged
// apart from the truncation. The case table is read once and passed to every
// cube, as the source insists.

installOn(IntegerVector3.prototype, 'MarchingCubesLattice', function (
  this: Any,
  valueAt: (i: number, j: number, k: number) => Any,
  pointAt: (i: number, j: number, k: number) => Any,
  isoLevel: number,
) {
  const table = (TriangleArray3D as Any).MarchingCubesCaseTable();
  const columns = Math.max(this.X - 1, 0);
  const rows = Math.max(this.Y - 1, 0);
  const layers = Math.max(this.Z - 1, 0);
  const plane = columns * rows;
  const cells = arr(plane * layers, n =>
    new IntegerVector3(n % columns, Math.trunc(n / columns) % rows, Math.trunc(n / plane)),
  );
  return new TriangleArray3D(
    (cells as Any).FlatMap((cell: Any) =>
      table.MarchingCubesCell(cell, valueAt, pointAt, isoLevel),
    ),
  );
});

// ===========================================================================
// The 2026-08-03 tracks — lattices, sampling, remeshing, finite elements,
// rigid bodies and cloth
// ===========================================================================
//
// `plato.g.ts` now covers four tiers rather than three (`stdlib/future` joined
// the recipe), and six new libraries landed the same day. They hit the same
// writer defects the sections above cover, plus two the earlier sweeps never
// reached: `Buffer<T>` has no runtime (defect 13), and the `Number` CONSTANTS
// other than Pi are called and nowhere defined (defect 7).
//
// Sources mirrored below:
//   stdlib/foundation/constants.library.plato
//   stdlib/foundation/matrices-ops.library.plato
//   stdlib/foundation/polynomials.library.plato
//   stdlib/geometry/meshes.library.plato
//   stdlib/geometry/lattices.library.plato
//   stdlib/geometry/sampling.library.plato
//   stdlib/future/finite-elements.library.plato
//   stdlib/future/rigid-dynamics.library.plato
//   stdlib/future/collision.library.plato

// ---- constants.library.plato — the rest of the Number statics -------------
//
// Defect 7, widened. `Number.Pi` / `Epsilon` / `MinValue` / `MaxValue` were
// filled above; the four-tier output calls fourteen more that are emitted
// nowhere — `Number.Tau()` alone is called seventeen times, and it is what
// `Angle.Turns`, `TpmsFrequency` and every periodic sampler go through. Each
// value is the body of constants.library.plato (or of the library named).
// Installed on the constructor AND the prototype, because the writer emits
// these constants in both static and instance position.

const numberConstants: Array<[string, number]> = [
  ['Tau', 2 * Math.PI],
  ['TwoPi', 2 * Math.PI],
  ['HalfPi', Math.PI / 2],
  ['E', Math.E],
  ['GoldenRatio', (1 + Math.sqrt(5)) / 2],
  ['Phi', (1 + Math.sqrt(5)) / 2],
  ['RPhi', 2 / (1 + Math.sqrt(5))],
  ['SqrtTwo', 1.4142135623730951],
  ['SqrtThree', 1.7320508075688772],
  ['SqrtFive', 2.23606797749979],
  ['Ln10', 2.302585092994046],
  ['Ln2', 0.6931471805599453],
  ['Log10E', 0.4342944819032518],
  ['RadiansPerDegree', Math.PI / 180],
  ['DegreesPerRadian', 180 / Math.PI],
  // Not from constants.library.plato, but the same shape and the same gap.
  ['TukeyFenceMultiplier', 1.5], // statistics.library.plato
  ['SdfGradientStep', 0.0001], // implicit-sdf.library.plato
  ['CsgPlaneTolerance', 0.00000001], // solids-csg.library.plato
];

for (const [name, value] of numberConstants) {
  Object.defineProperty(Number, name, { value: () => value, writable: true, configurable: true });
  installOn(globalThis.Number.prototype, name, function (): number {
    return value;
  });
}

// ---- Buffer<T> and List<T> have no runtime (defect 13) ---------------------
//
// `FilledNumbers` is emitted as `let slots = new Buffer(this)` and `Freeze()`
// is called on the result, but `plato.g.ts` never declares `Buffer`. The
// identifier is free, so under Node it resolves to the host's byte buffer —
// `new Buffer(n)` allocates bytes and `.Set` is not a function — and in the
// browser it is undefined. The class below is intrinsics.library.plato's
// affine buffer: `Set` returns the same object because a `Buffer` is used
// exactly once, and `Freeze` hands out the immutable array.
//
// Node's own `Buffer` statics are copied across so `Buffer.from` / `alloc` /
// `isBuffer` keep working for everything else in the process; only the
// single-argument `new Buffer(n)` form changes meaning, and that form has been
// deprecated in Node since v4.

class PlatoBuffer {
  private readonly slots: Any[];
  constructor(capacity: number) {
    this.slots = new Array(Math.max(0, Math.trunc(capacity)));
  }
  Count(): number {
    return this.slots.length;
  }
  // Truncating, for the same reason `Arr.At` below does: a buffer slot reached
  // through an Integer division emitted as float division is otherwise written
  // between slots and read back as undefined.
  At(i: number): Any {
    return this.slots[Math.trunc(i)];
  }
  Set(i: number, value: Any): PlatoBuffer {
    this.slots[Math.trunc(i)] = value;
    return this;
  }
  Freeze(): Any {
    const values = this.slots;
    return arr(values.length, i => values[i]);
  }
  toString(): string {
    return `Buffer(${this.slots.length})`;
  }
}

// `List<T>` is the growing sibling: Add appends, Set overwrites, Freeze seals.
class PlatoList {
  private readonly values: Any[] = [];
  Count(): number {
    return this.values.length;
  }
  At(i: number): Any {
    return this.values[i];
  }
  Add(x: Any): PlatoList {
    this.values.push(x);
    return this;
  }
  Set(i: number, x: Any): PlatoList {
    this.values[i] = x;
    return this;
  }
  Freeze(): Any {
    const values = this.values.slice();
    return arr(values.length, i => values[i]);
  }
  toString(): string {
    return `List(${this.values.length})`;
  }
}

{
  const hostBuffer = (globalThis as Any).Buffer;
  if (hostBuffer !== undefined) {
    for (const key of Object.getOwnPropertyNames(hostBuffer)) {
      if (key === 'prototype' || key === 'name' || key === 'length') continue;
      try {
        (PlatoBuffer as Any)[key] = hostBuffer[key];
      } catch {
        /* a non-writable static; nothing downstream reads it off the shim */
      }
    }
    Object.defineProperty(PlatoBuffer, Symbol.hasInstance, {
      value: (x: Any) => x instanceof PlatoBuffer || hostBuffer[Symbol.hasInstance](x),
      configurable: true,
    });
  }
  (globalThis as Any).Buffer = PlatoBuffer;
  (globalThis as Any).List = PlatoList;
}

// ---- the array surface, continued (defect 1 / plato-429) ------------------

// rigid-dynamics.library.plato: `ReplacedAt(xs, index, value)`. The one
// primitive the sequential-impulse fold is written on — `RigidWorld3D`'s three
// impulse passes each rebuild `Bodies` and `Constraints` through it, so nothing
// in the rigid-body track runs without this one line.
install('ReplacedAt', function (this: Any, index: number, value: Any) {
  return eager(this.Count(), i => (i === index ? value : this.At(i)));
});

// matrices-ops.library.plato — the two fixed-width replacements behind the
// matrix decompositions.
install('ReplaceAxis', function (this: Any, index: number, axis: Any) {
  const basis = this;
  return arr(3, i => (i === index ? axis : basis.At(i)));
});
install('ReplaceElement', function (this: Any, index: number, value: number) {
  const values = this;
  return arr(3, i => (i === index ? value : values.At(i)));
});

// polynomials.library.plato — Horner from the highest power down.
install('HornerEval', function (this: Any, x: number): number {
  let acc = 0;
  for (let i = this.Count() - 1; i >= 0; i--) acc = acc * x + this.At(i);
  return acc;
});

// meshes.library.plato — the edge census the remeshing track reads topology
// through.
install('DirectedEdges', function (this: Any) {
  return this.FlatMap((f: Any) => f.Edges());
});
install('IsFirstUndirectedOccurrence', function (this: Any, index: number): boolean {
  const e = this.At(index);
  for (let d = 0; d < index; d++) if (this.At(d).SameUndirectedEdge(e)) return false;
  return true;
});
install('UndirectedEdgeCount', function (this: Any): number {
  let total = 0;
  for (let i = 0; i < this.Count(); i++) if (this.IsFirstUndirectedOccurrence(i)) total++;
  return total;
});

// ---- finite-elements.library.plato — system vectors ------------------------
//
// A system vector is a plain `Array<Number>`, one entry per degree of freedom,
// and these five are everything the conjugate gradient in `SparseMatrix.Solve`
// asks of one. All five have an `Array<Number>` first parameter, so none is
// emitted and the solver cannot take a single step without them.
//
// THE THREE THAT RETURN A VECTOR MUST BE EAGER. Conjugate gradient rebuilds the
// iterate, the residual and the search direction on every iteration, each from
// the last, so a lazy `Zip` makes iteration k a stack of k views over stacks of
// views — and `Multiply(matrix, direction)` reads that stack once per matrix
// entry. Measured on a 60-DOF bar with the iteration cap at 10 / 20 / 40 / 80:
// 71 / 336 / 2437 / 6439 ms, which is cubic in the iteration count. Eagerly,
// the same solves are flat in it. A 10x3x3 cantilever (1224 DOF) did not finish
// in ten minutes lazily.

install('SystemDot', function (this: Any, b: Any): number {
  let total = 0;
  for (let i = 0; i < this.Count(); i++) total += this.At(i) * b.At(i);
  return total;
});
install('SystemSubtract', function (this: Any, b: Any) {
  return eager(this.Count(), (i: number) => this.At(i) - b.At(i));
});
install('SystemAddScaled', function (this: Any, b: Any, t: number) {
  return eager(this.Count(), (i: number) => this.At(i) + b.At(i) * t);
});
install('SystemProduct', function (this: Any, b: Any) {
  return eager(this.Count(), (i: number) => this.At(i) * b.At(i));
});
install('SystemNorm', function (this: Any): number {
  return Math.sqrt(this.SystemDot(this));
});

// ScatterLoads(contributions, dofCount): a list of degree-of-freedom
// contributions summed into a dense load vector. Contributions may repeat a
// degree of freedom; they add.
install('ScatterLoads', function (this: Any, dofCount: number) {
  const loads = new Array(Math.max(0, Math.trunc(dofCount))).fill(0);
  for (let k = 0; k < this.Count(); k++) {
    const c = this.At(k);
    loads[c.Dof.Value] += c.Amount;
  }
  return arr(loads.length, i => loads[i]);
});

// ConstantGradientStiffnessEntries(nodes, gradients, components, moduli,
// measure): every stiffness entry of one constant-gradient element, in global
// degree-of-freedom numbering. `rs / components` is an Integer division in the
// source (defect 4) — emitted as float division it would index the row and
// column components between slots — so the truncation is spelled out.
install('ConstantGradientStiffnessEntries', function (
  this: Any,
  gradients: Any,
  components: number,
  moduli: Any,
  measure: number,
) {
  const nodes = this;
  const n = nodes.Count();
  const entries: Any[] = [];
  for (let i = 0; i < n; i++) {
    for (let j = 0; j < n; j++) {
      for (let rs = 0; rs < components * components; rs++) {
        const r = Math.trunc(rs / components);
        const s = rs % components;
        entries.push(
          new SparseMatrixEntry(
            nodes.At(i).Value * components + r,
            nodes.At(j).Value * components + s,
            gradients.At(i).ElasticStiffnessTerm(gradients.At(j), r, s, moduli, measure),
          ),
        );
      }
    }
  }
  return arr(entries.length, k => entries[k]);
});

// DisplacementGradient(nodal, gradients, r, c): the (r, c) entry of the
// displacement gradient inside a constant-gradient element.
install('DisplacementGradient', function (this: Any, gradients: Any, r: number, c: number): number {
  let total = 0;
  for (let i = 0; i < this.Count(); i++) total += this.At(i).At(r) * gradients.At(i).At(c);
  return total;
});

// ---- sum types, the rest of them (defect 5) --------------------------------
//
// CHK320 drops every sum type in the language from this target while their
// consumers are emitted anyway, so each one is a free identifier resolved
// against globalThis. The four already installed above are the ones the noise
// and polygon tiers need — this fills in the remaining declarations of
// stdlib/{foundation,geometry,graphics,future}, so a demo can name a case
// without discovering the gap itself. `sumType` gives each case `Is<Case>()`
// and a `Default`, which is all the emitted consumers ask.

const declaredSumTypes: Array<[string, string[]]> = [
  // foundation
  ['Axis3D', ['X', 'Y', 'Z']],
  ['Axis2D', ['X', 'Y']],
  ['SignedAxis3D', ['PosX', 'NegX', 'PosY', 'NegY', 'PosZ', 'NegZ']],
  ['GraphLayout', ['ForceDirected', 'Circular', 'Layered', 'Grid', 'Radial', 'Spectral']],
  ['RotationOrder', ['XYZ', 'XZY', 'YXZ', 'YZX', 'ZXY', 'ZYX']],
  ['CorrelationStatistic', ['Pearson', 'Spearman', 'Kendall']],
  // geometry
  ['EdgeOrientation', ['Forward', 'Reversed']],
  ['SurfacePointShape', ['Elliptic', 'Parabolic', 'Hyperbolic', 'Planar', 'Umbilic']],
  ['TpmsFamily', ['Gyroid', 'SchwarzPrimitive', 'SchwarzDiamond', 'Neovius', 'IwpSurface']],
  ['AttributeDomain', ['PerVertex', 'PerFace', 'PerCorner', 'PerUndirectedEdge', 'Uniform']],
  ['LaplacianWeighting', ['UniformWeights', 'CotangentWeights']],
  ['GridBoundary', ['ClampToEdge', 'Wrap', 'Mirror', 'ZeroOutside']],
  ['Containment', ['Disjoint', 'Intersects', 'Contains', 'Within']],
  ['SubdivisionScheme', ['CatmullClark', 'Loop', 'DooSabin']],
  ['Manifoldness', ['Unknown', 'Manifold', 'ManifoldWithBoundary', 'NonManifold']],
  // future
  ['CollisionPhase', ['Begin', 'Stay', 'End']],
  ['BeamRestraint', ['Fixed', 'Pinned', 'Roller', 'Free']],
  ['PlaneCondition', ['PlaneStress', 'PlaneStrain']],
  ['BodyMotion', ['Static', 'Kinematic', 'Dynamic']],
  ['MaterialCombine', ['Average', 'Min', 'Max', 'Multiply']],
];

for (const [name, cases] of declaredSumTypes) {
  if ((globalThis as Any)[name] === undefined) sumType(name, cases);
}

// The library functions that DISPATCH on one of those, which vanish with it.
// Each is `match (x) { … }` in its source and is reached only through the case
// value, so the body lives on the case object.

function sumCaseMethod(typeName: string, method: string, fn: (tag: string, ...rest: Any[]) => Any): void {
  const factory = (globalThis as Any)[typeName];
  const sample = factory.Default;
  installOn(Object.getPrototypeOf(sample), method, function (this: Any, ...rest: Any[]) {
    return fn(this.Tag, ...rest);
  });
}

// lattices.library.plato — the three TPMS dispatchers. `TpmsField3D.Eval`,
// `TpmsNetwork3D.Eval` and `TpmsSheet3D.Eval` are all emitted and all call
// through these, so the whole TPMS half of the lattice track hangs off them.
sumCaseMethod('TpmsFamily', 'TpmsNodalValue', (tag, period: number, p: Any): number => {
  const k = 2 * Math.PI / period;
  const x = p.X * k;
  const y = p.Y * k;
  const z = p.Z * k;
  switch (tag) {
    case 'Gyroid':
      return Math.sin(x) * Math.cos(y) + Math.sin(y) * Math.cos(z) + Math.sin(z) * Math.cos(x);
    case 'SchwarzPrimitive':
      return Math.cos(x) + Math.cos(y) + Math.cos(z);
    case 'SchwarzDiamond':
      return (
        Math.sin(x) * Math.sin(y) * Math.sin(z) +
        Math.sin(x) * Math.cos(y) * Math.cos(z) +
        Math.cos(x) * Math.sin(y) * Math.cos(z) +
        Math.cos(x) * Math.cos(y) * Math.sin(z)
      );
    case 'Neovius':
      return 3 * (Math.cos(x) + Math.cos(y) + Math.cos(z)) + 4 * Math.cos(x) * Math.cos(y) * Math.cos(z);
    default:
      return (
        2 * (Math.cos(x) * Math.cos(y) + Math.cos(y) * Math.cos(z) + Math.cos(z) * Math.cos(x)) -
        (Math.cos(2 * x) + Math.cos(2 * y) + Math.cos(2 * z))
      );
  }
});
sumCaseMethod('TpmsFamily', 'TpmsPartialBound', (tag): number => {
  switch (tag) {
    case 'Gyroid':
      return 2;
    case 'SchwarzPrimitive':
      return 1;
    case 'SchwarzDiamond':
      return 4;
    case 'Neovius':
      return 7;
    default:
      return 6;
  }
});
sumCaseMethod('TpmsFamily', 'TpmsGradientBound', (tag, period: number): number => {
  const family = (globalThis as Any).TpmsFamily[tag]();
  return family.TpmsPartialBound() * Math.sqrt(3) * (2 * Math.PI / period);
});
sumCaseMethod('TpmsFamily', 'TpmsNormalizedValue', (tag, period: number, level: number, p: Any): number => {
  const family = (globalThis as Any).TpmsFamily[tag]();
  return (family.TpmsNodalValue(period, p) - level) / family.TpmsGradientBound(period);
});

// collision.library.plato — `Combine(kind, a, b)`, how two materials' friction
// and restitution merge.
sumCaseMethod('MaterialCombine', 'Combine', (tag, a: number, b: number): number => {
  switch (tag) {
    case 'Average':
      return (a + b) / 2;
    case 'Min':
      return Math.min(a, b);
    case 'Max':
      return Math.max(a, b);
    default:
      return a * b;
  }
});

// rigid-dynamics.library.plato — `MobilityScale(motion)`: 1 for a dynamic body
// and 0 for anything an impulse must not move.
sumCaseMethod('BodyMotion', 'MobilityScale', (tag): number => (tag === 'Dynamic' ? 1 : 0));

// finite-elements.library.plato — `RestrainedDofCount(r)`, the beam restraints.
sumCaseMethod('BeamRestraint', 'RestrainedDofCount', (tag): number => {
  switch (tag) {
    case 'Fixed':
      return 2;
    case 'Pinned':
      return 1;
    case 'Roller':
      return 1;
    default:
      return 0;
  }
});

// engineering.types.plato — `BeamLoad` is the one sum type in these tracks that
// CARRIES FIELDS, so the tag-only factory above will not do: each case is a
// record and `LoadContributions` reads its members. Without it `Beam.SolveBeam`
// — the whole Euler-Bernoulli path — has no way to be given a load.
{
  class BeamLoadCase {
    constructor(
      readonly Tag: string,
      readonly Position: Any,
      readonly EndPosition: Any,
      readonly Magnitude: number,
    ) {}
    get StartPosition(): Any {
      return this.Position;
    }
    IsPointForce(): boolean {
      return this.Tag === 'PointForce';
    }
    IsDistributedForce(): boolean {
      return this.Tag === 'DistributedForce';
    }
    IsMoment(): boolean {
      return this.Tag === 'Moment';
    }
    // LoadContributions(load, b, elementCount): a point force onto the
    // deflection of the nearest node, a moment onto that node's rotation, and a
    // distributed force onto the four degrees of freedom of every element it
    // covers.
    LoadContributions(b: Any, elementCount: number): Any {
      if (this.Tag === 'DistributedForce') {
        return b.BeamDistributedContributions(
          elementCount,
          this.Position,
          this.EndPosition,
          this.Magnitude,
        );
      }
      const node = b.BeamNearestNode(elementCount, this.Position);
      const slot = this.Tag === 'Moment' ? node * 2 + 1 : node * 2;
      return arr(1, () => new DofLoad(new DofIndex(slot), this.Magnitude));
    }
    toString(): string {
      return this.Tag;
    }
  }
  (globalThis as Any).BeamLoad = {
    PointForce: (position: Any, magnitude: number) =>
      new BeamLoadCase('PointForce', position, position, magnitude),
    DistributedForce: (start: Any, end: Any, magnitude: number) =>
      new BeamLoadCase('DistributedForce', start, end, magnitude),
    Moment: (position: Any, magnitude: number) =>
      new BeamLoadCase('Moment', position, position, magnitude),
    get Default() {
      return new BeamLoadCase('PointForce', new Length(0), new Length(0), 0);
    },
  };
}

// `BeamStiffnessEntries` numbers its rows `e * 2 + k / 4` over Integers, and
// the row is not a subscript — it is stored in the entry and later compared for
// `IsDiagonal` — so the truncation in `Arr.At` does not reach it and a quarter
// of every element matrix landed on a fractional row. This is the source body
// with the truncation restored.
installOn(Beam.prototype, 'BeamStiffnessEntries', function (this: Any, elementCount: number) {
  const elementLength = this.Length.Meters / elementCount;
  const element = (this.BendingStiffness() as Any).BeamElementStiffness(elementLength);
  const entries: Any[] = [];
  for (let e = 0; e < elementCount; e++) {
    for (let k = 0; k < 16; k++) {
      entries.push(
        new SparseMatrixEntry(e * 2 + Math.trunc(k / 4), e * 2 + (k % 4), element.At(k)),
      );
    }
  }
  return arr(entries.length, i => entries[i]);
});

// ---- lattices.library.plato — the strut-list operators ---------------------
//
// Every operator over a welded strut list has `Array<Line3D>` first, so the
// whole "operators over a strut list" section of the source is unemitted, and
// `StrutLattice3D.TotalStrutLength` / `RelativeDensity` / `ToSdf` — which ARE
// emitted — call into it.

install('TotalLength', function (this: Any): number {
  let total = 0;
  for (let i = 0; i < this.Count(); i++) total += this.At(i).Length();
  return total;
});

// Both RelativeDensity overloads, told apart by whether the second argument is
// a number (one radius for every strut) or an array (one radius per strut).
install('RelativeDensity', function (this: Any, radius: Any, envelope: Any): number {
  const volume =
    (envelope.Max.X - envelope.Min.X) *
    (envelope.Max.Y - envelope.Min.Y) *
    (envelope.Max.Z - envelope.Min.Z);
  if (typeof radius === 'number') {
    return (Math.PI * radius * radius * this.TotalLength()) / volume;
  }
  let solid = 0;
  for (let i = 0; i < this.Count(); i++) {
    const r = radius.At(i);
    solid += r * r * this.At(i).Length();
  }
  return (Math.PI * solid) / volume;
});

// The three StrutRadii overloads: a constant, a field read at the midpoint, and
// a grading field interpolated over a range.
install('StrutRadii', function (this: Any, radius: Any, range?: Any) {
  const struts = this;
  if (typeof radius === 'number') return struts.Map(() => radius);
  if (range === undefined) return struts.Map((s: Any) => radius.Eval(s.Centroid()));
  return struts.Map((s: Any) =>
    range.Start.Lerp(range.End, Math.min(1, Math.max(0, radius.Eval(s.Centroid())))),
  );
});

// The three Trimmed overloads are one body: every region kind answers
// `Contains(point)`, and a strut is kept or dropped whole by its midpoint.
install('Trimmed', function (this: Any, region: Any) {
  return this.FlatMap((s: Any) => (region.Contains(s.Centroid()) as Any).KeepSegment(s));
});

install('Deformed', function (this: Any, mapping: (p: Any) => Any) {
  return this.Map((s: Any) => s.Deform(mapping));
});

// ToSdf(struts, radius) and ToSdf(struts, radii) — the uniform and graded
// solids a strut list is swept into.
install('ToSdf', function (this: Any, radius: Any) {
  return typeof radius === 'number'
    ? new StrutSdf3D(this as IArray<Line3D>, radius)
    : new GradedStrutSdf3D(this as IArray<Line3D>, radius);
});

// `StrutLattice3D.Cells()` enumerates the cell grid with `n / columns % rows`
// and `n / plane` — Integer divisions, emitted as float division, so every cell
// past the first row landed at a fractional index and the tiling came out as
// noise. This is the source body with the truncation restored.
Object.defineProperty(StrutLattice3D.prototype, 'Cells', {
  value: function (this: Any) {
    const columns = Math.max(this.Counts.X, 0);
    const rows = Math.max(this.Counts.Y, 0);
    const layers = Math.max(this.Counts.Z, 0);
    const plane = columns * rows;
    return arr(
      plane * layers,
      n => new IntegerVector3(n % columns, Math.trunc(n / columns) % rows, Math.trunc(n / plane)),
    );
  },
  writable: true,
  configurable: true,
});

// ---- sampling.library.plato — Integer division (defect 4) ------------------
//
// `RadicalInverse(index, base)` is the one that must not be missed: every
// Halton, Hammersley and Sobol point is built from it, and its recursion is on
// `index / base` — an Integer division. Emitted as float division the recursion
// never terminates on an integer and the sequence is silently wrong rather
// than throwing. This is the source body with the truncation restored.
installOn(globalThis.Number.prototype, 'RadicalInverse', function (this: Any, base: number): number {
  const index = this.valueOf();
  if (index <= 0 || base < 2) return 0;
  return ((index % base) + Math.trunc(index / base).RadicalInverse(base)) / base;
});

// `JitteredCellPoint2D/3D` receive their cell indices as `n / cellCounts.X` and
// `n / (cellCounts.X * cellCounts.Y)` from `JitteredGridPoints2D/3D`, so
// truncating at the callee repairs the grid without restating the caller. The
// `.Modulo` chains truncate the same way, because trunc(a % b) == trunc(a) % b
// for a non-negative `a` and an integer `b`.
for (const [proto, name, arity] of [
  [Bounds2D.prototype, 'JitteredCellPoint2D', 2],
  [Bounds3D.prototype, 'JitteredCellPoint3D', 3],
] as Array<[Any, string, number]>) {
  const original = proto[name];
  if (original === undefined) continue;
  installOn(proto, name, function (this: Any, ...args: Any[]) {
    const fixed = args.slice();
    for (let k = args.length - arity; k < args.length; k++) fixed[k] = Math.trunc(fixed[k]);
    return original.apply(this, fixed);
  });
}

// `StratumPoint2D/3D` divide twice over: the caller hands them `n /
// samplesPerStratum` as the stratum number, and the body then splits that
// number across the strata grid with `s / strataCounts.X`. Both are Integer
// divisions, so the whole body is restated.
installOn(Bounds2D.prototype, 'StratumPoint2D', function (
  this: Any,
  strataCounts: Any,
  seed: number,
  s: number,
  k: number,
) {
  const stratum = Math.trunc(s);
  const sample = Math.trunc(k);
  const i = stratum % strataCounts.X;
  const j = Math.trunc(stratum / strataCounts.X);
  const u = (seed as Any).SampleUnit(stratum, sample * 2);
  const v = (seed as Any).SampleUnit(stratum, sample * 2 + 1);
  return new Point2D(
    this.Min.X + ((i + u) * (this.Max.X - this.Min.X)) / strataCounts.X,
    this.Min.Y + ((j + v) * (this.Max.Y - this.Min.Y)) / strataCounts.Y,
  );
});

installOn(Bounds3D.prototype, 'StratumPoint3D', function (
  this: Any,
  strataCounts: Any,
  seed: number,
  s: number,
  k: number,
) {
  const stratum = Math.trunc(s);
  const sample = Math.trunc(k);
  const i = (stratum % strataCounts.X) + (seed as Any).SampleUnit(stratum, sample * 3);
  const j =
    (Math.trunc(stratum / strataCounts.X) % strataCounts.Y) +
    (seed as Any).SampleUnit(stratum, sample * 3 + 1);
  const l =
    Math.trunc(stratum / (strataCounts.X * strataCounts.Y)) +
    (seed as Any).SampleUnit(stratum, sample * 3 + 2);
  return new Point3D(
    this.Min.X + (i * (this.Max.X - this.Min.X)) / strataCounts.X,
    this.Min.Y + (j * (this.Max.Y - this.Min.Y)) / strataCounts.Y,
    this.Min.Z + (l * (this.Max.Z - this.Min.Z)) / strataCounts.Z,
  );
});

// `PatternGrid(region, count)`'s row count is `(count + columns - 1) / columns`
// — the ceiling idiom over Integers, which float division turns into a
// fractional row count and then a fractional cell index.
const patternGrid = Bounds2D.prototype.PatternGrid;
installOn(Bounds2D.prototype, 'PatternGrid', function (this: Any, count: number) {
  const grid = patternGrid.call(this, count);
  return new IntegerVector2(grid.X, Math.trunc(grid.Y));
});

// `ThinnedPoints2D(count, points)` keeps a point when the two integer ratios
// `(i * count) / points.Count` and `((i + 1) * count) / points.Count` differ —
// which under float division they always do, so nothing was ever thinned and
// blue noise came back at the Poisson-disk count instead of the asked-for one.
installOn(globalThis.Number.prototype, 'ThinnedPoints2D', function (this: Any, points: Any) {
  const count = this.valueOf();
  const n = points.Count();
  const kept: Any[] = [];
  for (let i = 0; i < n; i++) {
    if (n <= count || Math.trunc((i * count) / n) !== Math.trunc(((i + 1) * count) / n)) {
      kept.push(points.At(i));
    }
  }
  return arr(kept.length, i => kept[i]);
});

// ---- Integer division that lands in a subscript (defect 4) -----------------
//
// The sections above restate one body each. This is the same defect met from
// the other side, and it is the common case: an emitted body computes an index
// with `n.Divide(k)` and immediately subscripts with it —
// `nodes.At(k.Divide(3))` in `FaceTraction3D.LoadContributions`,
// `TetrahedronCell.GravityContributions` and `TriangleCell.GravityContributions`
// — so the read lands between slots and returns `undefined`. A fractional array
// subscript is never meaningful, and Plato's Integer division truncates toward
// zero, so truncating in `At` restores exactly the arithmetic the source means,
// everywhere at once. `Arr` is the only `IArray` implementation in the emitted
// module, so this is the single choke point.
const arrayAt = Arr.prototype.At;
Object.defineProperty(Arr.prototype, 'At', {
  value: function (this: Any, n: number) {
    return arrayAt.call(this, Math.trunc(n));
  },
  writable: true,
  configurable: true,
});

// The same defect where the fractional index is passed on rather than
// subscripted with. `ClothGrid3D` lays its whole sheet out by splitting a flat
// number across the grid — `n.Divide(columns)`, `n.Divide(2).Divide(columns)` —
// and hands the halves to the builders below, so the sheet came out with
// duplicated springs, fractional vertex numbers and faces indexing nothing.
// Truncating the integer parameters at each builder repairs the layout without
// restating six bodies: `trunc((n / a) % b) == trunc(n / a) % b` and
// `trunc(trunc(n / a) / b) == trunc(n / (a * b))` for a non-negative `n` and
// integer divisors, which is every case here.
function truncateLeading(proto: object, name: string, count: number): void {
  const original = (proto as Any)[name];
  if (original === undefined) return;
  installOn(proto, name, function (this: Any, ...args: Any[]) {
    for (let k = 0; k < count && k < args.length; k++) args[k] = Math.trunc(args[k]);
    return original.apply(this, args);
  });
}

truncateLeading(ClothGrid3D.prototype, 'GridVertexIndex', 2);
truncateLeading(ClothGrid3D.prototype, 'GridPosition', 2);
truncateLeading(ClothGrid3D.prototype, 'GridSpring', 4);
truncateLeading(ClothGrid3D.prototype, 'GridShearSpring', 3);
truncateLeading(ClothGrid3D.prototype, 'GridQuad', 2);
truncateLeading(ClothGrid3D.prototype, 'GridTriangle', 3);
truncateLeading(ClothGrid3D.prototype, 'GridVertex', 1);

// ---- more dropped overloads (defect 3) -------------------------------------

// `Point3D.Subtract` keeps the `Vector3D` body (translate by a displacement)
// and skips `Subtract(Point3D)` (the displacement between two points), so
// `self.Position - self.PreviousPosition` in `ClothVertex.Velocity` reaches
// `delta.Negative()` on a Point and throws. `p - q` is `q.Between(p)`.
for (const pointType of [Point2D, Point3D]) {
  const subtract = (pointType.prototype as Any).Subtract;
  installOn(pointType.prototype, 'Subtract', function (this: Any, right: Any) {
    if (right instanceof pointType) return right.Between(this);
    return subtract.call(this, right);
  });
}

// `SolverBody3D.SeparationSpeed` and `DirectionalMass` each keep the five-
// parameter body (two separate anchors) and skip the four-parameter one (both
// bodies anchored at the same world point), which is the one the contact
// pipeline calls: `ConstraintRows` passes (bodies[B], point, normal) and the
// emitted body reads the normal as the second anchor and `direction` as
// undefined. Dispatch on arity; the source defines the short form as the long
// one with the point repeated.
for (const name of ['SeparationSpeed', 'DirectionalMass']) {
  const longForm = (SolverBody3D.prototype as Any)[name];
  if (longForm === undefined) continue;
  installOn(SolverBody3D.prototype, name, function (this: Any, b: Any, ...rest: Any[]) {
    if (rest.length === 2) return longForm.call(this, b, rest[0], rest[0], rest[1]);
    return longForm.apply(this, [b, ...rest]);
  });
}

// ---- collision.library.plato — the whole-world narrow phase ----------------
//
// Four array-receiver functions, and `RigidWorld3D` reaches all of them through
// `BallSceneConstraints`, which IS emitted.

// Flatten(manifolds, bodies, threshold): the rows of a set of manifolds, in
// manifold order.
install('Flatten', function (this: Any, bodies: Any, threshold: Any) {
  return this.FlatMap((m: Any) => m.ConstraintRows(bodies, threshold));
});

// WarmStartFrom(fresh, previous, tolerance): last step's accumulated impulses
// carried onto this step's matching rows; an unmatched row keeps its zeros.
install('WarmStartFrom', function (this: Any, previous: Any, tolerance: Any) {
  return this.Map((row: Any) =>
    previous.Reduce(row, (acc: Any, old: Any) =>
      acc.SameContact(old, tolerance) ? acc.WithImpulsesOf(old) : acc,
    ),
  );
});

// BallOf(bodies, radii, index): body `index` read as a ball. A SolverBody3D
// carries no shape, so a scene keeps its radii in a parallel array.
install('BallOf', function (this: Any, radii: Any, index: number) {
  const i = Math.trunc(index);
  return new Sphere(this.At(i).Center, radii.At(i));
});

// BallSceneManifolds(bodies, radii, ground, groundBody, friction, restitution).
// The ordered-pair enumeration divides the flat pair number by the body count
// twice (`k / n < k % n`), both Integer divisions, so the truncation is spelled
// out. `Collide(Sphere, Plane)` is the second overload of its group and was
// dropped (defect 3); it is re-dispatched below.
install('BallSceneManifolds', function (
  this: Any,
  radii: Any,
  ground: Any,
  groundBody: Any,
  friction: number,
  restitution: Any,
) {
  const bodies = this;
  const n = bodies.Count();
  const manifolds: Any[] = [];
  for (let k = 0; k < n * n; k++) {
    const a = Math.trunc(k / n);
    const b = k % n;
    if (a >= b || a === groundBody.Value || b === groundBody.Value) continue;
    manifolds.push(
      (new BodyIndex(a) as Any).Manifold(
        new BodyIndex(b),
        bodies.BallOf(radii, a).Collide(bodies.BallOf(radii, b)),
        friction,
        restitution,
      ),
    );
  }
  for (let k = 0; k < n; k++) {
    if (k === groundBody.Value) continue;
    manifolds.push(
      (new BodyIndex(k) as Any).Manifold(
        groundBody,
        bodies.BallOf(radii, k).Collide(ground),
        friction,
        restitution,
      ),
    );
  }
  return arr(manifolds.length, i => manifolds[i]);
});

// `Quaternion.Multiply` keeps the SCALAR body and skips the Hamilton product,
// so `q2.Multiply(q1)` runs the scalar body with a quaternion on the right,
// and the commuted-scalar shim above turns each component into a Quaternion
// rather than a number. That is what stops `IntegrateOrientation`, and with it
// every rigid-body step. The body is `Multiply(a, b)` of
// rotations-ops.library.plato.
const quaternionMultiply = (Quaternion.prototype as Any).Multiply;
installOn(Quaternion.prototype, 'Multiply', function (this: Any, b: Any) {
  if (!(b instanceof Quaternion)) return quaternionMultiply.call(this, b);
  const a = this;
  const cx = a.Y * b.Z - a.Z * b.Y;
  const cy = a.Z * b.X - a.X * b.Z;
  const cz = a.X * b.Y - a.Y * b.X;
  const dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z;
  return new Quaternion(
    a.X * b.W + b.X * a.W + cx,
    a.Y * b.W + b.Y * a.W + cy,
    a.Z * b.W + b.Z * a.W + cz,
    a.W * b.W - dot,
  );
});

// `Sphere.Collide` keeps the ball-versus-ball body and skips the other three
// (defect 3), so a plane, a box or a capsule argument runs the sphere body and
// reads `.Radius` off something that has none. Re-dispatch on the runtime
// argument; the emitted ball-versus-ball body stays the fallback.
const sphereCollide = (Sphere.prototype as Any).Collide;
installOn(Sphere.prototype, 'Collide', function (this: Any, b: Any) {
  if (b instanceof Plane) {
    // Collide(a: Sphere, b: Plane): the normal is the plane's, reversed, so it
    // runs from A toward B; the position is the foot of the centre.
    const penetration = this.Radius - b.SignedDistance(this.Center);
    return ((penetration > 0) as Any).MaybeContact(
      (b.ClosestPoint(this.Center) as Any).FreshContact(
        b.Normal.Vector.Negative().FromVectorUnchecked(),
        penetration,
      ),
    );
  }
  return sphereCollide.call(this, b);
});

// ---- remeshing.library.plato — the array-first half of every pass ----------
//
// Every derived-connectivity and merge helper in that library takes an
// `Array<…>` first, so `TopologyOf` — which every remeshing pass starts with —
// is the first thing to fail, and Loop, Butterfly, Laplacian smoothing, weld,
// split, collapse, flip, Catmull-Clark and Doo-Sabin all fail behind it. The
// bodies below are that file's, in its order.

// Corner twins and undirected-edge numbering.
install('NamesItsEdge', function (this: Any, c: number): boolean {
  const twin = this.At(c);
  return twin.IsNone() || c < twin.Value;
});

// Merging: the shared tail of welding, edge collapse and decimation.
install('CompactMerge', function (this: Any, placement: (i: number) => Any) {
  const representatives = this;
  const n = representatives.Count();
  const survives: boolean[] = [];
  for (let i = 0; i < n; i++) survives.push(representatives.At(i).Value === i);
  const rank: number[] = [];
  let seen = 0;
  for (let i = 0; i < n; i++) {
    rank.push(seen);
    if (survives[i]) seen++;
  }
  const targets = arr(n, i => new VertexIndex(rank[representatives.At(i).Value]));
  const kept: Any[] = [];
  for (let i = 0; i < n; i++) if (survives[i]) kept.push(placement(i));
  return new VertexRemap(targets, arr(kept.length, i => kept[i]));
});
// Eager, and it has to be: each pass reads the previous array TWICE per
// element (`representatives[representatives[i].Value]`), so thirty-two lazy
// passes cost 2^32 reads for one element.
install('JumpedRepresentatives', function (this: Any) {
  const representatives = this;
  return eager(representatives.Count(), i => representatives.At(representatives.At(i).Value));
});
install('ResolvedRepresentatives', function (this: Any) {
  let reps: Any = this;
  for (let pass = 0; pass < 32; pass++) reps = reps.JumpedRepresentatives();
  return reps;
});

// Welding.
install('WeldRepresentative', function (this: Any, i: number, tolerance: number) {
  const positions = this;
  let found = i;
  for (let j = i; j >= 0; j--) {
    if (positions.At(j).Distance(positions.At(i)) <= tolerance) found = j;
  }
  return new VertexIndex(found);
});
install('WeldRemap', function (this: Any, tolerance: number) {
  const positions = this;
  const raw = arr(positions.Count(), i => positions.WeldRepresentative(i, tolerance));
  return (raw as Any).ResolvedRepresentatives().CompactMerge((i: number) => positions.At(i));
});

// Edge split.
install('SplitVertexNumbers', function (this: Any, baseVertexCount: number) {
  const mask = this;
  const numbers: number[] = [];
  let rank = 0;
  for (let e = 0; e < mask.Count(); e++) {
    numbers.push(mask.At(e) ? baseVertexCount + rank : -1);
    if (mask.At(e)) rank++;
  }
  return arr(numbers.length, e => new VertexIndex(numbers[e]));
});

// Edge collapse.
install('RowContains', function (this: Any, vertex: Any): boolean {
  for (let i = 0; i < this.Count(); i++) if (this.At(i).Value === vertex.Value) return true;
  return false;
});
install('SharedNeighborCount', function (this: Any, pair: Any): number {
  const neighbors = this;
  const rowA = neighbors.At(pair.A.Value);
  const rowB = neighbors.At(pair.B.Value);
  let total = 0;
  for (let i = 0; i < rowA.Count(); i++) if (rowB.RowContains(rowA.At(i))) total++;
  return total;
});
install('HasFreeNeighborhood', function (this: Any, neighbors: Any, vertex: number): boolean {
  const state = this;
  if (state.At(vertex) !== -1) return false;
  const row = neighbors.At(vertex);
  for (let i = 0; i < row.Count(); i++) if (state.At(row.At(i).Value) !== -1) return false;
  return true;
});
install('ClaimedByCollapse', function (this: Any, neighbors: Any, edge: Any) {
  const state = this;
  const rowA = neighbors.At(edge.A.Value);
  const rowB = neighbors.At(edge.B.Value);
  return eager(state.Count(), v => {
    if (v === edge.A.Value || v === edge.B.Value) return edge.A.Value;
    const index = new VertexIndex(v);
    return rowA.RowContains(index) || rowB.RowContains(index) ? -2 : state.At(v);
  });
});
install('CollapseClaims', function (this: Any, neighbors: Any, collapseMask: Any, vertexCount: number) {
  const edges = this;
  let state: Any = eager(vertexCount, () => -1);
  for (let e = 0; e < edges.Count(); e++) {
    const edge = edges.At(e);
    if (
      collapseMask.At(e) &&
      state.HasFreeNeighborhood(neighbors, edge.A.Value) &&
      state.HasFreeNeighborhood(neighbors, edge.B.Value)
    ) {
      state = state.ClaimedByCollapse(neighbors, edge);
    }
  }
  return state;
});
install('IsAcceptedCollapse', function (this: Any, state: Any, e: number): boolean {
  const edge = this.At(e);
  return (
    edge.A.Value !== edge.B.Value &&
    state.At(edge.A.Value) === edge.A.Value &&
    state.At(edge.B.Value) === edge.A.Value
  );
});
install('CollapsedPositions', function (this: Any, edges: Any, state: Any, placement: (e: number) => Any) {
  let moved: Any = this;
  for (let e = 0; e < edges.Count(); e++) {
    if (!edges.IsAcceptedCollapse(state, e)) continue;
    const target = edges.At(e).A.Value;
    const previous = moved;
    const point = placement(e);
    moved = eager(previous.Count(), v => (v === target ? point : previous.At(v)));
  }
  return moved;
});
install('CollapseRepresentatives', function (this: Any) {
  const state = this;
  return arr(state.Count(), v => new VertexIndex(state.At(v) < 0 ? v : state.At(v)));
});
install('CrossesBoundary', function (this: Any, pair: Any): boolean {
  return this.At(pair.A.Value) !== this.At(pair.B.Value);
});

// Edge flip.
install('AreAdjacent', function (this: Any, a: Any, b: Any): boolean {
  for (let i = 0; i < this.Count(); i++) {
    const pair = this.At(i);
    if (
      (pair.A.Value === a.Value && pair.B.Value === b.Value) ||
      (pair.A.Value === b.Value && pair.B.Value === a.Value)
    ) {
      return true;
    }
  }
  return false;
});
install('ClaimAt', function (this: Any, vertex: Any): number {
  return vertex.IsNone() ? -1 : this.At(vertex.Value);
});
install('ValenceOrRegular', function (this: Any, vertex: Any): number {
  return vertex.IsNone() ? 6 : this.At(vertex.Value);
});

// Polygon-mesh connectivity, for Catmull-Clark and Doo-Sabin.
install('PolygonCornerTwins', function (this: Any, destinations: Any) {
  const vertices = this;
  const n = vertices.Count();
  return arr(n, c => {
    let found = -1;
    for (let d = 0; d < n; d++) {
      if (
        vertices.At(d).Value === destinations.At(c).Value &&
        destinations.At(d).Value === vertices.At(c).Value
      ) {
        found = d;
      }
    }
    return new CornerIndex(found);
  });
});
install('PolygonCornerEdges', function (this: Any) {
  const twins = this;
  const n = twins.Count();
  const naming: boolean[] = [];
  const rank: number[] = [];
  let seen = 0;
  for (let c = 0; c < n; c++) {
    naming.push(twins.NamesItsEdge(c));
    rank.push(seen);
    if (naming[c]) seen++;
  }
  return arr(n, c => new UndirectedEdgeIndex(naming[c] ? rank[c] : rank[twins.At(c).Value]));
});
install('PolygonEdgeCount', function (this: Any): number {
  let total = 0;
  for (let c = 0; c < this.Count(); c++) if (this.NamesItsEdge(c)) total++;
  return total;
});
install('DooSabinEdgeFace', function (this: Any, twins: Any, corner: number) {
  const nextCorners = this;
  const twin = twins.At(corner).Value;
  return arr(4, i =>
    new VertexIndex(
      i === 0 ? nextCorners.At(twin).Value : i === 1 ? twin : i === 2 ? nextCorners.At(corner).Value : corner,
    ),
  );
});

// Smoothing and decimation.
install('UniformLaplacian', function (this: Any, neighbors: Any, v: number): Vector3D {
  const positions = this;
  const row = neighbors.At(v);
  if (row.Count() === 0) return new Vector3D(0, 0, 0);
  return positions.At(v).Between(row.Map((n: Any) => positions.At(n.Value)).AverageOfPoints());
});
install('CostRank', function (this: Any, i: number): number {
  const costs = this;
  let rank = 0;
  for (let j = 0; j < costs.Count(); j++) {
    if (costs.At(j) < costs.At(i) || (costs.At(j) === costs.At(i) && j < i)) rank++;
  }
  return rank;
});

// ---- cloth.library.plato — the solver is array-first throughout ------------
//
// `Cloth3D.Step` and `StepMassSpring` are emitted, and everything they call
// takes `Array<ClothVertex>` first, so both stop at the first sweep. The
// Gauss-Seidel sequencing the source is careful about — constraint k sees the
// corrections constraints 0..k-1 made — is the left fold below, kept as a fold
// rather than turned into an in-place loop so the semantics stay the source's.

install('WithVertex', function (this: Any, index: Any, vertex: Any) {
  const self = this;
  return eager(self.Count(), i => (i === index.Value ? vertex : self.At(i)));
});
install('WithTwoVertices', function (this: Any, c: Any, a: Any, b: Any) {
  const self = this;
  return eager(self.Count(), i =>
    i === c.VertexA.Value ? a : i === c.VertexB.Value ? b : self.At(i),
  );
});
install('WithFourVertices', function (this: Any, c: Any, a: Any, b: Any, wingC: Any, wingD: Any) {
  const self = this;
  return eager(self.Count(), i =>
    i === c.VertexA.Value
      ? a
      : i === c.VertexB.Value
        ? b
        : i === c.VertexC.Value
          ? wingC
          : i === c.VertexD.Value
            ? wingD
            : self.At(i),
  );
});

install('DistanceCorrection', function (this: Any, c: Any, stiffness: number, dt: Any): Vector3D {
  const a = this.At(c.VertexA.Value);
  const b = this.At(c.VertexB.Value);
  const offset = a.Position.Subtract(b.Position);
  const error = offset.Magnitude() - c.RestLength.Meters;
  const alpha = (c.Compliance as Any).SafeDivide(dt.Seconds * dt.Seconds, 0);
  const share = a.SolverInverseMass() + b.SolverInverseMass() + alpha;
  return offset
    .NormalizeOr((Vector3D as Any).Zero())
    .Multiply(-((error * stiffness) as Any).SafeDivide(share, 0));
});
install('ProjectDistance', function (this: Any, c: Any, stiffness: number, dt: Any) {
  const vertices = this;
  const a = vertices.At(c.VertexA.Value);
  const b = vertices.At(c.VertexB.Value);
  const correction = vertices.DistanceCorrection(c, stiffness, dt);
  return vertices.WithTwoVertices(
    c,
    a.Displaced(correction.Multiply(a.SolverInverseMass())),
    b.Displaced(correction.Multiply(-b.SolverInverseMass())),
  );
});
install('SweepDistance', function (this: Any, constraints: Any, stiffness: number, dt: Any) {
  let current: Any = this;
  for (let k = 0; k < constraints.Count(); k++) {
    current = current.ProjectDistance(constraints.At(k), stiffness, dt);
  }
  return current;
});
install('SolveDistance', function (
  this: Any,
  constraints: Any,
  iterations: number,
  stiffness: number,
  dt: Any,
) {
  let current: Any = this;
  for (let i = 0; i < iterations; i++) current = current.SweepDistance(constraints, stiffness, dt);
  return current;
});

install('ProjectBend', function (this: Any, c: Any, stiffness: number) {
  const vertices = this;
  const v1 = vertices.At(c.VertexA.Value);
  const v2 = vertices.At(c.VertexB.Value);
  const v3 = vertices.At(c.VertexC.Value);
  const v4 = vertices.At(c.VertexD.Value);
  const edge = v2.Position.Subtract(v1.Position);
  const wingC = v3.Position.Subtract(v1.Position);
  const wingD = v4.Position.Subtract(v1.Position);
  const n1 = edge.BendNormal(wingC);
  const n2 = edge.BendNormal(wingD);
  const cosine = Math.min(1, Math.max(-1, n1.Dot(n2)));
  const q3 = edge.BendWingGradient(wingC, n1, n2, cosine);
  const q4 = edge.BendWingGradient(wingD, n2, n1, cosine);
  const q2 = edge
    .BendEdgeGradientTerm(wingC, n1, n2, cosine)
    .Add(edge.BendEdgeGradientTerm(wingD, n2, n1, cosine))
    .Negative();
  const q1 = q2.Add(q3).Add(q4).Negative();
  const share =
    v1.SolverInverseMass() * q1.Dot(q1) +
    v2.SolverInverseMass() * q2.Dot(q2) +
    v3.SolverInverseMass() * q3.Dot(q3) +
    v4.SolverInverseMass() * q4.Dot(q4);
  const error = Math.acos(cosine) - c.RestAngle.Radians;
  const scale = -(
    (4 * Math.sqrt(Math.max(0, 1 - cosine * cosine)) * error * stiffness) as Any
  ).SafeDivide(share, 0);
  return vertices.WithFourVertices(
    c,
    v1.Displaced(q1.Multiply(scale * v1.SolverInverseMass())),
    v2.Displaced(q2.Multiply(scale * v2.SolverInverseMass())),
    v3.Displaced(q3.Multiply(scale * v3.SolverInverseMass())),
    v4.Displaced(q4.Multiply(scale * v4.SolverInverseMass())),
  );
});
install('SweepBend', function (this: Any, constraints: Any, stiffness: number) {
  let current: Any = this;
  for (let k = 0; k < constraints.Count(); k++) {
    current = current.ProjectBend(constraints.At(k), stiffness);
  }
  return current;
});
install('SolveBend', function (this: Any, constraints: Any, iterations: number, stiffness: number) {
  let current: Any = this;
  for (let i = 0; i < iterations; i++) current = current.SweepBend(constraints, stiffness);
  return current;
});

// particles.library.plato — the one array-receiver member of that library, and
// what `ParticleSystem3D.Step` is. Eager, unlike the source's `Map`: a lazily
// mapped step chains one view per frame, so a simulation left running builds an
// unbounded stack of them and reads slow down without ever failing.
install('StepParticles', function (this: Any, forces: Any, time: number, dt: Any) {
  const particles = this;
  return eager(particles.Count(), i => {
    const p = particles.At(i);
    return p.Step(forces.AccelerationAt(p.Position, p.Velocity, time), dt);
  });
});

install('SpringForceOn', function (this: Any, c: Any, index: number, settings: Any, dt: Any): Vector3D {
  const vertices = this;
  const isEnd = c.VertexA.Value === index || c.VertexB.Value === index;
  if (!isEnd) return (Vector3D as Any).Zero();
  const near = c.VertexA.Value === index ? c.VertexA : c.VertexB;
  const far = c.VertexA.Value === index ? c.VertexB : c.VertexA;
  const offset = vertices.At(near.Value).Position.Subtract(vertices.At(far.Value).Position);
  const axis = offset.NormalizeOr((Vector3D as Any).Zero());
  const stretch = offset.Magnitude() - c.RestLength.Meters;
  const closing = vertices
    .At(near.Value)
    .Velocity(dt)
    .Subtract(vertices.At(far.Value).Velocity(dt))
    .Dot(axis);
  const magnitude =
    settings.Stiffness.NewtonsPerMeter * stretch + settings.Damping.NewtonSecondsPerMeter * closing;
  return axis.Multiply(-magnitude);
});
install('SpringAccelerationOn', function (
  this: Any,
  constraints: Any,
  index: number,
  settings: Any,
  dt: Any,
): Vector3D {
  const vertices = this;
  let total = (Vector3D as Any).Zero();
  for (let k = 0; k < constraints.Count(); k++) {
    total = total.Add(vertices.SpringForceOn(constraints.At(k), index, settings, dt));
  }
  return total.Multiply(vertices.At(index).SolverInverseMass());
});

export {};
