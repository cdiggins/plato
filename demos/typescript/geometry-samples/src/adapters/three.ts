// Adapter layer between the Plato geometry library / core scene types and
// Three.js.
//
// This is the only module (besides the app shell) that imports Three.js.
// Everything crossing the boundary goes through here, in both directions:
//
//   plato/core -> three:  vector3DToThree, meshDataToBufferGeometry,
//                         drawableToObject3D, drawablesToGroup
//   three -> plato/core:  threeVectorToVector3D, bufferGeometryToMeshData
//
// Keeping the conversions in one place means samples stay portable (Node,
// workers, other renderers) and the Three.js API surface is easy to audit.

import * as THREE from 'three';
import type { Drawable, LinesData, MeshData, PointsData } from '../core/types.js';
import { Point3D, TriangleMesh3D, Vector3D } from '../plato/plato.g.js';
import { meshFromIndices, meshIndices, toArray } from '../core/meshBuilder.js';

const DEFAULT_MESH_COLOR = 0x8899aa;

// ---- plato/core -> three ---------------------------------------------------

export function vector3DToThree(v: Vector3D): THREE.Vector3 {
    return new THREE.Vector3(v.X, v.Y, v.Z);
}

export function point3DToThree(p: Point3D): THREE.Vector3 {
    return new THREE.Vector3(p.X, p.Y, p.Z);
}

/** Flattens Plato points into the xyz triples a BufferAttribute wants. */
export function flattenPoints(points: readonly Point3D[]): number[] {
    const out = new Array<number>(points.length * 3);
    for (let i = 0; i < points.length; i++) {
        out[i * 3] = points[i].X;
        out[i * 3 + 1] = points[i].Y;
        out[i * 3 + 2] = points[i].Z;
    }
    return out;
}

export function meshDataToBufferGeometry(mesh: MeshData): THREE.BufferGeometry {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position',
        new THREE.Float32BufferAttribute(flattenPoints(toArray(mesh.mesh.Positions)), 3));
    geometry.setIndex(meshIndices(mesh.mesh));
    if (mesh.flatShading) {
        geometry.computeVertexNormals();
    } else {
        // Area-weighted vertex normals from the stdlib, not from Three.js.
        const normals = toArray(mesh.mesh.VertexNormalVectors());
        geometry.setAttribute('normal', new THREE.Float32BufferAttribute(
            normals.flatMap(n => [n.X, n.Y, n.Z]), 3));
    }
    return geometry;
}

export function meshDataToObject3D(mesh: MeshData): THREE.Mesh {
    const material = new THREE.MeshStandardMaterial({
        color: mesh.color ?? DEFAULT_MESH_COLOR,
        flatShading: mesh.flatShading ?? false,
        wireframe: mesh.wireframe ?? false,
        transparent: mesh.opacity !== undefined && mesh.opacity < 1,
        opacity: mesh.opacity ?? 1,
        side: THREE.DoubleSide,
        metalness: 0.1,
        roughness: 0.75,
    });
    return new THREE.Mesh(meshDataToBufferGeometry(mesh), material);
}

export function linesDataToObject3D(lines: LinesData): THREE.LineSegments {
    const geometry = new THREE.BufferGeometry();
    const ends = lines.segments.flatMap(s => [s.A, s.B]);
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(flattenPoints(ends), 3));
    const material = new THREE.LineBasicMaterial({
        color: lines.color ?? 0xffffff,
        transparent: lines.opacity !== undefined && lines.opacity < 1,
        opacity: lines.opacity ?? 1,
    });
    return new THREE.LineSegments(geometry, material);
}

export function pointsDataToObject3D(points: PointsData): THREE.Points {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(flattenPoints(points.points), 3));
    const material = new THREE.PointsMaterial({
        color: points.color ?? 0xffffff,
        size: points.size ?? 0.05,
        sizeAttenuation: true,
    });
    return new THREE.Points(geometry, material);
}

export function drawableToObject3D(drawable: Drawable): THREE.Object3D {
    switch (drawable.kind) {
        case 'mesh': return meshDataToObject3D(drawable);
        case 'lines': return linesDataToObject3D(drawable);
        case 'points': return pointsDataToObject3D(drawable);
    }
}

export function drawablesToGroup(drawables: Drawable[]): THREE.Group {
    const group = new THREE.Group();
    for (const d of drawables)
        group.add(drawableToObject3D(d));
    return group;
}

// ---- three -> plato/core ---------------------------------------------------

export function threeVectorToVector3D(v: THREE.Vector3): Vector3D {
    return new Vector3D(v.x, v.y, v.z);
}

export function bufferGeometryToMesh(geometry: THREE.BufferGeometry): TriangleMesh3D {
    const position = geometry.getAttribute('position');
    const positions: Point3D[] = [];
    for (let i = 0; i < position.count; i++)
        positions.push(new Point3D(position.getX(i), position.getY(i), position.getZ(i)));

    const indices = geometry.index
        ? Array.from(geometry.index.array as ArrayLike<number>)
        : Array.from({ length: position.count }, (_, i) => i); // soup -> trivial index

    return meshFromIndices(positions, indices);
}

// ---- lifetime --------------------------------------------------------------

/** Disposes every geometry and material below root (Three.js needs this). */
export function disposeObject3D(root: THREE.Object3D): void {
    root.traverse(obj => {
        const anyObj = obj as { geometry?: THREE.BufferGeometry; material?: THREE.Material | THREE.Material[] };
        anyObj.geometry?.dispose();
        const material = anyObj.material;
        if (Array.isArray(material))
            material.forEach(m => m.dispose());
        else
            material?.dispose();
    });
}
