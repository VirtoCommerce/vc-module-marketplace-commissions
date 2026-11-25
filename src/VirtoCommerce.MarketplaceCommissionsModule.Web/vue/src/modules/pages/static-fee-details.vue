<template>
  <VcBlade
    :title="param ? (original.name ?? '') : $t('MARKETPLACE_COMMISSIONS.DETAILS.TITLE_NEW')"
    :subtitle="$t('MARKETPLACE_COMMISSIONS.DETAILS.SUBTITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="mc-form">
        <VcInput
          v-model="item.name"
          :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.NAME')"
          :placeholder="$t('MARKETPLACE_COMMISSIONS.DETAILS.NAME_PLACEHOLDER')"
          :disabled="readonly"
          maxlength="254"
          name="name"
          rules="required"
          required
        />
        <VcTextarea
          v-model="item.description"
          :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.DESCRIPTION')"
          :placeholder="$t('MARKETPLACE_COMMISSIONS.DETAILS.DESCRIPTION_PLACEHOLDER')"
          :disabled="readonly"
          maxlength="4000"
          name="description"
        />
        <!-- the default commission can't be unset: another one is made default instead -->
        <VcSwitch
          v-model="item.isDefault"
          class="mc-form__default"
          :label="$t(original.isDefault ? 'MARKETPLACE_COMMISSIONS.DETAILS.DEFAULT' : 'MARKETPLACE_COMMISSIONS.DETAILS.SET_DEFAULT')"
          :label-tooltip="$t('MARKETPLACE_COMMISSIONS.DETAILS.DEFAULT_TOOLTIP')"
          :hint="original.isDefault ? $t('MARKETPLACE_COMMISSIONS.DETAILS.CAN_NOT_REMOVE_DEFAULT') : undefined"
          :disabled="readonly || original.isDefault"
        />
        <VcRow class="mc-form__row">
          <VcCol>
            <VcInput
              v-model="item.fee"
              type="number"
              :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.FEE')"
              :placeholder="$t('MARKETPLACE_COMMISSIONS.DETAILS.FEE_PLACEHOLDER')"
              :disabled="readonly"
              name="fee"
              rules="required"
              required
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.calculationType"
              :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.CALCULATION_TYPE')"
              :options="calculationTypes"
              option-value="id"
              option-label="title"
              :clearable="false"
              :disabled="readonly"
              name="calculationType"
            />
          </VcCol>
        </VcRow>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, useBladeWidgets, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import {
  CommissionFeeType,
  FeeCalculationType,
  type CommissionFeeDetails,
} from "../../api_client/virtocommerce.marketplacecommissions";
import type { StaticFeeVendorsOptions } from "../types";
import BulkSellerCommissionPopup from "../components/BulkSellerCommissionPopup.vue";

// legacy fee-static-details (and fee-static-add for a new one)
defineBlade({ url: "/static-commission", name: "MarketplaceCommissionsStaticFeeDetails", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf, openBlade, exposeToChildren } = useBlade();
const { showConfirmation, showError, showInfo } = usePopup();
const { hasAccess } = usePermissions();

const NEW_FEE: CommissionFeeDetails = { type: CommissionFeeType.Static, calculationType: FeeCalculationType.Percent, fee: 0 };
const item = ref<CommissionFeeDetails>({ ...NEW_FEE });
// the saved commission: the title and the "default" lock follow it, not the edits
const original = ref<CommissionFeeDetails>({});
const loading = ref(false);
const readonly = computed(() => !hasAccess("operator:resources"));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("MARKETPLACE_COMMISSIONS.DETAILS.CLOSE_CONFIRM")) });

const calculationTypes = computed(() =>
  ["Percent", "Fixed"].map((id) => ({ id, title: t(`MARKETPLACE_COMMISSIONS.CALCULATION_TYPES.${id}`) })),
);

const valid = computed(() => !!item.value.name?.trim() && item.value.fee !== undefined && item.value.fee !== null && `${item.value.fee}` !== "");

