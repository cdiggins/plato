import * as THREE from 'three';
import { Point2D, type FunctionSdf2D } from '../plato/plato.g.js';

/** Sample a generated FunctionSdf2D into a CanvasTexture shown on a fullscreen quad. */
export class Field2DViewer {
  readonly renderer: THREE.WebGLRenderer;
  private scene = new THREE.Scene();
  private camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
  private mesh: THREE.Mesh;
  private material: THREE.MeshBasicMaterial;
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private texture: THREE.CanvasTexture;
  private sdf: FunctionSdf2D | null = null;
  private zoom = 1.35;
  private raf = 0;
  private dirty = true;
  private readonly sampleW = 360;
  private readonly sampleH = 360;

  constructor(container: HTMLElement) {
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    container.appendChild(this.renderer.domElement);

    this.canvas = document.createElement('canvas');
    this.canvas.width = this.sampleW;
    this.canvas.height = this.sampleH;
    this.ctx = this.canvas.getContext('2d', { willReadFrequently: true })!;
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
    };
    new ResizeObserver(resize).observe(container);
    resize();

    const tick = () => {
      if (this.dirty) {
        this.rasterize();
        this.dirty = false;
      }
      this.renderer.render(this.scene, this.camera);
      this.raf = requestAnimationFrame(tick);
    };
    this.raf = requestAnimationFrame(tick);
  }

  setSdf(sdf: FunctionSdf2D): void {
    this.sdf = sdf;
    this.dirty = true;
  }

  setZoom(z: number): void {
    this.zoom = z;
    this.dirty = true;
  }

  private rasterize(): void {
    if (!this.sdf) return;
    const { sampleW: w, sampleH: h, zoom, sdf } = this;
    const img = this.ctx.createImageData(w, h);
    const data = img.data;
    const aspect = w / h;

    for (let y = 0; y < h; y++) {
      const py = ((1 - (y + 0.5) / h) * 2 - 1) * zoom;
      for (let x = 0; x < w; x++) {
        const px = (((x + 0.5) / w) * 2 - 1) * zoom * aspect;
        const d = sdf.Eval(new Point2D(px, py));
        const i = (y * w + x) * 4;
        const rgb = fieldColor(d);
        data[i] = rgb[0];
        data[i + 1] = rgb[1];
        data[i + 2] = rgb[2];
        data[i + 3] = 255;
      }
    }
    this.ctx.putImageData(img, 0, 0);
    this.texture.needsUpdate = true;
  }

  dispose(): void {
    cancelAnimationFrame(this.raf);
    this.material.dispose();
    this.texture.dispose();
    this.mesh.geometry.dispose();
    this.renderer.dispose();
    this.renderer.domElement.remove();
  }
}

function fieldColor(d: number): [number, number, number] {
  const bands = Math.abs((d * 4) % 1 - 0.5);
  const contour = smoothstep(0, 0.04, Math.abs(d));
  const inside = d < 0;
  let r = inside ? 26 : 196;
  let g = inside ? 74 : 92;
  let b = inside ? 122 : 38;
  const shade = 0.55 + 0.45 * bands;
  r = Math.round(r * shade);
  g = Math.round(g * shade);
  b = Math.round(b * shade);
  // White zero level set
  r = Math.round(lerp(255, r, contour));
  g = Math.round(lerp(255, g, contour));
  b = Math.round(lerp(255, b, contour));
  const fog = smoothstep(2.2, 0.4, Math.abs(d));
  const mix = 0.35 + 0.65 * fog;
  return [
    Math.round(lerp(15, r, mix)),
    Math.round(lerp(18, g, mix)),
    Math.round(lerp(23, b, mix)),
  ];
}

function lerp(a: number, b: number, t: number): number {
  return a + (b - a) * t;
}

function smoothstep(e0: number, e1: number, x: number): number {
  const t = Math.min(Math.max((x - e0) / (e1 - e0), 0), 1);
  return t * t * (3 - 2 * t);
}
