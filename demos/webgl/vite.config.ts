import { resolve } from 'node:path';
import { defineConfig } from 'vite';

// One Vite app, one page per demo: the demos share src/plato and src/shared but
// each has its own entry so a link goes straight to it.
//
// `gratify` is the vendored UI framework under vendor/ (plain ESM, no package),
// aliased here and in tsconfig's `paths` so both Vite and tsc resolve the bare
// specifier the same way.
const page = (name: string): string => resolve(__dirname, `${name}.html`);

export default defineConfig({
  resolve: {
    alias: {
      gratify: resolve(__dirname, 'vendor/gratify/index.js'),
    },
  },
  server: {
    port: 5175,
    strictPort: true,
  },
  build: {
    rollupOptions: {
      input: {
        index: page('index'),
        polyhedra: page('polyhedra'),
        polygons: page('polygons'),
        csg: page('csg'),
        deformers: page('deformers'),
        curves: page('curves'),
        surfaces: page('surfaces'),
        noise: page('noise'),
        colors: page('colors'),
        transforms: page('transforms'),
        marching: page('marching'),
        voxels: page('voxels'),
      },
    },
  },
});
