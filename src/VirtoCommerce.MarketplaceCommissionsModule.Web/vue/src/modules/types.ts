// Blade options; type aliases, not interfaces, so they fit openBlade's Record<string, unknown> options

/** MarketplaceCommissionsOrderFees: the order whose line item commissions are shown */
export type OrderFeesOptions = {
  orderId: string;
  orderNumber?: string;
};
