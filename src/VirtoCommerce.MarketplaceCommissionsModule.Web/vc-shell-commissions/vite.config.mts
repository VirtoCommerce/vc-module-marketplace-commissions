import { resolve, join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { getDynamicModuleConfiguration } from "@vc-shell/config-generator";

const __dirname = dirname(fileURLToPath(import.meta.url));

export default getDynamicModuleConfiguration({
  build: {
    manifest: "manifest.json",
    copyPublicDir: false,
    sourcemap: true,
    minify: false,
    lib: {
      entry: resolve(__dirname, "./src/module/index.ts"),
    },
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
  outDir: "./dist/packages/modules",
  compatibility: {
    framework: "^1.1.0",
  },
});
