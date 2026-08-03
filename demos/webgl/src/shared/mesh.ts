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
