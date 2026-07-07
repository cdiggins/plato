// The demo browser for the Rust geometry samples.
//
// The geometry is computed by the Rust crate compiled to WebAssembly
// (geometry_samples.wasm, hand-rolled C ABI — see src/wasm_api.rs). This
// module reads the drawables out of wasm linear memory and renders them with
// Three.js, mirroring demos/typescript/geometry-samples (the TypeScript demo). Colors and
// point sizes live here: the Rust Drawable carries pure geometry only.

import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

// ---- Sample metadata (titles, descriptions, per-drawable styles) -----------
// The order matches the Rust registry (src/samples/mod.rs); styles mirror the
// colors of the TypeScript demo's build() functions.

const SAMPLES = [
    {
        id: 'parametric-surface', file: 'parametric_surface.rs', title: 'Parametric Surface',
        desc: 'Tessellating f(u, v) into an indexed triangle mesh with vertex normals.',
        styles: [{ color: 0x4da3ff }],
    },
    {
        id: 'icosphere', file: 'icosphere.rs', title: 'Icosphere Subdivision',
        desc: 'Recursive triangle subdivision of an icosahedron with an edge-midpoint cache.',
        styles: [0, 1, 2, 3].map(() => ({ color: 0x4da3ff, flat: true })),
    },
    {
        id: 'terrain', file: 'terrain.rs', title: 'Value-Noise Terrain',
        desc: 'Fractal Brownian motion over a hashed lattice displaces a heightfield grid.',
        styles: [{ color: 0x6fbf73, flat: true }],
    },
    {
        id: 'delaunay', file: 'delaunay.rs', title: 'Delaunay Triangulation',
        desc: 'Bowyer-Watson incremental insertion with the empty-circumcircle property.',
        styles: [{ color: 0x27435f, flat: true }, { color: 0x7fc4ff }, { color: 0xffd166, size: 0.05 }],
    },
    {
        id: 'convex-hull', file: 'convex_hull.rs', title: 'Convex Hull',
        desc: "Andrew's monotone chain in O(n log n), extruded into a prism.",
        styles: [{ color: 0x4da3ff, opacity: 0.35, flat: true }, { color: 0x9fd0ff }, { color: 0xffd166, size: 0.06 }],
    },
    {
        id: 'spline-tube', file: 'spline_tube.rs', title: 'Spline + Tube Sweep',
        desc: 'Catmull-Rom interpolation with twist-free parallel-transport frames.',
        styles: [{ color: 0xc678dd }, { color: 0x556070 }, { color: 0xffd166, size: 0.09 }],
    },
    {
        id: 'octree', file: 'octree.rs', title: 'Octree',
        desc: 'Adaptive spatial subdivision: leaves split at 8 points, down to depth 5.',
        styles: [{ color: 0x3f76a8, opacity: 0.7 }, { color: 0xffd166, size: 0.035 }],
    },
    {
        id: 'bvh', file: 'bvh.rs', title: 'BVH (AABB Tree)',
        desc: 'Median-split bounding box hierarchy over icosphere triangles, shown at three depths.',
        styles: [
            { color: 0x4da3ff, opacity: 0.25 }, { color: 0x4da3ff, opacity: 0.25 }, { color: 0x4da3ff, opacity: 0.25 },
            { color: 0xff6b6b, opacity: 0.85 }, { color: 0xffd166, opacity: 0.85 }, { color: 0x6fbf73, opacity: 0.85 },
        ],
    },
    {
        id: 'half-edge', file: 'half_edge.rs', title: 'Half-Edge + Smoothing',
        desc: 'Half-edge connectivity drives one-ring Laplacian smoothing (before / after).',
        styles: [{ color: 0xff8c66, flat: true }, { color: 0x6fbf73 }],
    },
    {
        id: 'raycast', file: 'raycast.rs', title: 'Raycasting',
        desc: 'Moller-Trumbore ray-triangle intersection: a ray grid vs. an icosphere.',
        styles: [
            { color: 0x4da3ff, opacity: 0.45 }, { color: 0xffd166, opacity: 0.9 },
            { color: 0x39414e, opacity: 0.5 }, { color: 0xff6b6b, size: 0.07 },
        ],
    },
    {
        id: 'poisson-disk', file: 'poisson_disk.rs', title: 'Poisson Disk Sampling',
        desc: "Bridson's blue-noise sampling next to plain white noise, same point count.",
        styles: [{ color: 0x7fc4ff, size: 0.06 }, { color: 0xff8c66, size: 0.06 }, { color: 0x39414e }],
    },
    {
        id: 'marching-squares', file: 'marching_squares.rs', title: 'Marching Squares',
        desc: 'Iso-contours of a metaball field via the 16-case cell lookup table.',
        styles: [{ color: 0x4da3ff }, { color: 0xffd166 }, { color: 0xff6b6b }],
    },
];

