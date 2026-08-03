// Hand-written prelude for the generated stdlib TypeScript.
//
// `Plato.TypeScriptWriter` emits calls to the Plato `Array<T>` library functions
// in extension-method position (`points.Concatenate(more)`,
// `positions.PolygonMeshOfFaces(faces)`) but does not emit the functions
// themselves — `IArray<T>` in `plato.g.ts` declares only At/Count/Map/Reduce.
// Scalar and Point paths (what the SDF demo exercises) are unaffected; the mesh,
// polygon and CSG paths are not reachable without these.
//
// Every body here mirrors the Plato source it is named after:
//   stdlib/foundation/collections.library.plato
//   stdlib/foundation/collections-jagged.types.plato
//   stdlib/geometry/meshes-polygon.library.plato
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
  Tuple3,
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
install('Slice', function (this: Any, start: number, count: number) {
  return arr(count, i => this.At(start + i));
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

export {};
