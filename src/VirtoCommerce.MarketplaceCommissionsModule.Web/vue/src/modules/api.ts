import { VcmpFeeClient } from "../api_client/virtocommerce.marketplacecommissions";

// The generated clients are plain fetch wrappers (same-origin cookie auth), so one instance each is enough.
// VcmpSellerCommissionClient is not used yet: the seller commissions widget waits for a Vue seller blade.
export const api = {
  fees: new VcmpFeeClient(),
};
