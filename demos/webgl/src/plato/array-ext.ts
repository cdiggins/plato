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
  IntegerVector3,
  Direction3D,
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

export {};
