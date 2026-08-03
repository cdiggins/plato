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
  Rotation2D,
  Tuple3,
  Bounds2D,
  Bounds3D,
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
