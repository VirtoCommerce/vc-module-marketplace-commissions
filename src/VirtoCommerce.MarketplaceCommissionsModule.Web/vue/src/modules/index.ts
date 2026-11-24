import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import OrderFeeWidget from "./components/OrderFeeWidget.vue";
import SellerCommissionWidget from "./components/SellerCommissionWidget.vue";

// Order details: line items commissions widget (legacy order-fee-widget registered into "customerOrderDetailWidgets").
registerExternalWidget({
  id: "MarketplaceCommissionsOrderFees",
  title: "Commissions",
  component: markRaw(OrderFeeWidget),
  targetBlades: ["OrdersDetails"],
  // not on an order that isn't saved yet
  isVisible: (blade) => !!blade?.param,
});

// Seller details (MarketplaceVendor): the seller's static commission (legacy seller-commissions-widget in "sellerDetails").
registerExternalWidget({
  id: "MarketplaceCommissionsSellerCommission",
  title: "Commissions",
  component: markRaw(SellerCommissionWidget),
  targetBlades: ["MarketplaceSellerDetails"],
  // not on a seller that isn't saved yet
  isVisible: (blade) => !!blade?.param,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
