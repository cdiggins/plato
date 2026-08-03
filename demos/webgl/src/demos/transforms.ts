// Transforms — a scene catalog over `stdlib/foundation/transforms.{concepts,types,library}.plato`,
// `rotations.types.plato`, `rotations-ops.library.plato`, `matrices.{types,library}.plato`,
// `matrices-ops.library.plato` and `axes.{types,library}.plato`.
//
// Every matrix, quaternion, pose and frame below is a value of one of the
// generated types in `src/plato/plato.g.ts`, and every product, inverse,
// conversion and interpolation is a generated member. This file builds inputs
// (angles, axes, subject meshes, lattices) and repacks answers into Three.js
// arrows, lines and meshes. No transform formula the stdlib defines is
// re-derived here: matrix multiplication is `Matrix4x4.Multiply`, quaternion
// interpolation is `Quaternion.Slerp`, the widening conversions are
// `AxisAngle.Quaternion` / `Quaternion.Matrix4x4` / `Matrix4x4.AffineTransform3D`.
//
// Where the emitted library cannot do it, the status line says so by name — see
// `reading()`. The TypeScript writer keeps only the FIRST overload of a member
// and comments the rest out ("Skipped: overload or duplicate member"), and this
// page is where that shows most: it is the reason `Quaternion` cannot be
// multiplied by a `Quaternion`, `Matrix4x4` cannot be multiplied by a scalar
// (hence no `Invert`), `Matrix3x3` cannot be multiplied by a `Matrix3x3` (hence
// no `Transform2D.Compose`), and `Point2D` cannot be pushed through an
// `AffineTransform2D`. Each of those is kept in the scene it belongs to, named,
// with the failure printed rather than a hand-rolled substitute.
//
// The six spatial scenes take the page's perspective camera; the planar scene
// overrides it with the orthographic, grid-free, non-spinning one.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import {
  fromArray,
  polygon2DLines,
  polygonMeshEdges,
  polygonMeshGeometry,
  polylineGeometry,
  toArray,
} from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial, type ViewerOptions } from '../shared/viewer.js';
import {
  AffineTransform2D,
  AffineTransform3D,
  Angle,
  AxisAngle,
  Basis3D,
  Direction3D,
  Frame3D,
  Matrix3x2,
  Matrix4x4,
  Number2,
  Number3,
  Point2D,
  Point3D,
  Polygon2D,
  PolygonMesh3D,
  Pose3D,
  Quaternion,
  RegularStar2D,
  Rotation2D,
  RotationAbout2D,
  RotationAbout3D,
  Transform2D,
  Translation3D,
  UniformScaling3D,
  Vector2D,
  Vector3D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same contract as `src/demos/polygons.ts`: a member that throws is a gap in the
// emitted library, not a fact about the geometry, so the status line keeps the
// member's name and the failure. A member that silently returns a non-number —
// the dropped-overload signature — gets `flag()`, which reports the value it
// actually produced instead of pretending it is a measurement.

interface Reading {
  label: string;
  value: string;
}

function reading(label: string, produce: () => string): Reading {
  try {
    return { label, value: produce() };
  } catch (error) {
    return { label, value: `UNAVAILABLE (${(error as Error).message})` };
  }
}

function note(label: string, value: string): Reading {
  return { label, value };
}

/** A reading whose whole point is that the member may hand back rubbish. */
function flag(label: string, produce: () => unknown, explain: string): Reading {
  return reading(label, () => {
    const value = produce();
    if (typeof value === 'number' && Number.isFinite(value)) return n4(value);
    return `${describe(value)} — ${explain}`;
  });
}

function describe(value: unknown): string {
  if (typeof value === 'number') return String(value);
  return `not a number (${Object.prototype.toString.call(value).slice(8, -1)})`;
}

const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const sci = (x: number): string => (x === 0 ? '0' : x.toExponential(1));
const p3s = (p: Point3D): string => `(${n2(p.X)}, ${n2(p.Y)}, ${n2(p.Z)})`;
const p2s = (p: Point2D): string => `(${n2(p.X)}, ${n2(p.Y)})`;
const v3s = (v: Vector3D): string => `(${n2(v.X)}, ${n2(v.Y)}, ${n2(v.Z)})`;
const q4s = (q: Quaternion): string => `(${n2(q.X)}, ${n2(q.Y)}, ${n2(q.Z)} | ${n2(q.W)})`;

/** The affine block of a 4x4, read out through the generated `ElementAt`. */
function affineText(m: Matrix4x4): string {
  const row = (r: number): string => `${n2(m.ElementAt(r, 0))} ${n2(m.ElementAt(r, 1))} ${n2(m.ElementAt(r, 2))}`;
  return `[${row(0)} | ${row(1)} | ${row(2)} | ${row(3)}]`;
}

/** The 3x2 the planar affine transform carries, likewise through `ElementAt`. */
function affine2Text(m: Matrix3x2): string {
  const row = (r: number): string => `${n2(m.ElementAt(r, 0))} ${n2(m.ElementAt(r, 1))}`;
  return `[${row(0)} | ${row(1)} | ${row(2)}]`;
}

/** How far a 4x4 is from the identity, entry by entry. */
function identityResidual(m: Matrix4x4): number {
  let worst = 0;
  for (let r = 0; r < 4; r++) {
    for (let c = 0; c < 4; c++) {
      worst = Math.max(worst, Math.abs(m.ElementAt(r, c) - (r === c ? 1 : 0)));
    }
  }
  return worst;
}

/** The largest gap between two point lists, measured with `Point3D.Distance`. */
function maxSeparation(a: readonly Point3D[], b: readonly Point3D[]): number {
  let worst = 0;
  for (let i = 0; i < Math.min(a.length, b.length); i++) worst = Math.max(worst, a[i].Distance(b[i]));
  return worst;
}

interface Built {
  object: THREE.Object3D;
  readings: Reading[];
}

function sceneOf(spec: {
  id: string;
  title: string;
  description: string;
  plato: string[];
  controls?: Control[];
  viewer?: ViewerOptions;
  fit?: boolean;
  build(params: Params): Built;
}): Scene {
  let latest: Reading[] = [];
  return {
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    controls: spec.controls,
    viewer: spec.viewer,
    build(params: Params): THREE.Object3D {
      const built = spec.build(params);
      latest = built.readings;
      return spec.fit ? fitToView(built.object) : built.object;
    },
    status(): string {
      return latest.map(r => `${r.label} ${r.value}`).join('  ·  ');
    },
  };
}

/**
 * The shell's orthographic camera is sized by height alone, so the planar scene
 * would clip sideways on a narrow stage. Scale to whichever half-extent is
 * smaller, refreshed per frame because the stage is resizable.
 */
function fitToView(object: THREE.Object3D): THREE.Object3D {
  const box = new THREE.Box3().setFromObject(object);
  const extent = Math.max(
    Math.abs(box.min.x),
    Math.abs(box.max.x),
    Math.abs(box.min.y),
    Math.abs(box.max.y),
  );
  if (!Number.isFinite(extent) || extent <= 0) return object;

  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute([0, 0, 0], 3));
  const probe = new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ size: 0, transparent: true, opacity: 0, depthWrite: false }),
  );
  probe.onBeforeRender = (_renderer, _scene, camera): void => {
    const ortho = camera as THREE.OrthographicCamera;
    if (!ortho.isOrthographicCamera) return;
    const half = Math.min((ortho.top - ortho.bottom) / 2, (ortho.right - ortho.left) / 2);
    object.scale.setScalar(Math.min(1, (half * 0.96) / extent));
  };
  object.add(probe);
  return object;
}

