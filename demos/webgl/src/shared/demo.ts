import type * as THREE from 'three';

/** A slider or checkbox in the sidebar. `key` is the name `build` reads back. */
export interface Control {
  key: string;
  label: string;
  kind: 'slider' | 'toggle' | 'select';
  min?: number;
  max?: number;
  step?: number;
  /** Option labels for `kind: 'select'`; the value is the chosen index. */
  options?: string[];
  def: number;
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
   * Build the visual. Called on selection and on every parameter change, so it
   * must be pure — the viewer disposes the previous result.
   */
  build(params: Params): THREE.Object3D;
}

/** The catalog a demo page hands to `mountDemo`. */
export interface Demo {
  title: string;
  /** Shown under the title; name the .plato files the demo draws on. */
  subtitle: string;
  scenes: Scene[];
}