// ---- WASM loading and drawable extraction -----------------------------------

async function loadWasm() {
    // instantiateStreaming needs the application/wasm MIME type; fall back to
    // ArrayBuffer instantiation for servers that do not send it.
    try {
        const { instance } = await WebAssembly.instantiateStreaming(fetch('./geometry_samples.wasm'), {});
        return instance.exports;
    } catch {
        const bytes = await (await fetch('./geometry_samples.wasm')).arrayBuffer();
        const { instance } = await WebAssembly.instantiate(bytes, {});
        return instance.exports;
    }
}

/** Builds sample i in wasm and copies its drawables out of linear memory. */
function buildDrawables(wasm, sampleIndex) {
    const count = wasm.build_sample(sampleIndex);
    const drawables = [];
    for (let d = 0; d < count; d++) {
        const kind = ['mesh', 'lines', 'points'][wasm.drawable_kind(d)];
        // Float64 in wasm -> Float32 for the GPU. Copy immediately: the wasm
        // buffers are only valid until the next build_sample call (and memory
        // growth detaches views).
        const positions = Float32Array.from(new Float64Array(
            wasm.memory.buffer, wasm.drawable_positions_ptr(d), wasm.drawable_positions_len(d)));
        const drawable = { kind, positions };
        if (kind === 'mesh') {
            drawable.indices = new Uint32Array(
                wasm.memory.buffer, wasm.drawable_indices_ptr(d), wasm.drawable_indices_len(d)).slice();
            drawable.normals = Float32Array.from(new Float64Array(
                wasm.memory.buffer, wasm.drawable_normals_ptr(d), wasm.drawable_normals_len(d)));
        }
        drawables.push(drawable);
    }
    return drawables;
}

// ---- Drawable -> Three.js ----------------------------------------------------

function drawableToObject3D(drawable, style = {}) {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(drawable.positions, 3));

    if (drawable.kind === 'mesh') {
        geometry.setIndex(new THREE.BufferAttribute(drawable.indices, 1));
        if (drawable.normals?.length && !style.flat)
            geometry.setAttribute('normal', new THREE.BufferAttribute(drawable.normals, 3));
        else
            geometry.computeVertexNormals();
        return new THREE.Mesh(geometry, new THREE.MeshStandardMaterial({
            color: style.color ?? 0x8899aa,
            flatShading: style.flat ?? false,
            transparent: style.opacity !== undefined && style.opacity < 1,
            opacity: style.opacity ?? 1,
            side: THREE.DoubleSide,
            metalness: 0.1,
            roughness: 0.75,
        }));
    }
    if (drawable.kind === 'lines') {
        return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({
            color: style.color ?? 0xffffff,
            transparent: style.opacity !== undefined && style.opacity < 1,
            opacity: style.opacity ?? 1,
        }));
    }
    return new THREE.Points(geometry, new THREE.PointsMaterial({
        color: style.color ?? 0xffffff,
        size: style.size ?? 0.05,
        sizeAttenuation: true,
    }));
}

function disposeObject3D(root) {
    root.traverse(obj => {
        obj.geometry?.dispose();
        if (Array.isArray(obj.material)) obj.material.forEach(m => m.dispose());
        else obj.material?.dispose();
    });
}

// ---- Viewer -------------------------------------------------------------------

class Viewer {
    constructor(container) {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x0f1115);
        this.content = null;

        this.camera = new THREE.PerspectiveCamera(50, 1, 0.01, 100);
        this.camera.position.set(3, 2, 4);

        // preserveDrawingBuffer keeps the canvas readable for screenshots.
        this.renderer = new THREE.WebGLRenderer({ antialias: true, preserveDrawingBuffer: true });
        this.renderer.setPixelRatio(window.devicePixelRatio);
        container.appendChild(this.renderer.domElement);

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;

