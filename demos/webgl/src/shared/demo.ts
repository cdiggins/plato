import type * as THREE from 'three';
import type { ViewerOptions } from './viewer.js';

/**
 * One control in the sidebar. `key` is the name `build` reads back — except for
 * `kind: 'color'`, which owns three parameters instead of one (see `colorDef`).
 */
export interface Control {
  key: string;
  label: string;
  kind: 'slider' | 'toggle' | 'select' | 'color';
  min?: number;
  max?: number;
  step?: number;
  /** Option labels for `kind: 'select'`; the value is the chosen index. */
  options?: string[];
  def: number;
  /**
   * `kind: 'color'` only: the starting hue, saturation and value, each 0..1.
   * The picker writes them back as `${key}H`, `${key}S` and `${key}V`, so a
   * control keyed `tint` is read as `params.tintH`, `params.tintS`, `params.tintV`.
   * `def` is ignored for this kind.
   */
  colorDef?: [number, number, number];
}

/** The parameters a `color` control contributes, for a control's key. */
export function colorKeys(key: string): [string, string, string] {
  return [`${key}H`, `${key}S`, `${key}V`];
}

export type Params = Record<string, number>;

/** One entry in a demo's sidebar list. */
export interface Scene {
  id: string;
  title: string;
  /** One or two sentences: what the Plato source does, not what Three.js does. */
  description: string;
  /** The generated members this scene calls, for the source panel. */
  plato: string[];
  controls?: Control[];
  /**
   * Camera and stage settings for this scene, merged over the page's. A page
   * that mixes planar and spatial scenes sets `orthographic` here rather than
   * settling for one camera; changing these rebuilds the viewer.
   */
  viewer?: ViewerOptions;
  /**
   * Build the visual. Called on selection and on every parameter change, so it
   * must be pure — the viewer disposes the previous result.
   */
  build(params: Params): THREE.Object3D;
  /**
   * A line of live text under the description — counts, measured values,
   * whatever this build just computed. Called after `build`, on every rebuild.
   */
  status?(params: Params): string;
}

/** The catalog a demo page hands to `mountDemo`. */
export interface Demo {
  title: string;
  /** Shown under the title; name the .plato files the demo draws on. */
  subtitle: string;
  scenes: Scene[];
}
