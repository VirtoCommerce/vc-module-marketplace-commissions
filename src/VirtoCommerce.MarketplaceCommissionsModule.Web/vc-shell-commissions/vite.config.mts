import { defineConfig } from "vite";
import { resolve, join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  build: {
    manifest: "manifest.json",
    copyPublicDir: false,
    sourcemap: true,
    minify: false,
    lib: {
      entry: resolve(__dirname, "./src/module/index.ts"),
      fileName: (format, name) => `${name}.js`,
      formats: ["umd"],
      name: "VcShellDynamicModules",
    },

    outDir: join(__dirname, "./dist/packages/modules"),
    rollupOptions: {
      output: {
        globals: {
          vue: "Vue",
          "vue-router": "VueRouter",
          "lodash-es": "_",
          "@vc-shell/framework": "VcShellFramework",
        },
      },
      external: [
        /node_modules/,
        "@vc-shell/framework",
        "vue",
        "vue-router",
        "lodash-es",
      ],
    },
  },
});
