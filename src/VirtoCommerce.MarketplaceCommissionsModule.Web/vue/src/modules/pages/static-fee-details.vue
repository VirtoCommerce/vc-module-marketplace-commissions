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
          :label="$t(original.isDefault ? 'MARKETPLACE_COMMISSIONS.DETAILS.DEFAULT' : 'MARKETPLACE_COMMISSIONS.DETAILS.SET_DEFAULT')"
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
import { useBlade, useBladeForm, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import {
  CommissionFeeType,
  FeeCalculationType,
  type CommissionFeeDetails,
} from "../../api_client/virtocommerce.marketplacecommissions";

// legacy fee-static-details (and fee-static-add for a new one)
defineBlade({ url: "/static-commission", name: "MarketplaceCommissionsStaticFeeDetails", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf } = useBlade();
const { showConfirmation, showError } = usePopup();
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
    async clickHandler() {
      if (!(await showConfirmation(t("MARKETPLACE_COMMISSIONS.DETAILS.DELETE_CONFIRM", { name: original.value.name ?? "" })))) return;
      loading.value = true;
      try {
        await api.fees.deleteFee([param.value!]);
        // deleted: drop unsaved changes, otherwise closing the blade asks to save them
        form.setBaseline();
        callParent("reload");
        closeSelf();
      } catch (e) {
        showError(String((e as Error).message ?? e));
      } finally {
        loading.value = false;
      }
    },
  },
]);

onMounted(load);
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
</style>
