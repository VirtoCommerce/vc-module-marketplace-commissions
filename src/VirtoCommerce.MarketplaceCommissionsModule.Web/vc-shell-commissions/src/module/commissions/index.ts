import { createDynamicAppModule } from "@vc-shell/framework";
import overrides from "./schemaOverride";
import { useCommissions } from "./composables";
import * as locales from "./locales";

export default createDynamicAppModule({
  overrides,
  mixin: { SellerDetails: [useCommissions] },
  locales,
});
