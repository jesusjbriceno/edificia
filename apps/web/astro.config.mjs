import { defineConfig } from 'astro/config';
import react from '@astrojs/react';
import tailwindcss from '@tailwindcss/vite';
import node from '@astrojs/node';

// https://astro.build/config
export default defineConfig({
  integrations: [react()],
  vite: {
    plugins: [tailwindcss()],
    // Suppress broken source maps from node_modules (e.g. lucide-react)
    build: {
      rollupOptions: {
        onwarn(warning, warn) {
          if (warning.code === 'SOURCEMAP_ERROR') return;
          warn(warning);
        },
      },
    },
    server: {
      sourcemapIgnoreList: (sourcePath) => sourcePath.includes('node_modules'),
    },
    preview: {
      sourcemapIgnoreList: (sourcePath) => sourcePath.includes('node_modules'),
    },
  },
  output: 'server',
  adapter: node({
    mode: 'standalone',
  }),
});
