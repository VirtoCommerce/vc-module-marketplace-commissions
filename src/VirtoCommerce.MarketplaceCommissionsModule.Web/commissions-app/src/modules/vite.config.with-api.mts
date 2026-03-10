import { getDynamicModuleConfiguration } from "@vc-shell/config-generator";

export default getDynamicModuleConfiguration({
  entry: "./src/modules/index.ts",
  compatibility: {
    framework: "^1.2.4",
  },
});
