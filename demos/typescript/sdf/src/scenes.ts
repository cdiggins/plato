import {
  Angle,
  FunctionSdf2D,
  FunctionSdf3D,
  Point2D,
  Point3D,
  SdfShellModifier,
  SdfTwistModifier3D,
  Vector2D,
  Vector3D,
} from './plato/plato.g.js';

export type Dim = '2d' | '3d';

export interface SceneParam {
  key: string;
  label: string;
  min: number;
  max: number;
  default: number;
  step?: number;
}

export interface Scene {
  id: string;
  title: string;
  dim: Dim;
  description: string;
  plato: string;
  params: SceneParam[];
  /** Build a generated FunctionSdf from live slider values. */
  build(params: number[]): FunctionSdf2D | FunctionSdf3D;
}

function sdf2(f: (p: Point2D) => number): FunctionSdf2D {
  return FunctionSdf2D.Create(f);
}

function sdf3(f: (p: Point3D) => number): FunctionSdf3D {
  return FunctionSdf3D.Create(f);
}

export const scenes: Scene[] = [
  {
    id: 'circle',
    title: 'Circle',
    dim: '2d',
    description: 'Point2D.DistanceToCircle — exact signed distance.',
    plato: 'p.DistanceToCircle(radius)',
    params: [{ key: 'radius', label: 'Radius', min: 0.1, max: 1.4, default: 0.7 }],
    build: ([r]) => sdf2((p) => p.DistanceToCircle(r)),
  },
  {
    id: 'box2',
    title: 'Box 2D',
    dim: '2d',
    description: 'Point2D.DistanceToBox — axis-aligned rectangle.',
    plato: 'p.DistanceToBox(halfExtents)',
    params: [
      { key: 'hx', label: 'Half-width', min: 0.1, max: 1.2, default: 0.8 },
      { key: 'hy', label: 'Half-height', min: 0.1, max: 1.2, default: 0.45 },
    ],
    build: ([hx, hy]) => sdf2((p) => p.DistanceToBox(new Vector2D(hx, hy))),
  },
  {
    id: 'rounded-box2',
    title: 'Rounded Box 2D',
    dim: '2d',
    description: 'Point2D.DistanceToRoundedBox — corners rounded by Radius.',
    plato: 'p.DistanceToRoundedBox(halfExtents, radius)',
    params: [
      { key: 'hx', label: 'Half-width', min: 0.1, max: 1.2, default: 0.75 },
      { key: 'hy', label: 'Half-height', min: 0.1, max: 1.2, default: 0.4 },
      { key: 'r', label: 'Corner radius', min: 0.0, max: 0.5, default: 0.18 },
    ],
    build: ([hx, hy, r]) => sdf2((p) => p.DistanceToRoundedBox(new Vector2D(hx, hy), r)),
  },
  {
    id: 'ring',
    title: 'Ring / Annulus',
    dim: '2d',
    description: 'Point2D.DistanceToRing — planar counterpart of the torus.',
    plato: 'p.DistanceToRing(radius, thickness)',
    params: [
      { key: 'radius', label: 'Radius', min: 0.2, max: 1.2, default: 0.65 },
      { key: 'thickness', label: 'Thickness', min: 0.02, max: 0.5, default: 0.18 },
    ],
    build: ([r, t]) => sdf2((p) => p.DistanceToRing(r, t)),
  },
  {
    id: 'capsule2',
    title: 'Capsule 2D',
    dim: '2d',
    description: 'Point2D.DistanceToCapsule — segment thickened by radius.',
    plato: 'p.DistanceToCapsule(a, b, radius)',
    params: [{ key: 'radius', label: 'Radius', min: 0.05, max: 0.45, default: 0.18 }],
    build: ([r]) =>
      sdf2((p) => p.DistanceToCapsule(new Point2D(-0.7, -0.35), new Point2D(0.65, 0.4), r)),
  },
  {
    id: 'triangle2',
    title: 'Triangle 2D',
    dim: '2d',
    description: 'Point2D.DistanceToTriangle — winding-agnostic signed field.',
    plato: 'p.DistanceToTriangle(a, b, c)',
    params: [],
    build: () =>
      sdf2((p) =>
        p.DistanceToTriangle(new Point2D(-0.7, -0.55), new Point2D(0.85, -0.4), new Point2D(0.05, 0.75)),
      ),
  },
  {
    id: 'quad2',
    title: 'Quad 2D',
    dim: '2d',
    description: 'Point2D.DistanceToQuad — even-odd sign for concave rings.',
    plato: 'p.DistanceToQuad(a, b, c, d)',
    params: [],
    build: () =>
      sdf2((p) =>
        p.DistanceToQuad(
          new Point2D(-0.75, -0.55),
          new Point2D(0.8, -0.45),
          new Point2D(0.55, 0.65),
          new Point2D(-0.35, 0.4),
        ),
      ),
  },
  {
    id: 'union2',
    title: 'Union 2D',
    dim: '2d',
    description: 'Number.UnionDistance — pointwise min of circle and box.',
    plato: 'a.UnionDistance(b)',
    params: [],
    build: () =>
      sdf2((p) => {
        const a = new Point2D(p.X + 0.35, p.Y).DistanceToCircle(0.55);
        const b = new Point2D(p.X - 0.4, p.Y).DistanceToBox(new Vector2D(0.45, 0.35));
        return a.UnionDistance(b);
      }),
  },
  {
    id: 'smooth-union2',
    title: 'Smooth Union 2D',
    dim: '2d',
    description: 'Number.SmoothUnionDistance — polynomial blend over blendRadius.',
    plato: 'a.SmoothUnionDistance(b, blendRadius)',
    params: [{ key: 'k', label: 'Blend radius', min: 0.01, max: 0.6, default: 0.22 }],
    build: ([k]) =>
      sdf2((p) => {
        const a = new Point2D(p.X + 0.35, p.Y).DistanceToCircle(0.55);
        const b = new Point2D(p.X - 0.4, p.Y).DistanceToBox(new Vector2D(0.45, 0.35));
        return a.SmoothUnionDistance(b, k);
      }),
  },
  {
    id: 'difference2',
    title: 'Difference 2D',
    dim: '2d',
    description: 'Number.SubtractDistance — box carved from a circle.',
    plato: 'a.SubtractDistance(b)',
    params: [],
    build: () =>
      sdf2((p) => {
        const a = p.DistanceToCircle(0.75);
        const b = new Point2D(p.X - 0.35, p.Y).DistanceToBox(new Vector2D(0.55, 0.35));
        return a.SubtractDistance(b);
      }),
  },
  {
    id: 'xor2',
    title: 'Exclusive Or 2D',
    dim: '2d',
    description: 'Number.ExclusiveOrDistance — inside exactly one shape.',
    plato: 'a.ExclusiveOrDistance(b)',
    params: [],
    build: () =>
      sdf2((p) => {
        const a = new Point2D(p.X + 0.25, p.Y).DistanceToCircle(0.6);
        const b = new Point2D(p.X - 0.35, p.Y).DistanceToCircle(0.55);
        return a.ExclusiveOrDistance(b);
      }),
  },
  {
    id: 'shell2',
    title: 'Shell 2D',
    dim: '2d',
    description: 'SdfShellModifier.ApplyToDistance — thin region around the boundary.',
    plato: 'new SdfShellModifier(t).ApplyToDistance(d)',
    params: [{ key: 't', label: 'Thickness', min: 0.02, max: 0.35, default: 0.12 }],
    build: ([t]) => {
      const shell = new SdfShellModifier(t);
      return sdf2((p) => shell.ApplyToDistance(p.DistanceToRoundedBox(new Vector2D(0.7, 0.45), 0.15)));
    },
  },

  // —— 3D ——
  {
    id: 'sphere',
    title: 'Sphere',
    dim: '3d',
    description: 'Point3D.DistanceToSphere — exact signed distance at the origin.',
    plato: 'p.DistanceToSphere(radius)',
    params: [{ key: 'radius', label: 'Radius', min: 0.2, max: 1.4, default: 0.85 }],
    build: ([r]) => sdf3((p) => p.DistanceToSphere(r)),
  },
  {
    id: 'box3',
    title: 'Box 3D',
    dim: '3d',
    description: 'Point3D.DistanceToBox — axis-aligned box with half-extents.',
    plato: 'p.DistanceToBox(halfExtents)',
    params: [
      { key: 'hx', label: 'Half X', min: 0.15, max: 1.2, default: 0.7 },
      { key: 'hy', label: 'Half Y', min: 0.15, max: 1.2, default: 0.45 },
      { key: 'hz', label: 'Half Z', min: 0.15, max: 1.2, default: 0.55 },
    ],
    build: ([hx, hy, hz]) => sdf3((p) => p.DistanceToBox(new Vector3D(hx, hy, hz))),
  },
  {
    id: 'rounded-box3',
    title: 'Rounded Box 3D',
    dim: '3d',
    description: 'Point3D.DistanceToRoundedBox — edges rounded by Radius.',
    plato: 'p.DistanceToRoundedBox(halfExtents, radius)',
    params: [
      { key: 'hx', label: 'Half X', min: 0.15, max: 1.0, default: 0.6 },
      { key: 'hy', label: 'Half Y', min: 0.15, max: 1.0, default: 0.4 },
      { key: 'hz', label: 'Half Z', min: 0.15, max: 1.0, default: 0.5 },
      { key: 'r', label: 'Radius', min: 0.0, max: 0.4, default: 0.15 },
    ],
    build: ([hx, hy, hz, r]) => sdf3((p) => p.DistanceToRoundedBox(new Vector3D(hx, hy, hz), r)),
  },
  {
    id: 'torus',
    title: 'Torus',
    dim: '3d',
    description: 'Point3D.DistanceToTorus — ring in the XZ plane.',
    plato: 'p.DistanceToTorus(majorRadius, tubeRadius)',
    params: [
      { key: 'R', label: 'Major radius', min: 0.3, max: 1.4, default: 0.85 },
      { key: 'r', label: 'Tube radius', min: 0.05, max: 0.45, default: 0.22 },
    ],
    build: ([R, r]) => sdf3((p) => p.DistanceToTorus(R, r)),
  },
  {
    id: 'capsule3',
    title: 'Capsule 3D',
    dim: '3d',
    description: 'Point3D.DistanceToVerticalCapsule — segment along +Y.',
    plato: 'p.DistanceToVerticalCapsule(height, radius)',
    params: [
      { key: 'h', label: 'Height', min: 0.3, max: 2.0, default: 1.2 },
      { key: 'r', label: 'Radius', min: 0.05, max: 0.5, default: 0.28 },
    ],
    build: ([h, r]) =>
      sdf3((p) => new Point3D(p.X, p.Y + h * 0.5, p.Z).DistanceToVerticalCapsule(h, r)),
  },
  {
    id: 'cylinder',
    title: 'Capped Cylinder',
    dim: '3d',
    description: 'Point3D.DistanceToCappedCylinder — Y-axis cylinder with flat caps.',
    plato: 'p.DistanceToCappedCylinder(halfHeight, radius)',
    params: [
      { key: 'h', label: 'Half height', min: 0.15, max: 1.2, default: 0.55 },
      { key: 'r', label: 'Radius', min: 0.15, max: 1.0, default: 0.55 },
    ],
    build: ([h, r]) => sdf3((p) => p.DistanceToCappedCylinder(h, r)),
  },
  {
    id: 'capped-cone',
    title: 'Capped Cone',
    dim: '3d',
    description: 'Point3D.DistanceToCappedCone — conical frustum.',
    plato: 'p.DistanceToCappedCone(halfHeight, bottomRadius, topRadius)',
    params: [
      { key: 'h', label: 'Half height', min: 0.2, max: 1.2, default: 0.7 },
      { key: 'r1', label: 'Bottom radius', min: 0.0, max: 1.0, default: 0.7 },
      { key: 'r2', label: 'Top radius', min: 0.0, max: 1.0, default: 0.15 },
    ],
    build: ([h, r1, r2]) => sdf3((p) => p.DistanceToCappedCone(h, r1, r2)),
  },
  {
    id: 'ellipsoid',
    title: 'Ellipsoid',
    dim: '3d',
    description: 'Point3D.DistanceToEllipsoid — lower bound (safe for sphere tracing).',
    plato: 'p.DistanceToEllipsoid(radii)',
    params: [
      { key: 'rx', label: 'Radius X', min: 0.2, max: 1.2, default: 1.0 },
      { key: 'ry', label: 'Radius Y', min: 0.2, max: 1.2, default: 0.45 },
      { key: 'rz', label: 'Radius Z', min: 0.2, max: 1.2, default: 0.7 },
    ],
    build: ([rx, ry, rz]) => sdf3((p) => p.DistanceToEllipsoid(new Vector3D(rx, ry, rz))),
  },
  {
    id: 'smooth-blob',
    title: 'Smooth Union Blob',
    dim: '3d',
    description: 'Two spheres + box blended with SmoothUnionDistance.',
    plato: 'a.SmoothUnionDistance(b, blendRadius)',
    params: [{ key: 'k', label: 'Blend radius', min: 0.02, max: 0.55, default: 0.28 }],
    build: ([k]) =>
      sdf3((p) => {
        const s1 = new Point3D(p.X + 0.55, p.Y, p.Z).DistanceToSphere(0.7);
        const s2 = new Point3D(p.X - 0.55, p.Y, p.Z).DistanceToSphere(0.7);
        const box = p.DistanceToBox(new Vector3D(0.5, 0.4, 0.5));
        return s1.SmoothUnionDistance(s2, k).SmoothUnionDistance(box, k * 0.7);
      }),
  },
  {
    id: 'difference3',
    title: 'Difference 3D',
    dim: '3d',
    description: 'Number.SubtractDistance — sphere carved from a rounded box.',
    plato: 'a.SubtractDistance(b)',
    params: [{ key: 'r', label: 'Cutter radius', min: 0.3, max: 1.1, default: 0.7 }],
    build: ([r]) =>
      sdf3((p) => {
        const a = p.DistanceToRoundedBox(new Vector3D(0.85, 0.55, 0.65), 0.08);
        const b = new Point3D(p.X - 0.35, p.Y - 0.15, p.Z).DistanceToSphere(r);
        return a.SubtractDistance(b);
      }),
  },
  {
    id: 'chamfer-union',
    title: 'Chamfer Union',
    dim: '3d',
    description: 'Number.ChamferUnionDistance — 45° bevel of Width at the seam.',
    plato: 'a.ChamferUnionDistance(b, width)',
    params: [{ key: 'w', label: 'Chamfer width', min: 0.02, max: 0.45, default: 0.18 }],
    build: ([w]) =>
      sdf3((p) => {
        const a = new Point3D(p.X + 0.35, p.Y, p.Z).DistanceToBox(new Vector3D(0.5, 0.45, 0.45));
        const b = new Point3D(p.X - 0.4, p.Y, p.Z).DistanceToSphere(0.55);
        return a.ChamferUnionDistance(b, w);
      }),
  },
  {
    id: 'twist',
    title: 'Twist Modifier',
    dim: '3d',
    description: 'SdfTwistModifier3D.ApplyToDomain then sample a box.',
    plato: 'twist.ApplyToDomain(p).DistanceToBox(...)',
    params: [{ key: 'a', label: 'Angle / unit (rad)', min: 0.0, max: 2.5, default: 1.1 }],
    build: ([a]) => {
      const twist = new SdfTwistModifier3D(new Angle(a));
      return sdf3((p) => twist.ApplyToDomain(p).DistanceToBox(new Vector3D(0.55, 0.55, 0.9)));
    },
  },
  {
    id: 'extrude',
    title: 'Extrude 2D → 3D',
    dim: '3d',
    description: 'Number.ExtrudedDistance — planar rounded box swept along Z.',
    plato: 'planar.ExtrudedDistance(z, halfHeight)',
    params: [{ key: 'h', label: 'Half height', min: 0.1, max: 1.0, default: 0.35 }],
    build: ([h]) =>
      sdf3((p) => {
        const planar = new Point2D(p.X, p.Y).DistanceToRoundedBox(new Vector2D(0.7, 0.4), 0.12);
        return planar.ExtrudedDistance(p.Z, h);
      }),
  },
  {
    id: 'revolve',
    title: 'Revolve 2D → 3D',
    dim: '3d',
    description: 'Point3D.RevolvedPoint — circle profile spun about Y.',
    plato: 'p.RevolvedPoint(offset).DistanceToCircle(r)',
    params: [
      { key: 'offset', label: 'Orbit radius', min: 0.2, max: 1.2, default: 0.7 },
      { key: 'r', label: 'Profile radius', min: 0.08, max: 0.5, default: 0.22 },
    ],
    build: ([offset, r]) => sdf3((p) => p.RevolvedPoint(offset).DistanceToCircle(r)),
  },
  {
    id: 'shell3',
    title: 'Shell 3D',
    dim: '3d',
    description: 'SdfShellModifier on a torus — hollow tube surface.',
    plato: 'new SdfShellModifier(t).ApplyToDistance(d)',
    params: [{ key: 't', label: 'Thickness', min: 0.02, max: 0.25, default: 0.08 }],
    build: ([t]) => {
      const shell = new SdfShellModifier(t);
      return sdf3((p) => shell.ApplyToDistance(p.DistanceToTorus(0.85, 0.32)));
    },
  },
  {
    id: 'scene',
    title: 'CSG Scene',
    dim: '3d',
    description: 'Smooth blob + torus + ground plane — classic SDF showcase.',
    plato: 'Union / SmoothUnion / DistanceToPlane',
    params: [{ key: 'k', label: 'Blend radius', min: 0.02, max: 0.5, default: 0.22 }],
    build: ([k]) =>
      sdf3((p) => {
        const s1 = new Point3D(p.X + 0.5, p.Y - 0.35, p.Z).DistanceToSphere(0.55);
        const s2 = new Point3D(p.X - 0.5, p.Y - 0.35, p.Z).DistanceToSphere(0.55);
        const box = new Point3D(p.X, p.Y - 0.3, p.Z).DistanceToBox(new Vector3D(0.45, 0.3, 0.45));
        const blob = s1.SmoothUnionDistance(s2, k).SmoothUnionDistance(box, k * 0.8);
        const ring = new Point3D(p.X, p.Y - 0.35, p.Z).DistanceToTorus(0.95, 0.1);
        const solid = blob.UnionDistance(ring);
        const ground = p.DistanceToPlane(new Vector3D(0, 1, 0), 0.05);
        return solid.UnionDistance(ground);
      }),
  },
];

export function scenesByDim(dim: Dim): Scene[] {
  return scenes.filter((s) => s.dim === dim);
}
