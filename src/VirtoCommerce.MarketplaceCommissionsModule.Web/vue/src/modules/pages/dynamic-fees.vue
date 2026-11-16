<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'MARKETPLACE_COMMISSIONS_DYNAMIC_FEES'"
      :active-item-id="activeItemId"
      :row-actions="rowActions"
      :empty-state="{ icon: 'lucide-percent', title: $t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.EMPTY') }"
      @row-click="(e: { data: CommissionFee }) => edit(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="isActive"
        :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.STATE')"
        sortable
      >
        <template #body="{ data }">
          <VcStatus
            :variant="data.isActive ? 'success' : 'light-danger'"
            :extend="false"
            >{{ $t(data.isActive ? "MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.ACTIVE" : "MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.INACTIVE") }}</VcStatus
          >
        </template>
      </VcColumn>
      <VcColumn
        id="priority"
        :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.PRIORITY')"
        sortable
      />
      <VcColumn
        id="fee"
        :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.FEE')"
        sortable
      />
      <VcColumn
        id="calculationType"
        :title="$t('MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.CALCULATION_TYPE')"
        sortable
      >
        <template #body="{ data }">{{ calculationTypeName(data.calculationType) }}</template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  usePermissions,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { CommissionFeeType, type CommissionFee } from "../../api_client/virtocommerce.marketplacecommissions";

// legacy fee-dynamic-list
defineBlade({ url: "/dynamic-commissions", name: "MarketplaceCommissionsDynamicFees", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren, callParent } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();

const items = ref<CommissionFee[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const selection = ref<CommissionFee[]>([]);
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "priority", initialDirection: "ASC" });
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const calculationTypeName = (x?: string) => (x ? t(`MARKETPLACE_COMMISSIONS.CALCULATION_TYPES.${x}`) : "");

async function load() {
  loading.value = true;
  try {
    const res = await api.fees.searchFee({ type: CommissionFeeType.Dynamic, sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
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
  openBlade({ name: "MarketplaceCommissionsDynamicFeeDetails", param: id, onClose: () => (activeItemId.value = undefined) });
}

async function remove(fees: CommissionFee[]) {
  if (!fees.length) return;
  const message =
    fees.length === 1
      ? t("MARKETPLACE_COMMISSIONS.DETAILS.DELETE_CONFIRM", { name: fees[0].name ?? "" })
      : t("MARKETPLACE_COMMISSIONS.DYNAMIC_LIST.DELETE_CONFIRM");
  if (!(await showConfirmation(message))) return;
  await closeChildren();
  loading.value = true;
  try {
    await api.fees.deleteFee(fees.map((x) => x.id!));
    selection.value = [];
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
  await load();
}

// legacy row context menu: Edit / Delete
const rowActions = () => [
  { icon: "lucide-pencil", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.EDIT"), type: "info" as const, clickHandler: (x?: CommissionFee) => edit(x?.id) },
  ...(hasAccess("operator:resources")
    ? [
        {
          icon: "lucide-trash-2",
          title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.DELETE"),
          type: "danger" as const,
          clickHandler: (x?: CommissionFee) => (x ? remove([x]) : undefined),
        },
      ]
    : []),
];

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "operator:resources",
    clickHandler: () => edit(),
  },
  {
    id: "delete",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "operator:resources",
    disabled: !selection.value.length,
    clickHandler: () => remove(selection.value),
  },
]);

exposeToChildren({ reload: load });
// highlights this list in the commissions workspace; opened from elsewhere, the parent has no such method
callParent("listOpened", "MarketplaceCommissionsDynamicFees").catch(() => undefined);
</script>