// ---------------------------------------------------------------------------
// Presentation helpers

/** X, Y, Z, in that order, wherever a frame's axes are drawn. */
const AXIS_COLORS = [0xe05a5a, 0x63d6a8, 0x5aa2e0];

function segments(coordinates: number[], color: number): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color }));
}

function lines(geometry: THREE.BufferGeometry, color: number): THREE.LineSegments {
  return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color }));
}

function arrow(direction: Vector3D, origin: Point3D, length: number, color: number): THREE.Object3D {
  const d = new THREE.Vector3(direction.X, direction.Y, direction.Z);
  if (!Number.isFinite(d.lengthSq()) || d.lengthSq() === 0) return new THREE.Group();
  return new THREE.ArrowHelper(
    d.normalize(),
    new THREE.Vector3(origin.X, origin.Y, origin.Z),
    length,
    color,
    length * 0.26,
    length * 0.16,
  );
}

/** The three axes of a `Frame3D` (or a `Basis3D` planted at a point). */
function axisTriad(origin: Point3D, axes: readonly Vector3D[], length: number, dim = false): THREE.Group {
  const group = new THREE.Group();
  for (let i = 0; i < axes.length; i++) {
    const color = dim ? new THREE.Color(AXIS_COLORS[i]).multiplyScalar(0.45).getHex() : AXIS_COLORS[i];
    group.add(arrow(axes[i], origin, length, color));
  }
  return group;
}

/** A generated polygon mesh as flat-shaded faces plus its real edges. */
function meshObject(mesh: PolygonMesh3D, color: number, opacity = 1): THREE.Group {
  const group = new THREE.Group();
  const material = surfaceMaterial(color);
  if (opacity < 1) {
    material.transparent = true;
    material.opacity = opacity;
    material.depthWrite = false;
  }
  group.add(new THREE.Mesh(polygonMeshGeometry(mesh), material));
  group.add(new THREE.LineSegments(polygonMeshEdges(mesh), edgeMaterial(0x0d1117)));
  return group;
}

/** Edges only, for the "before" copy that should not fight the subject. */
function ghostMesh(mesh: PolygonMesh3D, color: number): THREE.LineSegments {
  return new THREE.LineSegments(polygonMeshEdges(mesh), new THREE.LineBasicMaterial({ color }));
}

function markers3D(points: readonly Point3D[], color: number, size = 6): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

function markers2D(points: readonly Point2D[], color: number, size = 5, z = 0): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

// ---------------------------------------------------------------------------
// Input construction
//
// Angles arrive from the sliders in degrees and axes in spherical coordinates
// because those are the readable controls; turning them into an `Angle` and a
// unit `Vector3D` is demo work. Everything downstream of here is generated.

const RAD = Math.PI / 180;
const radians = (degrees: number): Angle => new Angle(degrees * RAD);

/**
 * `Point3D.Transform` is emitted with its `AffineTransform3D` overload only —
 * every other one is commented out as "Skipped: overload or duplicate member" —
 * so the quaternion body comes from the prelude's runtime re-dispatch while the
 * declared signature still says affine. This is that seam, named once rather
 * than cast at each call site. The member being called is still the library's.
 */
function turn(point: Point3D, rotation: Quaternion): Point3D {
  return point.Transform(rotation as unknown as AffineTransform3D);
}

/** A unit vector from a polar/azimuth pair — always unit, so `Direction3D` is safe. */
function unitAxis(tiltDegrees: number, spinDegrees: number): Vector3D {
  const t = tiltDegrees * RAD;
  const s = spinDegrees * RAD;
  return new Vector3D(Math.sin(t) * Math.cos(s), Math.cos(t), Math.sin(t) * Math.sin(s));
}

/** The world axes, drawn dim at the origin under every spatial scene. */
function worldTriad(length = 1.1): THREE.Group {
  return axisTriad(
    new Point3D(0, 0, 0),
    [new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1)],
    length,
    true,
  );
}

/**
 * The subject the transform scenes push through the library. Small on purpose:
 * `build` runs on every slider tick, and every scene here transforms the subject
 * two or three times over. A dodecahedron is 20 vertices; an octahedron is 6.
 */
const subjects = {
  octahedron: (): PolygonMesh3D => PolygonMesh3D.Octahedron(),
  dodecahedron: (): PolygonMesh3D => PolygonMesh3D.Dodecahedron(),
  icosahedron: (): PolygonMesh3D => PolygonMesh3D.Icosahedron(),
};

const SPATIAL: ViewerOptions = { orthographic: false, grid: false, spin: false, distance: 6 };
const PLANAR: ViewerOptions = { orthographic: true, grid: false, spin: false };

// ---------------------------------------------------------------------------
// Scene: the TRS triple
// ---------------------------------------------------------------------------