async function load() {
  loading.value = true;
  try {
    item.value = param.value ? ((await api.fees.getFeeById(param.value)) as CommissionFeeDetails) : { ...NEW_FEE };
    original.value = { ...item.value };
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

// the vendors assigned to the commission: their count on the widget, the list behind it
const vendorsCount = ref<number | string>("…");

async function loadVendorsCount() {
  if (!param.value) return;
  vendorsCount.value =
    (await api.fees.searchSellerCommissions({ commissionFeeId: param.value, take: 0 }).catch(() => undefined))?.totalCount ?? 0;
}

useBladeWidgets([
  {
    id: "MarketplaceCommissionsStaticFeeVendors",
    icon: "lucide-store",
    title: "MARKETPLACE_COMMISSIONS.WIDGETS.VENDORS",
    badge: vendorsCount,
    // a new commission has no vendors yet
    isVisible: computed(() => !!param.value),
    onClick: openVendors,
    onRefresh: loadVendorsCount,
  },
]);

function openVendors() {
  if (!param.value) return;
  const options: StaticFeeVendorsOptions = { commissionFeeId: param.value, commissionName: original.value.name };
  openBlade({ name: "MarketplaceCommissionsStaticFeeVendors", options, onClose: loadVendorsCount });
}

async function save() {
  loading.value = true;
  try {
    const feeDetails = { ...item.value, fee: Number(item.value.fee) };
    if (param.value) {
      await api.fees.updateFee({ feeDetails });
    } else {
      await api.fees.createFee({ feeDetails });
    }
    form.setBaseline();
    callParent("reload");
    // as the legacy "Create" button: a new commission closes and shows in the refreshed list
    if (!param.value) {
      closeSelf();
      return;
    }
    await load();
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t(param.value ? "MARKETPLACE_COMMISSIONS.TOOLBAR.SAVE" : "MARKETPLACE_COMMISSIONS.TOOLBAR.CREATE"),
    icon: "lucide-save",
    permissions: "operator:resources",
    disabled: !form.canSave.value || !valid.value,
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    permissions: "operator:resources",
    disabled: !form.isModified.value,
    clickHandler: load,
  },
  {
    id: "delete",
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    // nothing to delete before it's saved; the default commission can't be deleted (legacy canExecute)
    isVisible: !!param.value,
    permissions: "operator:resources",
    disabled: loading.value || !!original.value.isDefault,
    clickHandler: startDelete,
  },
]);

// Deleting a commission with vendors: they are moved to another static commission first, then it's deleted.
// The backend refuses to delete a static commission that still has vendors, so a partial move keeps it.

const deleteVendorsCount = ref(0);

const { open: openDeletePopup, close: closeDeletePopup } = usePopup(
  computed(() => ({
    component: BulkSellerCommissionPopup,
    props: {
      commissionFeeId: param.value ?? "",
      commissionName: original.value.name,
      count: deleteVendorsCount.value,
      mode: "delete" as const,
    },
    emits: {
      onClose: () => closeDeletePopup(),
      onViewVendors: () => {
        closeDeletePopup();
        openVendors();
      },
      onConfirm: (target: { id: string; name: string }) => {
        closeDeletePopup();
        void moveVendorsAndDelete(target);
      },
    },
  })),
);

async function countVendors(): Promise<number> {
  const res = await api.fees.searchSellerCommissions({ commissionFeeId: param.value!, take: 0 });
  return res?.totalCount ?? 0;
}

async function startDelete() {
  const name = original.value.name ?? "";
  loading.value = true;
  let count: number;
  try {
    // not the widget badge: vendors may have been assigned since the blade was opened
    count = await countVendors();
  } catch (e) {
    showError(String((e as Error).message ?? e));
    return;
  } finally {
    loading.value = false;
  }
  vendorsCount.value = count;

  if (count > 0) {
    deleteVendorsCount.value = count;
    openDeletePopup();
    return;
  }

  if (!(await showConfirmation(t("MARKETPLACE_COMMISSIONS.DETAILS.DELETE_CONFIRM", { name })))) return;
  loading.value = true;
  try {
    await deleteAndClose();
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
}

async function moveVendorsAndDelete(target: { id: string; name: string }) {
  const name = original.value.name ?? "";
  loading.value = true;
  try {
    const res = await api.fees.bulkUpdateSellerCommissions({
      commissionFeeId: param.value!,
      targetCommissionFeeId: target.id,
      allSellers: true,
    });
    const moved = res?.updatedCount ?? 0;

    // vendors assigned or changed by someone else meanwhile are left: keep the commission, as the backend would
    const remaining = await countVendors();
    if (remaining > 0) {
      vendorsCount.value = remaining;
      await showError(t("MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.NOT_DELETED", { name, moved, to: target.name, remaining }, remaining));
      return;
    }

    await deleteAndClose();
    await showInfo(t("MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.DONE", { name, count: moved, to: target.name }, moved));
  } catch (e) {
    showError(String((e as Error).message ?? e));
    loadVendorsCount();
  } finally {
    loading.value = false;
  }
}

async function deleteAndClose() {
  await api.fees.deleteFee([param.value!]);
  // deleted: drop unsaved changes, otherwise closing the blade asks to save them
  form.setBaseline();
  callParent("reload");
  closeSelf();
}

onMounted(() => {
  load();
  loadVendorsCount();
});

// the vendors list changes the assignments in bulk
exposeToChildren({ reloadVendorsCount: loadVendorsCount });
</script>

<style scoped>
.mc-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.mc-form__row {
  gap: 16px;
}

/* a disabled VcSwitch turns off pointer events, which hides the label tooltip on the locked default commission */
.mc-form__default :deep(.vc-label__tooltip) {
  pointer-events: auto;
}
</style>
