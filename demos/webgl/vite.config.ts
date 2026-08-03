import { resolve } from 'node:path';
import { defineConfig } from 'vite';

// One Vite app, one page per demo: the four demos share src/plato and src/shared
// but each has its own entry so a link goes straight to it.
export default defineConfig({
  server: {
    port: 5175,
    strictPort: true,
  },
  build: {
    rollupOptions: {
      input: {
        index: resolve(__dirname, 'index.html'),
        polyhedra: resolve(__dirname, 'polyhedra.html'),
        polygons: resolve(__dirname, 'polygons.html'),
        csg: resolve(__dirname, 'csg.html'),
        deformers: resolve(__dirname, 'deformers.html'),
      },
    },
  },
});
