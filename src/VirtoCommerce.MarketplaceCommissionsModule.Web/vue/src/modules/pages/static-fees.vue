<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'MARKETPLACE_COMMISSIONS_STATIC_FEES'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-percent', title: $t('MARKETPLACE_COMMISSIONS.STATIC_LIST.EMPTY') }"
      @row-click="(e: { data: CommissionFee }) => edit(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="isDefault"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.DEFAULT')"
        sortable
        width="120px"
      >
        <template #body="{ data }">
          <VcIcon
            v-if="data.isDefault"
            icon="lucide-check"
            size="s"
            :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.DEFAULT')"
          />
        </template>
      </VcColumn>
      <VcColumn
        id="name"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="fee"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.FEE')"
        sortable
      />
      <VcColumn
        id="calculationType"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_LIST.CALCULATION_TYPE')"
        sortable
      >
        <template #body="{ data }">{{ calculationTypeName(data.calculationType) }}</template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useDataTableSort, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcIcon } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { CommissionFeeType, type CommissionFee } from "../../api_client/virtocommerce.marketplacecommissions";

// legacy fee-static-list
defineBlade({ url: "/static-commissions", name: "MarketplaceCommissionsStaticFees", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren, callParent } = useBlade();

const items = ref<CommissionFee[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const activeItemId = ref<string>();
// the default commission first, as the legacy list
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "isDefault", initialDirection: "DESC" });
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const calculationTypeName = (x?: string) => (x ? t(`MARKETPLACE_COMMISSIONS.CALCULATION_TYPES.${x}`) : "");

async function load() {
  loading.value = true;
  try {
    const res = await api.fees.searchFee({ type: CommissionFeeType.Static, sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}
load();
watch([sortExpression, () => pagination.skip], load);

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({ name: "MarketplaceCommissionsStaticFeeDetails", param: id, onClose: () => (activeItemId.value = undefined) });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "operator:resources",
    clickHandler: () => edit(),
  },
]);

exposeToChildren({ reload: load });
// highlights this list in the commissions workspace; opened from elsewhere, the parent has no such method
callParent("listOpened", "MarketplaceCommissionsStaticFees").catch(() => undefined);
</script>
