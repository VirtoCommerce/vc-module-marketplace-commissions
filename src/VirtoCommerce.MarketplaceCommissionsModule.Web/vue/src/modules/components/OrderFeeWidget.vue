<template>
  <!-- legacy order-fee-widget on the order blade ("customerOrderDetailWidgets"): opens the line items' commissions -->
  <VcWidget
    v-if="hasAccess('operator:resources')"
    icon="lucide-percent"
    :title="$t('MARKETPLACE_COMMISSIONS.WIDGETS.ORDER_FEES')"
    :disabled="!order?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed } from "vue";
import type { OrderFeesOptions } from "../types";

// the order shared by the host blade (OrdersDetails) via defineBladeContext({ item })
const ctx = injectBladeContext();
const order = computed(() => ctx.value.item as { id?: string; number?: string } | undefined);
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();

function open() {
  if (!order.value?.id) return;
  const options: OrderFeesOptions = { orderId: order.value.id, orderNumber: order.value.number };
  openBlade({ name: "MarketplaceCommissionsOrderFees", options });
}
</script>
