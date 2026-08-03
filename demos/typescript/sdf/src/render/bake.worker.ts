/**
 * Bake worker: evaluates a generated FunctionSdf3D over a z-slab of a dense grid.
 *
 * This is the ONLY place distance values are produced for the 3D viewer, and it
 * runs the same generated library as the main thread: scenes.ts builds the SDF
 * from stdlib code emitted by Plato.CLI --typescript. The GPU never computes a
 * distance — it only displays what is baked here.
 */
import { scenes } from '../scenes.js';
import { Point3D, type FunctionSdf3D } from '../plato/plato.g.js';

export interface BakeRequest {
  sceneId: string;
  params: number[];
  /** Grid resolution per axis. */
  res: number;
  /** Half-extent of the sampled cube, centred on the origin. */
  halfExtent: number;
  /** First z-layer (inclusive) and last (exclusive) of this slab. */
  z0: number;
  z1: number;
  /** Bake generation, echoed back so stale slabs can be discarded. */
  gen: number;
}

export interface BakeResponse {
  gen: number;
  res: number;
  z0: number;
  z1: number;
  /** res * res * (z1 - z0) distances, x-fastest then y then z. */
  values: Float32Array;
}

let cachedKey = '';
let cachedSdf: FunctionSdf3D | null = null;

function getSdf(sceneId: string, params: number[]): FunctionSdf3D {
  const key = `${sceneId}:${params.join(',')}`;
  if (key !== cachedKey) {
    const scene = scenes.find((s) => s.id === sceneId);
    if (!scene) throw new Error(`unknown scene ${sceneId}`);
    cachedKey = key;
    cachedSdf = scene.build(params) as FunctionSdf3D;
  }
  return cachedSdf!;
}

self.onmessage = (e: MessageEvent<BakeRequest>) => {
  const { sceneId, params, res, halfExtent, z0, z1, gen } = e.data;
  const sdf = getSdf(sceneId, params);
  const values = new Float32Array(res * res * (z1 - z0));
  const step = (2 * halfExtent) / (res - 1);
  let i = 0;
  for (let z = z0; z < z1; z++) {
    const pz = -halfExtent + z * step;
    for (let y = 0; y < res; y++) {
      const py = -halfExtent + y * step;
      for (let x = 0; x < res; x++) {
        values[i++] = sdf.Eval(new Point3D(-halfExtent + x * step, py, pz));
      }
    }
  }
  const msg: BakeResponse = { gen, res, z0, z1, values };
  (self as unknown as Worker).postMessage(msg, [values.buffer]);
};
