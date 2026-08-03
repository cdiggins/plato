import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import {
  Direction3D,
  Point3D,
  Ray3D,
  Vector3D,
  type FunctionSdf3D,
} from '../plato/plato.g.js';

/**
 * Sphere-trace a generated FunctionSdf3D on the CPU into a CanvasTexture.
 * Uses the library's Eval + GradientAt (Plato NumericGradientAt) — no mirrored GLSL.
 * Progressive tiles keep the UI responsive while the heavy generated bodies run.
 */
export class Raymarch3DViewer {
  readonly renderer: THREE.WebGLRenderer;
  private scene = new THREE.Scene();
  private camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
  private orbitCam = new THREE.PerspectiveCamera(50, 1, 0.1, 100);
  private controls: OrbitControls;
  private mesh: THREE.Mesh;
  private material: THREE.MeshBasicMaterial;
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private texture: THREE.CanvasTexture;
  private image: ImageData;
  private sdf: FunctionSdf3D | null = null;
  private raf = 0;
  private autoSpin = true;
  private generation = 0;
  private tileY = 0;
  private hue = 0.08;
  private ambient = 0.28;
  private readonly sampleW = 220;
  private readonly sampleH = 160;
  private readonly tileRows = 8;
  private readonly budget = 48;
  private readonly tolerance = 0.002;
  private readonly reach = 30;

  constructor(container: HTMLElement) {
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    container.appendChild(this.renderer.domElement);

    this.orbitCam.position.set(2.6, 1.6, 3.2);
    this.controls = new OrbitControls(this.orbitCam, this.renderer.domElement);
    this.controls.enableDamping = true;
    this.controls.target.set(0, 0.15, 0);
    this.controls.minDistance = 1.2;
    this.controls.maxDistance = 12;
    this.controls.addEventListener('start', () => {
      this.autoSpin = false;
    });
    this.controls.addEventListener('change', () => this.invalidate());

    this.canvas = document.createElement('canvas');
    this.canvas.width = this.sampleW;
    this.canvas.height = this.sampleH;
    this.ctx = this.canvas.getContext('2d', { willReadFrequently: true })!;
    this.image = this.ctx.createImageData(this.sampleW, this.sampleH);
    this.texture = new THREE.CanvasTexture(this.canvas);
    this.texture.colorSpace = THREE.SRGBColorSpace;
    this.texture.minFilter = THREE.LinearFilter;
    this.texture.magFilter = THREE.LinearFilter;

    this.material = new THREE.MeshBasicMaterial({ map: this.texture });
    this.mesh = new THREE.Mesh(new THREE.PlaneGeometry(2, 2), this.material);
    this.scene.add(this.mesh);

    const resize = () => {
      const w = container.clientWidth;
      const h = container.clientHeight;
      if (w === 0 || h === 0) return;
      this.renderer.setSize(w, h, false);
      this.orbitCam.aspect = w / h;
      this.orbitCam.updateProjectionMatrix();
      this.invalidate();
    };
    new ResizeObserver(resize).observe(container);
    resize();

    const tick = () => {
      // Advance the orbit only between complete frames — progressive tiles never
      // finish if every animation frame invalidates the buffer.
      if (this.autoSpin && this.tileY >= this.sampleH) {
        const xz = Math.hypot(this.orbitCam.position.x, this.orbitCam.position.z);
        const ang = Math.atan2(this.orbitCam.position.z, this.orbitCam.position.x) + 0.08;
        this.orbitCam.position.x = Math.cos(ang) * xz;
        this.orbitCam.position.z = Math.sin(ang) * xz;
        this.orbitCam.lookAt(this.controls.target);
        this.invalidate();
      }
      this.controls.update();
      this.traceTiles();
      this.renderer.render(this.scene, this.camera);
      this.raf = requestAnimationFrame(tick);
    };
    this.raf = requestAnimationFrame(tick);
  }

  setSdf(sdf: FunctionSdf3D): void {
    this.sdf = sdf;
    this.autoSpin = true;
    this.orbitCam.position.set(2.6, 1.6, 3.2);
    this.controls.target.set(0, 0.15, 0);
    this.invalidate();
  }

  setHue(h: number): void {
    this.hue = h;
    this.invalidate();
  }

  setAmbient(a: number): void {
    this.ambient = a;
    this.invalidate();
  }

  private invalidate(): void {
    this.generation++;
    this.tileY = 0;
  }

  private traceTiles(): void {
    if (!this.sdf || this.tileY >= this.sampleH) return;
    const gen = this.generation;
    const y0 = this.tileY;
    const y1 = Math.min(this.sampleH, y0 + this.tileRows);
    this.traceRows(y0, y1);
    if (gen !== this.generation) return;
    this.ctx.putImageData(this.image, 0, 0);
    this.texture.needsUpdate = true;
    this.tileY = y1;
  }

