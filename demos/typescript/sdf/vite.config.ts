// Configures Vite to build and serve the distance-field examples.
import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    port: 5174,
    strictPort: true,
  },
});
