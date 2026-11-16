import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import OrderFeeWidget from "./components/OrderFeeWidget.vue";

// Order details: line items commissions widget (legacy order-fee-widget registered into "customerOrderDetailWidgets").
// The legacy seller commissions widget ("sellerDetails") waits for a Vue seller blade in MarketplaceVendor.
registerExternalWidget({
  id: "MarketplaceCommissionsOrderFees",
  title: "Commissions",
  component: markRaw(OrderFeeWidget),
  targetBlades: ["OrdersDetails"],
  // not on an order that isn't saved yet
  isVisible: (blade) => !!blade?.param,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