const trs = sceneOf({
  id: 'trs',
  title: 'Translate, rotate, scale',
  description:
    'Three elementary 4x4s from matrices.library.plato — CreateTranslation, CreateRotationZ and ' +
    'CreateScale — composed with Matrix4x4.Multiply and applied to an octahedron. Plato matrices are ' +
    'row-vector (p′ = p·M), so A.Multiply(B) means "A first, then B", and the two orders below are ' +
    'genuinely different maps: the wire copy is the other order, and the status line reports how far ' +
    'apart they put the same vertex.',
  viewer: SPATIAL,
  plato: [
    'Matrix4x4.CreateTranslation',
    'Matrix4x4.CreateRotationZ',
    'Matrix4x4.CreateScale',
    'Matrix4x4.Multiply',
    'Matrix4x4.ElementAt',
    'Matrix4x4.Determinant',
    'Matrix4x4.Trace',
    'Matrix4x4.CanInvert',
    'Matrix4x4.AffineTransform3D',
    'PolygonMesh3D.Deform',
    'Point3D.Transform',
  ],
  controls: [
    { key: 'shift', label: 'Translate X', kind: 'slider', min: -2, max: 2, step: 0.01, def: 1.2 },
    { key: 'lift', label: 'Translate Y', kind: 'slider', min: -2, max: 2, step: 0.01, def: 0.4 },
    { key: 'angle', label: 'Rotate Z (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 55 },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0.2, max: 2, step: 0.01, def: 0.65 },
    { key: 'order', label: 'Order', kind: 'select', options: ['T→R→S', 'S→R→T'], def: 0 },
    { key: 'both', label: 'Draw both orders', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const T = Matrix4x4.CreateTranslation(new Vector3D(params.shift, params.lift, 0));
    const R = Matrix4x4.CreateRotationZ(radians(params.angle));
    const S = Matrix4x4.CreateScale(params.scale);

    // Application order left to right, which is also the product order because
    // the convention is row-vector.
    const forward = T.Multiply(R).Multiply(S);
    const reversed = S.Multiply(R).Multiply(T);
    const chosen = params.order < 0.5 ? forward : reversed;
    const other = params.order < 0.5 ? reversed : forward;

    const subject = subjects.octahedron();
    const push = (m: Matrix4x4): PolygonMesh3D => {
      const affine = m.AffineTransform3D();
      return subject.Deform(p => p.Transform(affine));
    };
    const chosenMesh = push(chosen);
    const otherMesh = push(other);

    const object = new THREE.Group();
    object.add(worldTriad());
    object.add(ghostMesh(subject, 0x3d4a5e));
    object.add(meshObject(chosenMesh, palette.surface));
    if (params.both) object.add(ghostMesh(otherMesh, palette.surfaceAlt));

    // The composed map's own frame: its three basis rows planted at its origin.
    const origin = new Point3D(chosen.ElementAt(3, 0), chosen.ElementAt(3, 1), chosen.ElementAt(3, 2));
    const basis = [0, 1, 2].map(
      r => new Vector3D(chosen.ElementAt(r, 0), chosen.ElementAt(r, 1), chosen.ElementAt(r, 2)),
    );
    object.add(axisTriad(origin, basis, 0.9));

    const here = toArray(chosenMesh.Positions);
    const there = toArray(otherMesh.Positions);

    return {
      object,
      readings: [
        note('order', params.order < 0.5 ? 'T→R→S' : 'S→R→T'),
        reading('composed affine block', () => affineText(chosen)),
        reading('Determinant', () => n4(chosen.Determinant())),
        reading('Trace', () => n4(chosen.Trace())),
        reading('CanInvert', () => String(chosen.CanInvert())),
        note('max vertex gap between the two orders', n4(maxSeparation(here, there))),
        reading('translation row', () => p3s(origin)),
        flag(
          'Matrix4x4.CreateScale(Number3)[0,0]',
          () => (Matrix4x4.CreateScale as unknown as (s: unknown) => Matrix4x4)(
            new Number3(params.scale, 1, 1),
          ).ElementAt(0, 0),
          'the Number3 overload was dropped, so the componentwise factor reached the uniform body',
        ),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: one rotation, four representations
// ---------------------------------------------------------------------------

const representations = sceneOf({
  id: 'representations',
  title: 'One rotation, four representations',
  description:
    'The same rotation authored as an AxisAngle and widened through the conversions in ' +
    'rotations-ops.library.plato: AxisAngle.Quaternion, Quaternion.Matrix4x4, then back with ' +
    'Matrix4x4.CreateFromRotationMatrix and Quaternion.AxisAngle. The coloured triad is ' +
    'Quaternion.Basis3D; the solid is turned by the matrix and the wire copy by the quaternion, so ' +
    'they coincide exactly. The round-trip error is in the status line.',
  viewer: SPATIAL,
  plato: [
    'AxisAngle.Quaternion',
    'AxisAngle.Matrix4x4',
    'Quaternion.Matrix4x4',
    'Quaternion.AxisAngle',
    'Quaternion.Basis3D',
    'Quaternion.EulerAngles',
    'Quaternion.Concatenate',
    'Quaternion.Length',
    'Matrix4x4.CreateFromRotationMatrix',
    'Matrix4x4.Determinant',
    'PolygonMesh3D.Transform',
    'PolygonMesh3D.Deform',
  ],
  controls: [
    { key: 'angle', label: 'Angle (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 75 },
    { key: 'tilt', label: 'Axis tilt (deg)', kind: 'slider', min: 0, max: 180, step: 1, def: 55 },
    { key: 'spin', label: 'Axis azimuth (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 35 },
  ],
  build(params: Params): Built {
    const axis = unitAxis(params.tilt, params.spin);
    const angle = radians(params.angle);
    const axisAngle = new AxisAngle(new Direction3D(axis), angle);

    const quaternion = axisAngle.Quaternion();
    const matrix = quaternion.Matrix4x4();
    const recoveredQuaternion = Quaternion.CreateFromRotationMatrix(matrix);
    const recoveredAxisAngle = recoveredQuaternion.AxisAngle();
    const basis: Basis3D = quaternion.Basis3D();

    const subject = subjects.icosahedron();
    const affine = matrix.AffineTransform3D();
    const viaMatrix = subject.Deform(p => p.Transform(affine));
    const viaQuaternion = subject.Transform(quaternion);

    const object = new THREE.Group();
    object.add(worldTriad(1.4));
    object.add(meshObject(viaMatrix, palette.surface, 0.85));
    object.add(ghostMesh(viaQuaternion, palette.accent));
    object.add(axisTriad(new Point3D(0, 0, 0), [basis.X, basis.Y, basis.Z], 1.9));
    // The axis itself, through the origin, as the line every representation fixes.
    object.add(
      segments(
        [-axis.X * 2.4, -axis.Y * 2.4, -axis.Z * 2.4, axis.X * 2.4, axis.Y * 2.4, axis.Z * 2.4],
        palette.surfaceAlt,
      ),
    );

    const quaternionGap = Math.max(
      Math.abs(quaternion.X - recoveredQuaternion.X),
      Math.abs(quaternion.Y - recoveredQuaternion.Y),
      Math.abs(quaternion.Z - recoveredQuaternion.Z),
      Math.abs(quaternion.W - recoveredQuaternion.W),
    );

    return {
      object,
      readings: [
        reading('AxisAngle', () => `${v3s(axisAngle.Axis.Vector)} @ ${n2(axisAngle.Angle.Degrees())}°`),
        reading('Quaternion', () => q4s(quaternion)),
        reading('Quaternion.Length', () => n4(quaternion.Length())),
        reading('Matrix4x4 affine block', () => affineText(matrix)),
        reading('Matrix4x4.Determinant', () => n4(matrix.Determinant())),
        reading('Matrix4x4.Trace', () => n4(matrix.Trace())),
        note('quaternion round-trip error', sci(quaternionGap)),
        reading('AxisAngle round-trip angle', () =>
          sci(Math.abs(Math.abs(recoveredAxisAngle.Angle.Radians) - Math.abs(angle.Radians))),
        ),
        note(
          'matrix image vs quaternion image',
          sci(maxSeparation(toArray(viaMatrix.Positions), toArray(viaQuaternion.Positions))),
        ),
        reading('Quaternion.EulerAngles', () => `${n2(quaternion.EulerAngles().Yaw.Degrees())}° yaw`),
        flag(
          'Quaternion.Concatenate',
          () => quaternion.Concatenate(Quaternion.CreateRotationX(radians(30))).W,
          'the Quaternion×Quaternion overload of Multiply was dropped, so the scalar body ran',
        ),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: interpolating between two orientations
// ---------------------------------------------------------------------------

const interpolation = sceneOf({
  id: 'interpolation',
  title: 'Slerp, Lerp and yaw/pitch/roll',
  description:
    'Two orientations built with Quaternion.CreateFromYawPitchRoll and blended three ways: ' +
    'Quaternion.Slerp (constant angular rate), Quaternion.Lerp (the normalised straight chord), and ' +
    'a blend of the yaw/pitch/roll numbers pushed back through CreateFromYawPitchRoll. Each curve is ' +
    'the path a fixed marker point traces under the blend — the Euler path leaves the great circle ' +
    'the other two share, which is exactly the difference worth seeing.',
  viewer: SPATIAL,
  plato: [
    'Quaternion.CreateFromYawPitchRoll',
    'Quaternion.Slerp',
    'Quaternion.Lerp',
    'Quaternion.Dot',
    'Quaternion.LinearSpace',
    'Point3D.Transform',
    'Point3D.Distance',
    'PolygonMesh3D.Transform',
  ],
  controls: [
    { key: 't', label: 'Blend t', kind: 'slider', min: 0, max: 1, step: 0.005, def: 0.35 },
    { key: 'yaw', label: 'End yaw (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 155 },
    { key: 'pitch', label: 'End pitch (deg)', kind: 'slider', min: -89, max: 89, step: 1, def: -60 },
    { key: 'steps', label: 'Path samples', kind: 'slider', min: 6, max: 48, step: 1, def: 28 },
  ],
  build(params: Params): Built {
    const steps = Math.round(params.steps);
    const t = params.t;
    // The endpoints are authored in yaw/pitch/roll so that the Euler blend has
    // numbers to interpolate: `Quaternion.EulerAngles()` cannot recover them
    // (see the representations scene), so the demo keeps them rather than
    // extracting them.
    const startYpr = [0, 0, 0];
    const endYpr = [params.yaw, params.pitch, 0];
    const yprAt = (u: number): number[] => startYpr.map((a, i) => a + (endYpr[i] - a) * u);
    const fromYpr = (ypr: number[]): Quaternion =>
      Quaternion.CreateFromYawPitchRoll(radians(ypr[0]), radians(ypr[1]), radians(ypr[2]));

    const a = fromYpr(startYpr);
    const b = fromYpr(endYpr);
    const marker = new Point3D(1.6, 0.55, 0);

    const slerpPath: Point3D[] = [];
    const lerpPath: Point3D[] = [];
    const eulerPath: Point3D[] = [];
    for (let i = 0; i <= steps; i++) {
      const u = i / steps;
      slerpPath.push(turn(marker, a.Slerp(b, u)));
      lerpPath.push(turn(marker, a.Lerp(b, u)));
      eulerPath.push(turn(marker, fromYpr(yprAt(u))));
    }

    const atSlerp = a.Slerp(b, t);
    const atLerp = a.Lerp(b, t);
    const atEuler = fromYpr(yprAt(t));

    const object = new THREE.Group();
    object.add(worldTriad(1.2));
    object.add(lines(polylineGeometry(slerpPath), palette.line));
    object.add(lines(polylineGeometry(lerpPath), palette.accent));
    object.add(lines(polylineGeometry(eulerPath), palette.surfaceAlt));
    object.add(
      markers3D([turn(marker, atSlerp)], palette.line, 10),
      markers3D([turn(marker, atLerp)], palette.accent, 10),
      markers3D([turn(marker, atEuler)], palette.surfaceAlt, 10),
    );
    object.add(meshObject(subjects.octahedron().Transform(atSlerp), palette.surface, 0.9));
    object.add(
      axisTriad(new Point3D(0, 0, 0), [atSlerp.Basis3D().X, atSlerp.Basis3D().Y, atSlerp.Basis3D().Z], 1.0),
    );

    const pathLength = (points: readonly Point3D[]): number => {
      let total = 0;
      for (let i = 0; i + 1 < points.length; i++) total += points[i].Distance(points[i + 1]);
      return total;
    };

    return {
      object,
      readings: [
        note('t', n2(t)),
        reading('Slerp', () => q4s(atSlerp)),
        reading('Lerp', () => q4s(atLerp)),
        reading('yaw/pitch/roll blend', () => q4s(atEuler)),
        reading('Quaternion.Dot(endpoints)', () => n4(a.Dot(b))),
        note('Slerp vs Lerp marker gap', n4(turn(marker, atSlerp).Distance(turn(marker, atLerp)))),
        note('Slerp vs Euler marker gap', n4(turn(marker, atSlerp).Distance(turn(marker, atEuler)))),
        note('path lengths (slerp / lerp / euler)',
          `${n4(pathLength(slerpPath))} / ${n4(pathLength(lerpPath))} / ${n4(pathLength(eulerPath))}`),
        reading('Quaternion.LinearSpace', () => `${a.LinearSpace(b, 8).Count()} samples`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: composition and inverse
// ---------------------------------------------------------------------------

const composeInverse = sceneOf({
  id: 'compose-inverse',
  title: 'Compose and invert',
  description:
    'A chain of three elementary transforms — Translation3D, RotationAbout3D and UniformScaling3D — ' +
    'each widened with AffineTransform3D() and joined with AffineTransform3D.Compose. The undo chain ' +
    'is each piece’s own Inverse() composed in the opposite order — the general ' +
    'AffineTransform3D.Inverse is not reachable in the emitted library, and the status line says so. ' +
    'Running the subject forward and then back returns it, and the residual is reported both as a ' +
    'matrix distance from Identity and as a vertex distance. Drop the scale to zero and CanInvert ' +
    'says no.',
  viewer: SPATIAL,
  plato: [
    'Translation3D.AffineTransform3D',
    'Translation3D.Inverse',
    'RotationAbout3D.AffineTransform3D',
    'RotationAbout3D.Inverse',
    'UniformScaling3D.AffineTransform3D',
    'UniformScaling3D.Inverse',
    'AffineTransform3D.Compose',
    'AffineTransform3D.CanInvert',
    'AffineTransform3D.Inverse',
    'AffineTransform3D.Identity',
    'AffineTransform3D.Matrix4x4',
    'Matrix4x4.Invert',
    'Matrix4x4.CanInvert',
    'PolygonMesh3D.Deform',
  ],
  controls: [
    { key: 'shift', label: 'Translate X', kind: 'slider', min: -2, max: 2, step: 0.01, def: 1.3 },
    { key: 'angle', label: 'Rotate (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 50 },
    { key: 'pivot', label: 'Rotation centre X', kind: 'slider', min: -2, max: 2, step: 0.01, def: 0.8 },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0, max: 1.8, step: 0.01, def: 0.8 },
  ],
  build(params: Params): Built {
    const translation = new Translation3D(new Vector3D(params.shift, 0.35, 0));
    const rotation = new RotationAbout3D(
      Quaternion.CreateRotationZ(radians(params.angle)),
      new Point3D(params.pivot, 0, 0),
    );
    const scaling = new UniformScaling3D(params.scale);

    const forward = translation
      .AffineTransform3D()
      .Compose(rotation.AffineTransform3D())
      .Compose(scaling.AffineTransform3D());
    // Each piece's own analytic inverse, composed the other way round.
    const backward = scaling
      .Inverse()
      .AffineTransform3D()
      .Compose(rotation.Inverse().AffineTransform3D())
      .Compose(translation.Inverse().AffineTransform3D());
    const roundTrip = forward.Compose(backward);
    const invertible = forward.CanInvert();

    const subject = subjects.dodecahedron();
    const moved = subject.Deform(p => p.Transform(forward));
    const recovered = moved.Deform(p => p.Transform(backward));

    const object = new THREE.Group();
    object.add(worldTriad());
    object.add(ghostMesh(subject, 0x3d4a5e));
    object.add(meshObject(moved, palette.surface, 0.9));
    // A collapsed scale sends the undo chain to infinity; drawing it would poison
    // the bounding box, so the scene shows only what survives.
    if (invertible) object.add(ghostMesh(recovered, palette.accent));
    object.add(markers3D([new Point3D(params.pivot, 0, 0)], palette.surfaceAlt, 9));

    const before = toArray(subject.Positions);
    const after = toArray(recovered.Positions);

    return {
      object,
      readings: [
        reading('forward affine block', () => affineText(forward.Matrix4x4())),
        reading('Determinant', () => n4(forward.Matrix4x4().Determinant())),
        reading('AffineTransform3D.CanInvert', () => String(invertible)),
        reading('Matrix4x4.CanInvert', () => String(forward.Matrix4x4().CanInvert())),
        reading('Compose(forward, backward) vs Identity', () => sci(identityResidual(roundTrip.Matrix4x4()))),
        note('vertex residual after the round trip', invertible ? sci(maxSeparation(before, after)) : 'n/a'),
        reading('AffineTransform3D.Identity', () =>
          sci(identityResidual(AffineTransform3D.Identity().Matrix4x4())),
        ),
        reading('AffineTransform3D.Inverse', () => affineText(forward.Inverse().Matrix4x4())),
        // `Invert` short-circuits to `Matrix4x4.NotANumber()` when `CanInvert`
        // is false, so the degenerate branch answers and the healthy one throws.
        reading('Matrix4x4.Invert', () => affineText(forward.Matrix4x4().Invert())),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: a transform applied to real geometry
// ---------------------------------------------------------------------------

const REPRESENTATIONS = ['Quaternion', 'AxisAngle', 'Pose3D', 'Matrix4x4', 'RotAbout', 'Basis3D', 'TRS'];

const applied = sceneOf({
  id: 'applied',
  title: 'Every representation, one polyhedron',
  description:
    'PolygonMesh3D implements IDeformable3D, so the library gives it two ways to carry a transform: ' +
    'the emitted PolygonMesh3D.Transform, whose surviving overload takes a Quaternion, and ' +
    'PolygonMesh3D.Deform(mapping), which takes any Point3D mapping and is how an AffineTransform3D ' +
    'reaches a mesh. Pick a representation: each converts up to the same affine map, the image should ' +
    'not move, and the status line reports the deviation from the quaternion reference.',
  viewer: SPATIAL,
  plato: [
    'PolygonMesh3D.Transform',
    'PolygonMesh3D.Deform',
    'PolygonMesh3D.RotateAbout',
    'PolygonMesh3D.Translate',
    'PolygonMesh3D.ScaleAbout',
    'PolygonMesh3D.TransformedByFrame',
    'Quaternion.Matrix4x4',
    'AxisAngle.AffineTransform3D',
    'Pose3D.AffineTransform3D',
    'Matrix4x4.AffineTransform3D',
    'RotationAbout3D.AffineTransform3D',
    'Basis3D.AffineTransform3D',
    'TrsTransform3D.AffineTransform3D',
  ],
  controls: [
    { key: 'kind', label: 'Representation', kind: 'select', options: REPRESENTATIONS, def: 0 },
    { key: 'angle', label: 'Angle (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 65 },
    { key: 'tilt', label: 'Axis tilt (deg)', kind: 'slider', min: 0, max: 180, step: 1, def: 60 },
  ],
  build(params: Params): Built {
    const kind = Math.round(params.kind);
    const angle = radians(params.angle);
    const axis = unitAxis(params.tilt, 25);
    const quaternion = Quaternion.CreateFromAxisAngle(axis, angle);

    // Every representation of the SAME rotation, each widened by its own member.
    const routes: { label: string; affine: () => AffineTransform3D }[] = [
      { label: 'Quaternion.Matrix4x4', affine: () => quaternion.Matrix4x4().AffineTransform3D() },
      {
        label: 'AxisAngle.AffineTransform3D',
        affine: () => new AxisAngle(new Direction3D(axis), angle).AffineTransform3D(),
      },
      { label: 'Pose3D.AffineTransform3D', affine: () => quaternion.Pose3D().AffineTransform3D() },
      {
        label: 'Matrix4x4.CreateFromAxisAngle',
        affine: () => Matrix4x4.CreateFromAxisAngle(axis, angle).AffineTransform3D(),
      },
      {
        label: 'RotationAbout3D.AffineTransform3D',
        affine: () => new RotationAbout3D(quaternion, new Point3D(0, 0, 0)).AffineTransform3D(),
      },
      { label: 'Basis3D.AffineTransform3D', affine: () => quaternion.Basis3D().AffineTransform3D() },
      {
        label: 'TrsTransform3D.AffineTransform3D',
        affine: () => quaternion.Pose3D().TrsTransform3D().AffineTransform3D(),
      },
    ];
    const route = routes[Math.min(kind, routes.length - 1)];

    const subject = subjects.dodecahedron();
    // The reference image: `PolygonMesh3D.Transform` takes a Quaternion, the one
    // overload the writer kept, and derives itself from Deform.
    const reference = subject.Transform(quaternion);

    const object = new THREE.Group();
    object.add(worldTriad(1.4));
    object.add(ghostMesh(subject, 0x3d4a5e));

    let deviation: Reading;
    let selected: Reading;
    try {
      const affine = route.affine();
      const image = subject.Deform(p => p.Transform(affine));
      object.add(meshObject(image, palette.surface, 0.92));
      object.add(axisTriad(new Point3D(0, 0, 0), [
        new Vector3D(1, 0, 0).Transform(affine),
        new Vector3D(0, 1, 0).Transform(affine),
        new Vector3D(0, 0, 1).Transform(affine),
      ], 1.9));
      selected = note('via', route.label);
      const gap = maxSeparation(toArray(reference.Positions), toArray(image.Positions));
      deviation = note(
        'deviation from PolygonMesh3D.Transform(Quaternion)',
        Number.isFinite(gap) ? sci(gap) : `NOT FINITE (${gap})`,
      );
    } catch (error) {
      // Keep the reference image on screen so the scene still shows something,
      // and name the conversion that failed.
      object.add(meshObject(reference, palette.surfaceAlt, 0.92));
      selected = note('via', `${route.label} — UNAVAILABLE (${(error as Error).message})`);
      deviation = note('deviation', 'n/a — the quaternion reference is drawn instead');
    }

    return {
      object,
      readings: [
        selected,
        deviation,
        note('subject', `Dodecahedron, ${subject.VertexCount()} vertices / ${subject.FaceCount()} faces`),
        reading('PolygonMesh3D.Transform(Quaternion)', () => p3s(reference.Positions.At(0))),
        reading('PolygonMesh3D.Deform', () =>
          p3s(subject.Deform(p => p.Transform(quaternion.Matrix4x4().AffineTransform3D())).Positions.At(0)),
        ),
        reading('PolygonMesh3D.RotateAbout', () =>
          p3s(subject.RotateAbout(new Point3D(0.5, 0, 0), quaternion).Positions.At(0)),
        ),
        reading('PolygonMesh3D.Translate', () => p3s(subject.Translate(new Vector3D(1, 0, 0)).Positions.At(0))),
        reading('PolygonMesh3D.ScaleAbout', () =>
          p3s(subject.ScaleAbout(new Point3D(0, 0, 0), 0.5).Positions.At(0)),
        ),
        reading('PolygonMesh3D.TransformedByFrame', () =>
          p3s(subject.TransformedByFrame(quaternion.Pose3D().Frame3D()).Positions.At(0)),
        ),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: frames and bases
// ---------------------------------------------------------------------------

const frames = sceneOf({
  id: 'frames',
  title: 'Frames and bases',
  description:
    'A Pose3D — position plus orientation — expanded into a Frame3D by Pose3D.Frame3D. The coloured ' +
    'triad is the frame’s own X/Y/Z axes; the dim one at the origin is the world’s. The marker sits ' +
    'at fixed coordinates IN the frame and is placed in the world by Frame3D.FramePoint, then carried ' +
    'back by Pose3D.Inverse, so the status line can show the same point read both ways and the residual ' +
    'between them.',
  viewer: SPATIAL,
  plato: [
    'Pose3D.Frame3D',
    'Pose3D.Inverse',
    'Pose3D.Matrix4x4',
    'Pose3D.AffineTransform3D',
    'Pose3D.Compose',
    'Frame3D.FramePoint',
    'Frame3D.PointInFrame',
    'Frame3D.Matrix4x4',
    'Frame3D.AffineTransform3D',
    'Frame3D.FramePlane',
    'Frame3D.Pose3D',
    'Basis3D.AffineTransform3D',
    'Quaternion.Basis3D',
    'PolygonMesh3D.TransformedByFrame',
  ],
  controls: [
    { key: 'shift', label: 'Origin X', kind: 'slider', min: -2, max: 2, step: 0.01, def: 1.1 },
    { key: 'lift', label: 'Origin Y', kind: 'slider', min: -2, max: 2, step: 0.01, def: 0.6 },
    { key: 'yaw', label: 'Yaw (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 40 },
    { key: 'pitch', label: 'Pitch (deg)', kind: 'slider', min: -89, max: 89, step: 1, def: 25 },
    { key: 'u', label: 'Local X', kind: 'slider', min: -1.5, max: 1.5, step: 0.01, def: 0.9 },
    { key: 'v', label: 'Local Y', kind: 'slider', min: -1.5, max: 1.5, step: 0.01, def: 0.7 },
  ],
  build(params: Params): Built {
    const orientation = Quaternion.CreateFromYawPitchRoll(radians(params.yaw), radians(params.pitch), new Angle(0));
    const pose = new Pose3D(new Point3D(params.shift, params.lift, 0), orientation);
    const frame: Frame3D = pose.Frame3D();

    const local = new Point3D(params.u, params.v, 0.45);
    const world = frame.FramePoint(local.X, local.Y, local.Z);
    // Back the other way: the pose's own inverse, widened to an affine map.
    const back = pose.Inverse().AffineTransform3D();
    const recovered = world.Transform(back);

    const object = new THREE.Group();
    object.add(worldTriad(1.2));
    object.add(axisTriad(frame.Origin, [frame.XAxis.Vector, frame.YAxis.Vector, frame.ZAxis.Vector], 1.3));
    // The subject rides the frame, so the triad is attached to something solid.
    object.add(meshObject(subjects.octahedron().ScaleAbout(new Point3D(0, 0, 0), 0.35).TransformedByFrame(frame),
      palette.surface, 0.9));
    object.add(markers3D([world], palette.surfaceAlt, 11));
    object.add(
      segments([frame.Origin.X, frame.Origin.Y, frame.Origin.Z, world.X, world.Y, world.Z], palette.surfaceAlt),
    );
    object.add(segments([0, 0, 0, world.X, world.Y, world.Z], 0x3d4a5e));

    const basis: Basis3D = orientation.Basis3D();

    return {
      object,
      readings: [
        reading('Frame3D.Origin', () => p3s(frame.Origin)),
        reading('Frame3D.XAxis', () => v3s(frame.XAxis.Vector)),
        reading('Frame3D.ZAxis', () => v3s(frame.ZAxis.Vector)),
        note('point in the frame', p3s(local)),
        reading('Frame3D.FramePoint (world)', () => p3s(world)),
        reading('Frame3D.PointInFrame', () => p3s(frame.PointInFrame(local))),
        reading('Pose3D.Inverse round trip', () => `${p3s(recovered)}  residual ${sci(recovered.Distance(local))}`),
        reading('Frame3D.FramePlane', () =>
          `n ${v3s(frame.FramePlane().Normal.Vector)} d ${n2(frame.FramePlane().Distance)}`,
        ),
        reading('Frame3D.Matrix4x4', () => affineText(frame.Matrix4x4())),
        reading('Basis3D.AffineTransform3D', () => affineText(basis.AffineTransform3D().Matrix4x4())),
        reading('Frame3D.Pose3D', () => p3s(frame.Pose3D().Position)),
        reading('Pose3D.Compose', () => p3s(pose.Compose(Pose3D.Identity()).Position)),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene: the plane
// ---------------------------------------------------------------------------

const RADIUS = 1.15;

/** A square lattice of `Point2D`, the input the planar transforms move. */
function lattice(count: number, half: number): Point2D[] {
  const points: Point2D[] = [];
  for (let j = 0; j < count; j++) {
    for (let i = 0; i < count; i++) {
      points.push(new Point2D(
        -half + (2 * half * i) / (count - 1),
        -half + (2 * half * j) / (count - 1),
      ));
    }
  }
  return points;
}

const planar = sceneOf({
  id: 'planar',
  title: 'The plane',
  description:
    'The planar half of the catalog: Matrix3x2 built by CreateTranslation / CreateRotation / CreateScale, ' +
    'joined with AffineTransform2D.Compose and lifted to a Matrix3x3 by AffineTransform2D.Transform2D. ' +
    'The blue ring and lattice are moved by the named transforms Polygon2D.Transform(Rotation2D), ' +
    'Polygon2D.ScaleAbout and Polygon2D.Translate applied in order; the orange copy is the same ' +
    'composed affine map’s linear part carried by Vector2D.Transform, which is translation-free by ' +
    'design; and the green copy is that affine map pushed straight through Polygon2D.Deform, which ' +
    'appears only when Point2D.Transform(AffineTransform2D) evaluates. The status line reports the ' +
    'matrix, the inverse residual, and the members that are not there.',
  viewer: PLANAR,
  fit: true,
  plato: [
    'Matrix3x2.CreateTranslation',
    'Matrix3x2.CreateRotation',
    'Matrix3x2.CreateScale',
    'Matrix3x2.Multiply',
    'Matrix3x2.Determinant',
    'Matrix3x2.Invert',
    'Matrix3x2.Pose2D',
    'AffineTransform2D.Compose',
    'AffineTransform2D.Inverse',
    'AffineTransform2D.CanInvert',
    'AffineTransform2D.Transform2D',
    'Transform2D.Compose',
    'Rotation2D.Matrix3x2',
    'Pose2D.Frame2D',
    'Polygon2D.Transform',
    'Polygon2D.ScaleAbout',
    'Polygon2D.Translate',
    'Point2D.Transform',
    'Vector2D.Transform',
  ],
  controls: [
    { key: 'angle', label: 'Rotate (deg)', kind: 'slider', min: -180, max: 180, step: 1, def: 35 },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0.15, max: 2, step: 0.01, def: 0.7 },
    { key: 'shift', label: 'Translate X', kind: 'slider', min: -1.5, max: 1.5, step: 0.01, def: 0.7 },
    { key: 'grid', label: 'Lattice', kind: 'slider', min: 2, max: 9, step: 1, def: 5 },
  ],
  build(params: Params): Built {
    const rotation = new Rotation2D(radians(params.angle));
    const offset = new Vector2D(params.shift, params.shift * 0.45);

    // The matrix side: three elementary 3x2s widened and composed.
    const R = new AffineTransform2D(rotation.Matrix3x2());
    const S = new AffineTransform2D(Matrix3x2.CreateScale(params.scale));
    const T = new AffineTransform2D(Matrix3x2.CreateTranslation(offset));
    const affine = R.Compose(S).Compose(T);
    const lifted: Transform2D = affine.Transform2D();

    // The geometry side: the same rotate/scale/translate through Polygon2D's own
    // named transforms, which are derived from Polygon2D.Deform.
    const source = new Polygon2D(
      new RegularStar2D(new Point2D(0, 0), RADIUS, RADIUS * 0.45, 6, new Angle(0)).RegularStarVertices(),
    );
    const moved = source
      .Transform(rotation)
      .ScaleAbout(new Point2D(0, 0), params.scale)
      .Translate(offset);

    const count = Math.round(params.grid);
    const dots = lattice(count, RADIUS);
    // The affine map's linear part, which is what `Vector2D.Transform` carries.
    const linear = dots.map(p => p.PositionVector().Transform(affine).ToPoint());

    const object = new THREE.Group();
    object.add(lines(polygon2DLines(source, -0.02), 0x3d4a5e));
    object.add(markers2D(dots, 0x3d4a5e, 4, -0.02));
    object.add(lines(polygon2DLines(moved, 0.01), palette.line));
    const linearRing = new Polygon2D(
      fromArray(toArray(source.Points).map(p => p.PositionVector().Transform(affine).ToPoint())),
    );
    object.add(markers2D(linear, palette.surfaceAlt, 5, 0));
    object.add(lines(polygon2DLines(linearRing, 0), palette.surfaceAlt));

    // The composed affine map applied to the ring directly, through
    // `Polygon2D.Deform`. It is drawn only when it evaluates: the mapping needs
    // `Point2D.Transform(AffineTransform2D)`, whose body reaches for the
    // `Vector2D.Transform(Matrix3x2)` overload the writer dropped.
    const affineRing = reading('Polygon2D.Deform(p => p.Transform(affine))', () => {
      const image = source.Deform(p => p.Transform(affine));
      object.add(lines(polygon2DLines(image, 0.03), palette.accent));
      object.add(markers2D(toArray(image.Points), palette.accent, 5, 0.03));
      return p2s(image.Points.At(0));
    });

    // The composed map's own frame, from the Pose2D the matrix decomposes into.
    const pose = affine.Matrix.Pose2D();
    const frame = pose.Frame2D();
    const axis = (v: Vector2D, color: number): THREE.LineSegments =>
      segments(
        [frame.Origin.X, frame.Origin.Y, 0.02, frame.Origin.X + v.X * 0.6, frame.Origin.Y + v.Y * 0.6, 0.02],
        color,
      );
    object.add(axis(frame.XAxis.Vector, AXIS_COLORS[0]));
    object.add(axis(frame.YAxis.Vector, AXIS_COLORS[1]));

    const inverse = affine.Inverse();
    const identity2 = (m: Matrix3x2): number => {
      let worst = 0;
      for (let r = 0; r < 3; r++) {
        for (let c = 0; c < 2; c++) {
          worst = Math.max(worst, Math.abs(m.ElementAt(r, c) - (r === c ? 1 : 0)));
        }
      }
      return worst;
    };

    return {
      object,
      readings: [
        reading('composed Matrix3x2', () => affine2Text(affine.Matrix)),
        reading('Determinant', () => n4(affine.Matrix.Determinant())),
        reading('AffineTransform2D.CanInvert', () => String(affine.CanInvert())),
        reading('Matrix3x2.Invert ok', () => String(affine.Matrix.Invert().X1)),
        reading('Compose(affine, Inverse) vs Identity', () =>
          sci(identity2(affine.Compose(inverse).Matrix)),
        ),
        reading('Pose2D from the matrix', () =>
          `${p2s(pose.Position)} @ ${n2(pose.Rotation.Angle.Degrees())}°`,
        ),
        note('lattice', `${dots.length} points`),
        reading('Polygon2D.Transform(Rotation2D)', () => p2s(moved.Points.At(0))),
        affineRing,
        reading('Point2D.Transform(AffineTransform2D)', () =>
          p2s(new Point2D(1, 0).Transform(affine)),
        ),
        reading('Vector2D.Transform(AffineTransform2D)', () =>
          p2s(new Vector2D(1, 0).Transform(affine).ToPoint()),
        ),
        flag(
          'Transform2D.Compose[0,0]',
          () => lifted.Compose(Transform2D.Identity()).Matrix.ElementAt(0, 0),
          'the Matrix3x3×Matrix3x3 overload of Multiply was dropped, so the scalar body ran',
        ),
        flag(
          'Matrix3x2.CreateScale(Number2)[0,0]',
          () => (Matrix3x2.CreateScale as unknown as (s: unknown) => Matrix3x2)(
            new Number2(params.scale, 1),
          ).ElementAt(0, 0),
          'the Number2 overload was dropped',
        ),
        reading('RotationAbout2D centre correction', () => {
          const about = new RotationAbout2D(rotation, new Point2D(1, 0)).AffineTransform2D();
          const row = about.Matrix.Row3;
          return row.X === 0 && row.Y === 0
            ? 'ZERO — Matrix3x2.CreateRotation(angle, centre) was dropped, so the centre is ignored'
            : `(${n2(row.X)}, ${n2(row.Y)})`;
        }),
      ],
    };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Transforms',
  subtitle:
    'transforms.{concepts,types,library}.plato · rotations.types.plato · ' +
    'rotations-ops.library.plato · matrices.{types,library}.plato · matrices-ops.library.plato',
  scenes: [trs, representations, interpolation, composeInverse, applied, frames, planar],
};

mountDemo(demo, SPATIAL);

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
