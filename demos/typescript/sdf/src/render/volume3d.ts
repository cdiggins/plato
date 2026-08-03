import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

/**
 * GPU raymarcher over a CPU-baked distance field.
 *
 * Truth boundary: every distance value comes from the generated library —
 * bake.worker.ts runs FunctionSdf3D.Eval (stdlib via Plato.CLI --typescript)
 * over a dense grid, and this viewer sphere-traces the resulting 3D texture
 * with trilinear interpolation. The shader below contains no SDF formulas,
 * only texture sampling, a step loop, and shading.
 *
 * Resolution ladder keeps the UI live: a coarse bake displays immediately and
 * finer bakes replace it as the workers finish.
 */
export class Volume3DViewer {
  readonly renderer: THREE.WebGLRenderer;
  private scene = new THREE.Scene();
  private camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
  private orbitCam = new THREE.PerspectiveCamera(50, 1, 0.1, 100);
  private controls: OrbitControls;
  private mesh: THREE.Mesh;
  private material: THREE.ShaderMaterial;
  private texture: THREE.Data3DTexture | null = null;
  private workers: Worker[] = [];
  private gen = 0;
  private autoSpin = true;
  private raf = 0;
  private sceneId = '';
  private params: number[] = [];
  private rebakeTimer = 0;
  private pending = new Map<number, { values: Float32Array<ArrayBuffer>; res: number; remaining: number }>();
  private displayedGen = -1;
  private displayedRes = 0;
  private readonly halfExtent = 1.6;
  private readonly ladder = [32, 64, 96];
  /** Called with the active grid resolution whenever a bake lands. */
  onResolution: ((res: number) => void) | null = null;

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

    this.material = new THREE.ShaderMaterial({
      glslVersion: THREE.GLSL3,
      uniforms: {
        uField: { value: null },
        uHasField: { value: 0 },
        uCamPos: { value: new THREE.Vector3() },
        uCamBasis: { value: new THREE.Matrix3() },
        uFocal: { value: 1 },
        uAspect: { value: 1 },
        uHalfExtent: { value: this.halfExtent },
        uVoxel: { value: (2 * this.halfExtent) / 31 },
        uHue: { value: 0.08 },
        uAmbient: { value: 0.28 },
      },
      vertexShader: VERT,
      fragmentShader: FRAG,
    });
    this.mesh = new THREE.Mesh(new THREE.PlaneGeometry(2, 2), this.material);
    this.scene.add(this.mesh);

    const workerCount = Math.min(navigator.hardwareConcurrency || 4, 6);
    for (let i = 0; i < workerCount; i++) {
      const w = new Worker(new URL('./bake.worker.ts', import.meta.url), { type: 'module' });
      w.onmessage = (e) => this.onSlab(e.data);
      this.workers.push(w);
    }

    const resize = () => {
      const w = container.clientWidth;
      const h = container.clientHeight;
      if (w === 0 || h === 0) return;
      this.renderer.setSize(w, h, false);
      this.orbitCam.aspect = w / h;
      this.orbitCam.updateProjectionMatrix();
      this.material.uniforms.uAspect.value = w / h;
    };
    new ResizeObserver(resize).observe(container);
    resize();

    const tick = () => {
      if (this.autoSpin) {
        const xz = Math.hypot(this.orbitCam.position.x, this.orbitCam.position.z);
        const ang = Math.atan2(this.orbitCam.position.z, this.orbitCam.position.x) + 0.004;
        this.orbitCam.position.x = Math.cos(ang) * xz;
        this.orbitCam.position.z = Math.sin(ang) * xz;
        this.orbitCam.lookAt(this.controls.target);
      }
      this.controls.update();
      this.renderFrame();
      this.raf = requestAnimationFrame(tick);
    };
    this.raf = requestAnimationFrame(tick);
  }

  /** One synchronous frame — lets automation render while rAF is paused. */
  renderFrame(): void {
    this.orbitCam.updateMatrixWorld();
    const e = this.orbitCam.matrixWorld.elements;
    this.material.uniforms.uCamBasis.value.set(
      e[0], e[4], e[8],
      e[1], e[5], e[9],
      e[2], e[6], e[10],
    );
    this.material.uniforms.uCamPos.value.copy(this.orbitCam.position);
    const fov = (this.orbitCam.fov * Math.PI) / 180;
    this.material.uniforms.uFocal.value = 1 / Math.tan(fov * 0.5);
    this.renderer.render(this.scene, this.camera);
  }

  setScene(sceneId: string, params: number[]): void {
    const isNewScene = sceneId !== this.sceneId;
    this.sceneId = sceneId;
    this.params = [...params];
    if (isNewScene) {
      this.autoSpin = true;
      this.orbitCam.position.set(2.6, 1.6, 3.2);
      this.controls.target.set(0, 0.15, 0);
      this.bake();
    } else {
      // Param drag: debounce so we bake the settled value, not every tick.
      clearTimeout(this.rebakeTimer);
      this.rebakeTimer = window.setTimeout(() => this.bake(), 60);
    }
  }

  setHue(h: number): void {
    this.material.uniforms.uHue.value = h;
  }

  setAmbient(a: number): void {
    this.material.uniforms.uAmbient.value = a;
  }

  private bake(): void {
    this.gen++;
    this.pending.clear();
    for (const res of this.ladder) {
      const gen = this.gen * 100 + res;
      this.pending.set(gen, { values: new Float32Array(res * res * res), res, remaining: this.workers.length });
      const slab = Math.ceil(res / this.workers.length);
      for (let i = 0; i < this.workers.length; i++) {
        const z0 = i * slab;
        const z1 = Math.min(res, z0 + slab);
        if (z0 >= z1) {
          const p = this.pending.get(gen)!;
          p.remaining--;
          continue;
        }
        this.workers[i].postMessage({
          sceneId: this.sceneId,
          params: this.params,
          res,
          halfExtent: this.halfExtent,
          z0,
          z1,
          gen,
        });
      }
    }
  }

  private onSlab(msg: { gen: number; res: number; z0: number; z1: number; values: Float32Array }): void {
    const bake = this.pending.get(msg.gen);
    if (!bake) return; // stale generation
    bake.values.set(msg.values, msg.z0 * msg.res * msg.res);
    bake.remaining--;
    if (bake.remaining > 0) return;
    this.pending.delete(msg.gen);
    if (Math.floor(msg.gen / 100) !== this.gen) return; // stale generation
    // Within the current generation, never replace a finer bake with a coarser one.
    if (this.displayedGen === this.gen && bake.res <= this.displayedRes) return;
    this.displayedGen = this.gen;
    this.displayedRes = bake.res;
    this.upload(bake.values, bake.res);
  }

  private upload(values: Float32Array<ArrayBuffer>, res: number): void {
    this.texture?.dispose();
    const tex = new THREE.Data3DTexture(values, res, res, res);
    tex.format = THREE.RedFormat;
    tex.type = THREE.FloatType;
    tex.minFilter = THREE.LinearFilter;
    tex.magFilter = THREE.LinearFilter;
    tex.wrapS = tex.wrapT = tex.wrapR = THREE.ClampToEdgeWrapping;
    tex.unpackAlignment = 1;
    tex.needsUpdate = true;
    this.texture = tex;
    this.material.uniforms.uField.value = tex;
    this.material.uniforms.uHasField.value = 1;
    this.material.uniforms.uVoxel.value = (2 * this.halfExtent) / (res - 1);
    this.onResolution?.(res);
  }

  dispose(): void {
    cancelAnimationFrame(this.raf);
    clearTimeout(this.rebakeTimer);
    for (const w of this.workers) w.terminate();
    this.controls.dispose();
    this.texture?.dispose();
    this.material.dispose();
    this.mesh.geometry.dispose();
    this.renderer.dispose();
    this.renderer.domElement.remove();
  }
}

