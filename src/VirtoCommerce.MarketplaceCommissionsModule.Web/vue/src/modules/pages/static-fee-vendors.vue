<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.STATIC_FEE_VENDORS.TITLE')"
    :subtitle="options?.commissionName"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      v-model:select-all-active="selectAllActive"
      v-model:search-value="keyword"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      searchable
      selection-mode="multiple"
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
import { notification, useBlade, useDataTablePagination, useDataTableSort, useFunctions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import BulkSellerCommissionPopup from "../components/BulkSellerCommissionPopup.vue";
import { searchVendors } from "../lookups";
import type { StaticFeeVendorsOptions } from "../types";

// the vendors assigned to a static commission; opened by the static commission's "Vendors" widget.
// Selected vendors (or all of them, across pages) can be assigned to another static commission in bulk.
defineBlade({ name: "MarketplaceCommissionsStaticFeeVendors", permissions: ["operator:resources"], routable: false });

// MarketplaceVendor's seller blade, opened by name: no code dependency on that module (Commissions already depends on it)
const VENDOR_DETAILS_BLADE = "MarketplaceSellerDetails";

type Row = { id: string; sellerId: string; sellerName: string; createdDate?: Date };

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { options, openBlade, callParent } = useBlade<StaticFeeVendorsOptions>();
const { showError } = usePopup();
const { debounce } = useFunctions();

const items = ref<Row[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const keyword = ref("");
const activeItemId = ref<string>();
// the selected rows; with selectAllActive, every vendor matching the keyword is selected, on all pages
const selection = ref<Row[]>([]);
const selectAllActive = ref(false);
const selectedCount = computed(() => (selectAllActive.value ? totalCount.value : selection.value.length));
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
// "select all" and the selected rows belong to the previous search: a vendor out of sight must not be updated
watch(keyword, () => {
  clearSelection();
  pagination.setPage(1);
});
// in "select all" mode the rows of each page shown are checked too
watch(items, (rows) => {
  if (!selectAllActive.value) return;
  const ids = new Set(selection.value.map((x) => x.id));
  selection.value = [...selection.value, ...rows.filter((x) => !ids.has(x.id))];
});
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

function clearSelection() {
  selection.value = [];
  selectAllActive.value = false;
}

// the selection as the bulk update gets it: snapshotted when the popup opens
const bulk = ref<{ commissionFeeId: string; allSellers: boolean; keyword?: string; sellerIds: string[]; count: number }>();

const { open: openBulkPopup, close: closeBulkPopup } = usePopup(
  computed(() => ({
    component: BulkSellerCommissionPopup,
    props: {
      commissionFeeId: bulk.value?.commissionFeeId ?? "",
      commissionName: options.value?.commissionName,
      count: bulk.value?.count ?? 0,
      keyword: bulk.value?.allSellers ? bulk.value.keyword : undefined,
    },
    emits: {
      onClose: () => closeBulkPopup(),
      onConfirm: (target: { id: string; name: string }) => {
        closeBulkPopup();
        void bulkUpdate(target);
      },
    },
  })),
);

function openBulk() {
  const commissionFeeId = options.value?.commissionFeeId;
  if (!commissionFeeId || !selectedCount.value) return;
  bulk.value = {
    commissionFeeId,
    allSellers: selectAllActive.value,
    keyword: keyword.value || undefined,
    sellerIds: selection.value.map((x) => x.sellerId).filter(Boolean),
    count: selectedCount.value,
  };
  openBulkPopup();
}

async function bulkUpdate(target: { id: string; name: string }) {
  const request = bulk.value;
  if (!request) return;
  loading.value = true;
  try {
    const res = await api.fees.bulkUpdateSellerCommissions({
      commissionFeeId: request.commissionFeeId,
      targetCommissionFeeId: target.id,
      allSellers: request.allSellers,
      keyword: request.allSellers ? request.keyword : undefined,
      sellerIds: request.allSellers ? undefined : request.sellerIds,
    });
    const updated = res?.updatedCount ?? 0;
    if (updated < request.count) {
      // the others were reassigned (or removed) by someone else since the list was loaded
      notification.warning(t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.PARTIAL", { updated, count: request.count, name: target.name }));
    } else {
      notification.success(t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.SUCCESS", { count: updated, name: target.name }, updated));
    }
    clearSelection();
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
  await load();
  // the details blade shows the number of vendors on its widget; opened from elsewhere, the parent has no such method
  callParent("reloadVendorsCount").catch(() => undefined);
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "bulk",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.CHANGE_COMMISSION"),
    icon: "lucide-arrow-right-left",
    permissions: "operator:resources",
    disabled: !selectedCount.value,
    clickHandler: openBulk,
  },
]);
</script>
