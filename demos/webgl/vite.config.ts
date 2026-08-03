import { resolve } from 'node:path';
import { defineConfig } from 'vite';

// One Vite app, one page per demo: the demos share src/plato and src/shared but
// each has its own entry so a link goes straight to it.
//
// The Gratify panel is imported straight out of vendor/ by relative path, so
// Vite, tsc and tsx all resolve the same file with no alias to keep in sync.
const page = (name: string): string => resolve(__dirname, `${name}.html`);

export default defineConfig({
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
        lattices: page('lattices'),
        sampling: page('sampling'),
        remeshing: page('remeshing'),
        fea: page('fea'),
        rigidbody: page('rigidbody'),
        cloth: page('cloth'),
      },
    },
  },
});
