// A textured quad, for the demos whose subject is a field rather than a solid.
//
// Noise, colour gamuts and 2D distance fields all want the same thing: sample a
// function over a square, show the answer as pixels. The shell's stage is a
// Three.js scene, so the picture arrives as a `DataTexture` on a unit plane
// under the orthographic camera the planar scenes already use.
//
// Nothing here evaluates anything — the caller's `shade` is what calls the
// generated members, and this file only walks the lattice and packs bytes.

import * as THREE from 'three';

/** A colour a `shade` callback returns: components in 0..1. */
export interface Rgb {
  r: number;
  g: number;
  b: number;
}

export interface RasterOptions {
  /** Samples per side. Cost is quadratic — 256 is a comfortable ceiling. */
  resolution: number;
  /** World size of the quad, centred on the origin. */
  size?: number;
  /** Nearest-neighbour keeps a coarse lattice honest instead of blurring it. */
  smooth?: boolean;
}

/**
 * Sample `shade` over the unit square and hand back the quad showing it.
 * `(u, v)` runs 0..1 with v up, so the picture matches the axes on screen.
 */
export function rasterPlane(
  shade: (u: number, v: number) => Rgb,
  options: RasterOptions,
): THREE.Mesh {
  const { resolution, size = 2, smooth = false } = options;
  const data = new Uint8Array(resolution * resolution * 4);
  for (let j = 0; j < resolution; j++) {
    // Texture row 0 is the bottom of the quad; sample v to match.
    const v = (j + 0.5) / resolution;
    for (let i = 0; i < resolution; i++) {
      const u = (i + 0.5) / resolution;
      const c = shade(u, v);
      const o = (j * resolution + i) * 4;
      data[o] = channel(c.r);
      data[o + 1] = channel(c.g);
      data[o + 2] = channel(c.b);
      data[o + 3] = 255;
    }
  }
  const texture = new THREE.DataTexture(data, resolution, resolution, THREE.RGBAFormat);
  texture.magFilter = smooth ? THREE.LinearFilter : THREE.NearestFilter;
  texture.minFilter = THREE.LinearFilter;
  texture.generateMipmaps = false;
  texture.colorSpace = THREE.SRGBColorSpace;
  texture.needsUpdate = true;

  // The viewer disposes a material's `map` along with the material, so the
  // texture a rebuild leaves behind goes with it.
  return new THREE.Mesh(
    new THREE.PlaneGeometry(size, size),
    new THREE.MeshBasicMaterial({ map: texture, side: THREE.DoubleSide }),
  );
}

/** A 1D ramp as a thin quad — colour scales, gradients, transfer functions. */
export function rasterStrip(
  shade: (t: number) => Rgb,
  samples: number,
  width = 2,
  height = 0.18,
): THREE.Mesh {
  const data = new Uint8Array(samples * 4);
  for (let i = 0; i < samples; i++) {
    const c = shade((i + 0.5) / samples);
    data[i * 4] = channel(c.r);
    data[i * 4 + 1] = channel(c.g);
    data[i * 4 + 2] = channel(c.b);
    data[i * 4 + 3] = 255;
  }
  const texture = new THREE.DataTexture(data, samples, 1, THREE.RGBAFormat);
  texture.magFilter = THREE.LinearFilter;
  texture.minFilter = THREE.LinearFilter;
  texture.generateMipmaps = false;
  texture.colorSpace = THREE.SRGBColorSpace;
  texture.needsUpdate = true;
  return new THREE.Mesh(
    new THREE.PlaneGeometry(width, height),
    new THREE.MeshBasicMaterial({ map: texture, side: THREE.DoubleSide }),
  );
}

/** A greyscale ramp, the default reading of a scalar field. */
export const grey = (t: number): Rgb => ({ r: t, g: t, b: t });

function channel(x: number): number {
  const n = Math.round(x * 255);
  return n < 0 ? 0 : n > 255 ? 255 : n;
}
