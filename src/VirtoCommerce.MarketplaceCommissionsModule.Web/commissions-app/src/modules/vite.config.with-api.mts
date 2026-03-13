import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

export default getDynamicModuleConfiguration({
  entry: "./src/modules/index.ts",
  compatibility: {
    framework: "^1.2.4",
  },
});
