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
import { Vector3D } from '../plato/plato.g.js';

const DEFAULT_MESH_COLOR = 0x8899aa;

// ---- plato/core -> three ---------------------------------------------------

export function vector3DToThree(v: Vector3D): THREE.Vector3 {
    return new THREE.Vector3(v.X, v.Y, v.Z);
}

export function meshDataToBufferGeometry(mesh: MeshData): THREE.BufferGeometry {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(mesh.positions, 3));
    geometry.setIndex(mesh.indices);
    if (mesh.normals && !mesh.flatShading)
        geometry.setAttribute('normal', new THREE.Float32BufferAttribute(mesh.normals, 3));
    else
        geometry.computeVertexNormals();
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
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(lines.positions, 3));
    const material = new THREE.LineBasicMaterial({
        color: lines.color ?? 0xffffff,
        transparent: lines.opacity !== undefined && lines.opacity < 1,
        opacity: lines.opacity ?? 1,
    });
    return new THREE.LineSegments(geometry, material);
}

export function pointsDataToObject3D(points: PointsData): THREE.Points {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(points.positions, 3));
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

export function bufferGeometryToMeshData(geometry: THREE.BufferGeometry): MeshData {
    const position = geometry.getAttribute('position');
    const positions = Array.from(position.array as ArrayLike<number>).slice(0, position.count * 3);

    let indices: number[];
    if (geometry.index) {
        indices = Array.from(geometry.index.array as ArrayLike<number>);
    } else {
        indices = Array.from({ length: position.count }, (_, i) => i); // soup -> trivial index
    }

    const normalAttr = geometry.getAttribute('normal');
    const normals = normalAttr
        ? Array.from(normalAttr.array as ArrayLike<number>).slice(0, normalAttr.count * 3)
        : undefined;

    return { kind: 'mesh', positions, indices, normals };
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
