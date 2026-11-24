<template>
  <!-- legacy seller-commissions-widget on the seller blade ("sellerDetails"): opens the seller's commission -->
  <VcWidget
    v-if="hasAccess('operator:resources')"
    icon="lucide-percent"
    :title="$t('MARKETPLACE_COMMISSIONS.WIDGETS.SELLER_COMMISSION')"
    :disabled="!seller?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed } from "vue";
import type { SellerCommissionOptions } from "../types";

// the seller shared by the host blade (MarketplaceSellerDetails) via defineBladeContext({ item })
const ctx = injectBladeContext();
const seller = computed(() => ctx.value.item as { id?: string; name?: string } | undefined);
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();

function open() {
  if (!seller.value?.id) return;
  const options: SellerCommissionOptions = { sellerId: seller.value.id, sellerName: seller.value.name };
  openBlade({ name: "MarketplaceCommissionsSellerCommission", options });
}
</script>