const VERT = /* glsl */ `
out vec2 vNdc;
void main() {
  vNdc = position.xy;
  gl_Position = vec4(position.xy, 0.0, 1.0);
}
`;

// Display only: samples the baked field, no SDF formulas.
const FRAG = /* glsl */ `
precision highp float;
precision highp sampler3D;

uniform sampler3D uField;
uniform int uHasField;
uniform vec3 uCamPos;
uniform mat3 uCamBasis;
uniform float uFocal;
uniform float uAspect;
uniform float uHalfExtent;
uniform float uVoxel;
uniform float uHue;
uniform float uAmbient;

in vec2 vNdc;
out vec4 fragColor;

float sampleField(vec3 p) {
  vec3 t = (p + uHalfExtent) / (2.0 * uHalfExtent);
  return texture(uField, t).r;
}

vec2 boxSpan(vec3 ro, vec3 rd) {
  vec3 inv = 1.0 / rd;
  vec3 t0 = (vec3(-uHalfExtent) - ro) * inv;
  vec3 t1 = (vec3(uHalfExtent) - ro) * inv;
  vec3 tn = min(t0, t1);
  vec3 tf = max(t0, t1);
  return vec2(max(max(tn.x, tn.y), tn.z), min(min(tf.x, tf.y), tf.z));
}

vec3 gradientAt(vec3 p) {
  float e = uVoxel;
  return vec3(
    sampleField(p + vec3(e, 0, 0)) - sampleField(p - vec3(e, 0, 0)),
    sampleField(p + vec3(0, e, 0)) - sampleField(p - vec3(0, e, 0)),
    sampleField(p + vec3(0, 0, e)) - sampleField(p - vec3(0, 0, e)));
}

vec3 palette(float t) {
  return 0.5 + 0.5 * cos(6.28318 * (t + vec3(0.0, 0.33, 0.67)));
}

vec3 sky(vec3 rd) {
  float s = 0.55 + 0.45 * -rd.y;
  return vec3(18.0 * s, 18.9 * s, 21.6 * s) / 255.0;
}

void main() {
  vec3 rd = normalize(uCamBasis * vec3(vNdc.x * uAspect, vNdc.y, -uFocal));
  if (uHasField == 0) {
    fragColor = vec4(sky(rd), 1.0);
    return;
  }
  vec2 span = boxSpan(uCamPos, rd);
  float t = max(span.x, 0.0);
  bool hit = false;
  float tol = 0.6 * uVoxel;
  for (int i = 0; i < 160; i++) {
    if (t > span.y) break;
    vec3 p = uCamPos + rd * t;
    float d = sampleField(p);
    if (d <= tol) { hit = true; break; }
    // The interpolated field under-estimates near voxel edges; step conservatively.
    t += max(d * 0.9, 0.35 * uVoxel);
  }
  if (!hit) {
    fragColor = vec4(sky(rd), 1.0);
    return;
  }
  vec3 p = uCamPos + rd * t;
  vec3 n = normalize(gradientAt(p));
  vec3 light = normalize(vec3(0.4, 0.85, 0.35));
  float diff = max(dot(n, light), 0.0);
  float rim = pow(1.0 - max(dot(n, -rd), 0.0), 2.5);
  vec3 pal = palette(uHue + 0.15 * n.y);
  vec3 lit = pal * (uAmbient + (1.0 - uAmbient) * diff) + 0.1225 * rim;
  float fog = 1.0 - exp(-0.012 * t * t);
  fragColor = vec4(mix(lit, vec3(0.06, 0.07, 0.09), fog), 1.0);
}
`;
