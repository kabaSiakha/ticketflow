import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// En développement (npm run dev), les appels /api sont relayés vers le backend.
// En production, c'est nginx qui joue ce rôle (voir nginx.conf).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': process.env.API_URL || 'http://localhost:5000',
    },
  },
});
