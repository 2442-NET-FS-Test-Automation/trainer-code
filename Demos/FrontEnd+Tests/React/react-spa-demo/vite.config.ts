import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import istanbul from "vite-plugin-istanbul";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    // Istanbul provides coverage instrumentation. As Vite serves each src module
    // for our tests (e2e) it rewrites out code and inserts a bunch of counters. The app
    // runs the same, the counters just aggregate info (who called, how many times called, etc)
    // that Cypress will collect later.
    istanbul({
      include: 'src/*', 
      extension: ['.ts', '.tsx'],
      requireEnv: false
    })
  ],
})
