<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.ORDER_FEES.TITLE')"
    :subtitle="options?.orderNumber"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      :items="items"
      :state-key="'MARKETPLACE_COMMISSIONS_ORDER_FEES'"
      :empty-state="{ icon: 'lucide-percent', title: $t('MARKETPLACE_COMMISSIONS.ORDER_FEES.EMPTY') }"
    >
      <VcColumn
        id="name"
        :title="$t('MARKETPLACE_COMMISSIONS.ORDER_FEES.LINE_ITEM')"
        always-visible
      />
      <VcColumn
        id="commissionName"
        :title="$t('MARKETPLACE_COMMISSIONS.ORDER_FEES.COMMISSION_NAME')"
      />
      <VcColumn
        id="fee"
        :title="$t('MARKETPLACE_COMMISSIONS.ORDER_FEES.FEE')"
      >
        <template #body="{ data }">
          <span
            v-if="data.fee"
            class="mc-fee"
            >{{ data.fee }} <span class="mc-fee__currency">{{ data.currency }}</span></span
          >
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { getOrder, type OrderLineItem } from "../lookups";
import type { OrderFeesOptions } from "../types";

// legacy order-fee-list: the commission of each line item of an order (read-only)
defineBlade({ url: "/order-commissions", name: "MarketplaceCommissionsOrderFees", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<OrderFeesOptions>();
const { showError } = usePopup();

type Row = OrderLineItem & { commissionName?: string };
const items = ref<Row[]>([]);
const loading = ref(false);

// loads the order itself rather than taking its items as options, so a restored URL shows them too
async function load() {
  if (!options.value?.orderId) return;
  loading.value = true;
  try {
    const order = await getOrder(options.value.orderId);
    // the commission applied to a line item is described by its first fee detail (legacy commissionName)
    items.value = (order?.items ?? []).map((x) => ({ ...x, commissionName: x.feeDetails?.[0]?.description }));
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
]);

onMounted(load);
</script>

<style scoped>
.mc-fee {
  font-weight: 600;
}

.mc-fee__currency {
  font-size: 12px;
  font-weight: 400;
  color: var(--color-neutral-500);
}
</style>
