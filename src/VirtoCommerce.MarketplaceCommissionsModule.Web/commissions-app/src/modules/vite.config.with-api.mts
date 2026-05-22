import { fileURLToPath } from "node:url";
import path from "node:path";
import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

// vite.config sits at commissions-app/src/modules/, .NET module root is three levels up.
const moduleRoot = path.resolve(__dirname, "../../..");

export default getDynamicModuleConfiguration({
  entry: "./src/modules/index.ts",
  appId: "vendor-portal",
  moduleRoot,
  remoteName: "VirtoCommerce.MarketplaceCommissions",
});
