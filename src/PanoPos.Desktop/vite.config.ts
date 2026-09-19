import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import config from './desktop.config.json' with { type: 'json' };

export default defineConfig({
  plugins: [react()],
  clearScreen: false,
  server: {
    host: '127.0.0.1',
    port: 1420,
    strictPort: true,
    proxy: {
      '/api/v1': { target: config.apiBaseUrl, changeOrigin: true },
    },
    watch: { ignored: ['**/src-tauri/**'] },
  },
  preview: { host: '127.0.0.1', port: 1421, strictPort: true },
});
