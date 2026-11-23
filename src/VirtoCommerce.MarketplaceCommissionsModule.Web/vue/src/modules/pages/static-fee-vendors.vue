<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.TITLE')"
    :subtitle="options?.commissionName"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      searchable
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'MARKETPLACE_COMMISSIONS_STATIC_FEE_VENDORS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-store', title: $t('MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.EMPTY') }"
      @row-click="(e: { data: Row }) => openVendor(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <!-- not sortable: the name belongs to the vendor (MarketplaceVendor), the assignments are searched here -->
      <VcColumn
        id="sellerName"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.NAME')"
        always-visible
      />
      <VcColumn
        id="createdDate"
        :title="$t('MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.ASSIGNED')"
        type="date-ago"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useDataTableSort, useFunctions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { searchVendors } from "../lookups";
import type { StaticFeeVendorsOptions } from "../types";

// the vendors assigned to a static commission (read-only); opened by the static commission's "Vendors" widget
defineBlade({ name: "MarketplaceCommissionsStaticFeeVendors", permissions: ["operator:resources"], routable: false });

// MarketplaceVendor's seller blade, opened by name: no code dependency on that module (Commissions already depends on it)
const VENDOR_DETAILS_BLADE = "MarketplaceSellerDetails";

type Row = { id: string; sellerId: string; sellerName: string; createdDate?: Date };

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<StaticFeeVendorsOptions>();
const { showError } = usePopup();
const { debounce } = useFunctions();

const items = ref<Row[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const keyword = ref("");
const activeItemId = ref<string>();
// the latest assigned vendors first; the keyword matches vendor names (resolved by MarketplaceVendor on the server)
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "createdDate", initialDirection: "DESC" });
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

async function load() {
  const commissionFeeId = options.value?.commissionFeeId;
  if (!commissionFeeId) return;
  loading.value = true;
  try {
    const res = await api.fees.searchSellerCommissions({
      commissionFeeId,
      keyword: keyword.value || undefined,
      sort: sortExpression.value,
      skip: pagination.skip,
      take: PAGE_SIZE,
    });
    const assignments = res.results ?? [];
    // the assignment keeps the vendor id only: names come from the vendors themselves, so a renamed vendor shows its current name
    const sellerIds = assignments.map((x) => x.sellerId).filter((x): x is string => !!x);
    const vendors = sellerIds.length ? (await searchVendors(undefined, 0, sellerIds)).results : [];
    const names = new Map(vendors.map((x) => [x.id, x.name]));
    items.value = assignments.map((x) => ({
      id: x.id ?? x.sellerId ?? "",
      sellerId: x.sellerId ?? "",
      sellerName: names.get(x.sellerId ?? "") ?? x.sellerId ?? "",
      createdDate: x.createdDate,
    }));
    totalCount.value = res.totalCount ?? 0;
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
}
onMounted(load);
watch(keyword, () => pagination.setPage(1));
watch([sortExpression, keyword, () => pagination.skip], debounce(load, 300));

async function openVendor(row: Row) {
  if (!row.sellerId) return;
  activeItemId.value = row.id;
  try {
    await openBlade({ name: VENDOR_DETAILS_BLADE, param: row.sellerId, onClose: () => (activeItemId.value = undefined) });
  } catch {
    // the vendors plugin isn't loaded in this backoffice
    activeItemId.value = undefined;
    showError(t("MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.CANNOT_OPEN_VENDOR"));
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
]);
</script>
