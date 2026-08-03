// Shared Three.js stage: orbit camera, two lights, a ground grid, and a single
// swappable Object3D. Every demo page mounts one of these; nothing here knows
// about Plato.

import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

export interface ViewerOptions {
  /** Distance the camera starts at. */
  distance?: number;
  grid?: boolean;
  /** Auto-rotate until the user drags. */
  spin?: boolean;
  /** Look straight down -Z, for the 2D demos. */
  orthographic?: boolean;
}

export class Viewer {
  readonly scene = new THREE.Scene();
  readonly camera: THREE.PerspectiveCamera | THREE.OrthographicCamera;
  private readonly renderer: THREE.WebGLRenderer;
  private readonly controls: OrbitControls;
  private readonly container: HTMLElement;
  private readonly grid?: THREE.GridHelper;
  private content: THREE.Object3D | null = null;
  private spin: boolean;
  private raf = 0;

  constructor(container: HTMLElement, options: ViewerOptions = {}) {
    const { distance = 4.2, grid = true, spin = true, orthographic = false } = options;
    this.container = container;
    this.spin = spin;

    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.setClearColor(0x0f1115, 1);
    container.appendChild(this.renderer.domElement);

    if (orthographic) {
      const half = distance / 2;
      this.camera = new THREE.OrthographicCamera(-half, half, half, -half, -100, 100);
      this.camera.position.set(0, 0, 10);
    } else {
      this.camera = new THREE.PerspectiveCamera(45, 1, 0.05, 200);
      this.camera.position.set(distance * 0.62, distance * 0.5, distance * 0.78);
    }

    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enableDamping = true;
    this.controls.enableRotate = !orthographic;
    this.controls.addEventListener('start', () => {
      this.spin = false;
    });

    const key = new THREE.DirectionalLight(0xffffff, 2.1);
    key.position.set(2.5, 4, 3);
    const fill = new THREE.DirectionalLight(0x9db8ff, 0.7);
    fill.position.set(-3, -1.5, -2);
    this.scene.add(key, fill, new THREE.AmbientLight(0xffffff, 0.45));

    if (grid && !orthographic) {
      this.grid = new THREE.GridHelper(10, 20, 0x2a3140, 0x1b2027);
      this.grid.position.y = -1.6;
      this.scene.add(this.grid);
    }

    new ResizeObserver(() => this.resize()).observe(container);
    this.resize();
    this.loop();
  }

  /** Replace the displayed object, disposing whatever was there. */
  show(object: THREE.Object3D): void {
    if (this.content) {
      this.scene.remove(this.content);
      disposeTree(this.content);
    }
    this.content = object;
    this.scene.add(object);
  }

  setGridVisible(visible: boolean): void {
    if (this.grid) this.grid.visible = visible;
  }

  private resize(): void {
    const width = this.container.clientWidth;
    const height = this.container.clientHeight;
    if (width === 0 || height === 0) return;
    this.renderer.setSize(width, height, false);
    const aspect = width / height;
    if (this.camera instanceof THREE.PerspectiveCamera) {
      this.camera.aspect = aspect;
    } else {
      const half = (this.camera.top - this.camera.bottom) / 2;
      this.camera.left = -half * aspect;
      this.camera.right = half * aspect;
    }
    this.camera.updateProjectionMatrix();
  }

  private loop = (): void => {
    this.raf = requestAnimationFrame(this.loop);
    if (this.spin && this.content) this.content.rotation.y += 0.004;
    this.controls.update();
    this.renderer.render(this.scene, this.camera);
  };

  dispose(): void {
    cancelAnimationFrame(this.raf);
    this.controls.dispose();
    this.renderer.dispose();
  }
}

function disposeTree(root: THREE.Object3D): void {
  root.traverse(node => {
    const withGeometry = node as Partial<THREE.Mesh>;
    withGeometry.geometry?.dispose();
    const material = withGeometry.material;
    if (Array.isArray(material)) material.forEach(m => m.dispose());
    else material?.dispose();
  });
}

/** The palette the demos share, so the four pages look like one product. */
export const palette = {
  surface: 0x4f8fd6,
  surfaceAlt: 0xe0894a,
  accent: 0x63d6a8,
  edge: 0x0d1117,
  line: 0x8fd0ff,
};

export function surfaceMaterial(color = palette.surface): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    color,
    roughness: 0.42,
    metalness: 0.08,
    flatShading: true,
    side: THREE.DoubleSide,
  });
}

export function edgeMaterial(color = palette.edge): THREE.LineBasicMaterial {
  return new THREE.LineBasicMaterial({ color });
}