  private traceRows(y0: number, y1: number): void {
    const sdf = this.sdf!;
    const { sampleW: w, sampleH: h, budget, tolerance, reach, hue, ambient } = this;
    const aspect = w / h;
    const fov = (this.orbitCam.fov * Math.PI) / 180;
    const fl = 1 / Math.tan(fov * 0.5);
    this.orbitCam.updateMatrixWorld();
    const e = this.orbitCam.matrixWorld.elements;
    // Camera basis columns: right, up, back
    const right = new Vector3D(e[0], e[1], e[2]);
    const up = new Vector3D(e[4], e[5], e[6]);
    const back = new Vector3D(e[8], e[9], e[10]);
    const origin = new Point3D(
      this.orbitCam.position.x,
      this.orbitCam.position.y,
      this.orbitCam.position.z,
    );
    const light = new Vector3D(0.4, 0.85, 0.35).Normalize();
    const data = this.image.data;

    for (let y = y0; y < y1; y++) {
      const v = 1 - (y + 0.5) / h;
      const qy = (v * 2 - 1);
      for (let x = 0; x < w; x++) {
        const qx = (((x + 0.5) / w) * 2 - 1) * aspect;
        const dir = right
          .Multiply(qx)
          .Add(up.Multiply(qy))
          .Add(back.Multiply(-fl))
          .Normalize();
        const ray = new Ray3D(origin, new Direction3D(dir));
        const hit = march(sdf, ray, budget, tolerance, reach);
        const rgb = shade(sdf, hit, dir, light, hue, ambient);
        const i = (y * w + x) * 4;
        data[i] = rgb[0];
        data[i + 1] = rgb[1];
        data[i + 2] = rgb[2];
        data[i + 3] = 255;
      }
    }
  }

  dispose(): void {
    cancelAnimationFrame(this.raf);
    this.controls.dispose();
    this.material.dispose();
    this.texture.dispose();
    this.mesh.geometry.dispose();
    this.renderer.dispose();
    this.renderer.domElement.remove();
  }
}

interface Hit {
  ok: boolean;
  t: number;
  position: Point3D;
}

/** Iterative sphere tracing against FunctionSdf3D.Eval (same algorithm as Plato MarchFrom). */
function march(
  sdf: FunctionSdf3D,
  ray: Ray3D,
  budget: number,
  tolerance: number,
  reach: number,
): Hit {
  let t = 0;
  for (let i = 0; i < budget; i++) {
    const position = ray.PointAt(t);
    const distance = sdf.Eval(position);
    if (Math.abs(distance) <= tolerance) return { ok: true, t, position };
    t += distance;
    if (t > reach) break;
  }
  return { ok: false, t: 0, position: ray.Origin };
}

function shade(
  sdf: FunctionSdf3D,
  hit: Hit,
  rd: Vector3D,
  light: Vector3D,
  hue: number,
  ambient: number,
): [number, number, number] {
  if (!hit.ok) {
    const sky = 0.55 + 0.45 * -rd.Y;
    const c = 18 * sky;
    return [Math.round(c), Math.round(c * 1.05), Math.round(c * 1.2)];
  }
  const n = sdf.GradientAt(hit.position).Normalize();
  const diff = Math.max(n.Dot(light), 0);
  const rim = Math.pow(1 - Math.max(n.Dot(rd.Multiply(-1)), 0), 2.5);
  const pal = palette(hue + 0.15 * n.Y);
  const lit = [
    pal[0] * (ambient + (1 - ambient) * diff) + 0.35 * rim * 0.35,
    pal[1] * (ambient + (1 - ambient) * diff) + 0.35 * rim * 0.35,
    pal[2] * (ambient + (1 - ambient) * diff) + 0.35 * rim * 0.35,
  ];
  const fog = 1 - Math.exp(-0.012 * hit.t * hit.t);
  return [
    Math.round(lerp(lit[0], 0.06, fog) * 255),
    Math.round(lerp(lit[1], 0.07, fog) * 255),
    Math.round(lerp(lit[2], 0.09, fog) * 255),
  ];
}

function palette(t: number): [number, number, number] {
  return [
    0.5 + 0.5 * Math.cos(6.28318 * (t + 0.0)),
    0.5 + 0.5 * Math.cos(6.28318 * (t + 0.33)),
    0.5 + 0.5 * Math.cos(6.28318 * (t + 0.67)),
  ];
}

function lerp(a: number, b: number, t: number): number {
  return a + (b - a) * t;
}