        this.scene.add(new THREE.HemisphereLight(0xcfd8e6, 0x30363f, 1.1));
        const key = new THREE.DirectionalLight(0xffffff, 1.6);
        key.position.set(4, 6, 3);
        this.scene.add(key);
        const grid = new THREE.GridHelper(10, 20, 0x2a3140, 0x1c212b);
        grid.position.y = -1.6;
        this.scene.add(grid);

        // Re-checked every frame instead of relying on ResizeObserver, which
        // does not fire in some embedded/headless browsers.
        const resize = () => {
            const w = container.clientWidth, h = container.clientHeight;
            if (w === 0 || h === 0) return;
            const size = this.renderer.getSize(new THREE.Vector2());
            if (size.x === w && size.y === h) return;
            this.camera.aspect = w / h;
            this.camera.updateProjectionMatrix();
            this.renderer.setSize(w, h);
        };
        resize();

        this.renderer.setAnimationLoop(() => {
            resize();
            this.controls.update();
            this.renderer.render(this.scene, this.camera);
        });
    }

    setContent(object) {
        if (this.content) {
            this.scene.remove(this.content);
            disposeObject3D(this.content);
        }
        this.content = object;
        this.scene.add(object);
        this.fitCamera(object);
    }

    fitCamera(object) {
        const box = new THREE.Box3().setFromObject(object);
        const sphere = box.getBoundingSphere(new THREE.Sphere());
        const distance = (sphere.radius / Math.tan((this.camera.fov * Math.PI) / 360)) * 1.25;
        const direction = new THREE.Vector3(0.55, 0.45, 1).normalize();
        this.camera.position.copy(sphere.center.clone().add(direction.multiplyScalar(distance)));
        this.controls.target.copy(sphere.center);
        this.controls.update();
    }
}

// ---- App shell ------------------------------------------------------------------

const wasm = await loadWasm();
document.getElementById('status').textContent = '';

// Sanity check: the wasm registry must match the metadata table.
const idsLen = wasm.sample_ids_len();
const idsPtr = wasm.sample_ids_ptr();
const wasmIds = new TextDecoder()
    .decode(new Uint8Array(wasm.memory.buffer, idsPtr, idsLen)).split(',');
if (wasmIds.join() !== SAMPLES.map(s => s.id).join())
    console.warn('sample registry mismatch', wasmIds);

const viewer = new Viewer(document.getElementById('viewer'));
window.__viewer = viewer; // debugging/testing handle
const list = document.getElementById('sample-list');
const codeHeader = document.getElementById('code-header');
const codeTabs = document.getElementById('code-tabs');
const codeEl = document.getElementById('code');

const sourceCache = new Map();
async function fetchSource(path) {
    if (!sourceCache.has(path))
        sourceCache.set(path, await (await fetch(path)).text());
    return sourceCache.get(path);
}

async function showCode(sample, tab) {
    const tabs = [
        { label: sample.file, path: `../src/samples/${sample.file}` },
        { label: 'plato.rs (generated)', path: '../src/plato.rs' },
    ];
    codeTabs.replaceChildren(...tabs.map((t, i) => {
        const b = document.createElement('button');
        b.textContent = t.label;
        b.classList.toggle('active', i === tab);
        b.onclick = () => showCode(sample, i);
        return b;
    }));
    codeEl.textContent = await fetchSource(tabs[tab].path);
}

let buttons = [];
function select(index) {
    const sample = SAMPLES[index];
    buttons.forEach((b, i) => b.classList.toggle('active', i === index));

    const t0 = performance.now();
    const drawables = buildDrawables(wasm, index);
    const buildMs = performance.now() - t0;

    const group = new THREE.Group();
    drawables.forEach((d, i) => group.add(drawableToObject3D(d, sample.styles[i] ?? {})));
    viewer.setContent(group);

    document.getElementById('status').textContent = `built in Rust/WASM in ${buildMs.toFixed(1)} ms`;
    codeHeader.innerHTML = `<b>${sample.title}</b> — ${sample.desc}`;
    showCode(sample, 0);
}

buttons = SAMPLES.map((sample, i) => {
    const button = document.createElement('button');
    button.innerHTML = `${sample.title}<span class="sample-desc">${sample.desc}</span>`;
    button.onclick = () => select(i);
    list.appendChild(button);
    return button;
});

select(0);
