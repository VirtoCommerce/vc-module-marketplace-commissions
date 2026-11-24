import { VcmpFeeClient, VcmpSellerCommissionClient } from "../api_client/virtocommerce.marketplacecommissions";

// The generated clients are plain fetch wrappers (same-origin cookie auth), so one instance each is enough.
export const api = {
  fees: new VcmpFeeClient(),
  sellerCommissions: new VcmpSellerCommissionClient(),
};
