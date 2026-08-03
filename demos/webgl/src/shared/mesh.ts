// Bridge from the generated Plato geometry types to Three.js buffers.
//
// Nothing here computes geometry: positions and face loops come from the
// generated members (`PolygonMesh3D.FacePositions`, `Polygon3D.Points`, …) and
// this file only repacks them. Triangulation of a face is a fan, which is
// correct for the convex faces every producer in these demos emits.

import * as THREE from 'three';
import '../plato/array-ext.js';
import {
  Intrinsics,
  Point2D,
  Point3D,
  Polygon2D,
  Polygon3D,
  PolygonMesh3D,
  PolygonSoup3D,
  TriangleArray3D,
  TriangleMesh3D,
  type IArray,
} from '../plato/plato.g.js';

export function toVector3(p: Point3D): THREE.Vector3 {
  return new THREE.Vector3(p.X, p.Y, p.Z);
}

export function toArray<T>(xs: IArray<T>): T[] {
  const out: T[] = [];
  for (let i = 0; i < xs.Count(); i++) out.push(xs.At(i));
  return out;
}

/** IArray over a plain JS array, for handing values back to generated members. */
export function fromArray<T>(xs: readonly T[]): IArray<T> {
  return Intrinsics.MakeArray(...(xs as T[]));
}

/** Face loops of a polygon mesh as flat-shaded triangles. */
export function polygonMeshGeometry(mesh: PolygonMesh3D): THREE.BufferGeometry {
  const positions: number[] = [];
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const loop = toArray(mesh.FacePositions(f));
    for (let i = 1; i + 1 < loop.length; i++) {
      for (const p of [loop[0], loop[i], loop[i + 1]]) positions.push(p.X, p.Y, p.Z);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.computeVertexNormals();
  return geometry;
}

/** The same, for a soup of independent face polygons (the CSG result type). */
export function polygonSoupGeometry(soup: PolygonSoup3D): THREE.BufferGeometry {
  const positions: number[] = [];
  for (const poly of toArray(soup.Polygons)) {
    const loop = toArray(poly.Points);
    for (let i = 1; i + 1 < loop.length; i++) {
      for (const p of [loop[0], loop[i], loop[i + 1]]) positions.push(p.X, p.Y, p.Z);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.computeVertexNormals();
  return geometry;
}

/** Every face loop as a closed line loop — the polyhedron's real edges. */
export function polygonMeshEdges(mesh: PolygonMesh3D): THREE.BufferGeometry {
  const positions: number[] = [];
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const loop = toArray(mesh.FacePositions(f));
    for (let i = 0; i < loop.length; i++) {
      const a = loop[i];
      const b = loop[(i + 1) % loop.length];
      positions.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return geometry;
}

/** A closed 2D ring as a line loop on the z = 0 plane. */
export function polygon2DLines(polygon: Polygon2D, z = 0): THREE.BufferGeometry {
  const loop = toArray(polygon.Points);
  const positions: number[] = [];
  for (let i = 0; i < loop.length; i++) {
    const a = loop[i];
    const b = loop[(i + 1) % loop.length];
    positions.push(a.X, a.Y, z, b.X, b.Y, z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return geometry;
}

/** A 3D polygon as a closed line loop. */
export function polygon3DLines(polygon: Polygon3D): THREE.BufferGeometry {
  const loop = toArray(polygon.Points);
  const positions: number[] = [];
  for (let i = 0; i < loop.length; i++) {
    const a = loop[i];
    const b = loop[(i + 1) % loop.length];
    positions.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return geometry;
}

/**
 * A soup of independent triangles — what the marching-cubes members return.
 * Normals come from the triangles themselves, which is what an isosurface built
 * one cell at a time can honestly claim: neighbouring cells emit their own
 * copies of a shared vertex, so there is nothing to average across.
 */
export function triangleArrayGeometry(triangles: TriangleArray3D): THREE.BufferGeometry {
  const positions: number[] = [];
  for (const t of toArray(triangles.Triangles)) {
    for (const p of [t.A, t.B, t.C]) positions.push(p.X, p.Y, p.Z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.computeVertexNormals();
  return geometry;
}

/** An indexed triangle mesh, kept indexed so shared vertices average normals. */
export function triangleMeshGeometry(mesh: TriangleMesh3D): THREE.BufferGeometry {
  const positions: number[] = [];
  for (const p of toArray(mesh.Positions)) positions.push(p.X, p.Y, p.Z);
  const indices: number[] = [];
  for (const f of toArray(mesh.Faces)) indices.push(f.A.Value, f.B.Value, f.C.Value);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.setIndex(indices);
  geometry.computeVertexNormals();
  return geometry;
}

/** An open chain of points as one polyline — parametric curves, field lines. */
export function polylineGeometry(points: readonly Point3D[]): THREE.BufferGeometry {
  const positions: number[] = [];
  for (let i = 0; i + 1 < points.length; i++) {
    const a = points[i];
    const b = points[i + 1];
    positions.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return geometry;
}

/**
 * A parametric surface sampled on a (u, v) lattice as an indexed quad grid.
 * `evaluate` is the generated `Eval`; nothing here knows which surface it is.
 * `closedU` / `closedV` wrap the lattice so a torus has no visible seam.
 */
export function parametricGeometry(
  evaluate: (u: number, v: number) => Point3D,
  uSteps: number,
  vSteps: number,
  closedU = false,
  closedV = false,
): THREE.BufferGeometry {
  const uCount = uSteps + 1;
  const vCount = vSteps + 1;
  const positions: number[] = [];
  for (let j = 0; j < vCount; j++) {
    for (let i = 0; i < uCount; i++) {
      const p = evaluate(i / uSteps, j / vSteps);
      positions.push(p.X, p.Y, p.Z);
    }
  }
  const at = (i: number, j: number): number =>
    (closedV && j === vSteps ? 0 : j) * uCount + (closedU && i === uSteps ? 0 : i);
  const indices: number[] = [];
  for (let j = 0; j < vSteps; j++) {
    for (let i = 0; i < uSteps; i++) {
      const a = at(i, j);
      const b = at(i + 1, j);
      const c = at(i + 1, j + 1);
      const d = at(i, j + 1);
      indices.push(a, b, c, a, c, d);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.setIndex(indices);
  geometry.computeVertexNormals();
  return geometry;
}

/** Every face of a polygon mesh as a `Polygon3D`, the input CSG expects. */
export function meshToSoup(mesh: PolygonMesh3D): PolygonSoup3D {
  const polygons = Intrinsics.Range(mesh.FaceCount()).Map(f => new Polygon3D(mesh.FacePositions(f)));
  return new PolygonSoup3D(polygons);
}

export function point2D(x: number, y: number): Point2D {
  return new Point2D(x, y);
}

export function point3D(x: number, y: number, z: number): Point3D {
  return new Point3D(x, y, z);
}
