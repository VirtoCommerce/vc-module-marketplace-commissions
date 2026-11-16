<template>
  <VcBlade
    :title="param ? (original.name ?? '') : $t('MARKETPLACE_COMMISSIONS.DETAILS.TITLE_NEW')"
    :subtitle="$t('MARKETPLACE_COMMISSIONS.DETAILS.SUBTITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
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
        <VcRow class="mc-form__row">
          <VcCol class="mc-form__switch">
            <VcSwitch
              v-model="item.isActive"
              :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.IS_ACTIVE')"
              :disabled="readonly"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.priority"
              type="integer"
              :label="$t('MARKETPLACE_COMMISSIONS.DETAILS.PRIORITY')"
              :placeholder="$t('MARKETPLACE_COMMISSIONS.DETAILS.PRIORITY_PLACEHOLDER')"
              :hint="$t('MARKETPLACE_COMMISSIONS.DETAILS.PRIORITY_HINT')"
              :error-message="priorityError"
              :disabled="readonly"
              name="priority"
              required
            />
          </VcCol>
        </VcRow>
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
        <VcCard
          v-if="item.expressionTree"
          :header="$t('MARKETPLACE_COMMISSIONS.DETAILS.CONDITIONS')"
          class="mc-form__conditions"
        >
          <FeeConditions
            :tree="item.expressionTree as never"
            :disabled="readonly"
          />
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCard,
  VcCol,
  VcContainer,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import FeeConditions from "../components/FeeConditions.vue";
import type { DynamicCommissionFee } from "../../api_client/virtocommerce.marketplacecommissions";

// legacy fee-dynamic-details (and fee-dynamic-add for a new one)
defineBlade({ url: "/dynamic-commission", name: "MarketplaceCommissionsDynamicFeeDetails", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();

const item = ref<DynamicCommissionFee>({});
// the saved commission: the blade title shows its name, not the one being typed
const original = ref<DynamicCommissionFee>({});
const loading = ref(false);
const readonly = computed(() => !hasAccess("operator:resources"));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("MARKETPLACE_COMMISSIONS.DETAILS.CLOSE_CONFIRM")) });

const calculationTypes = computed(() =>
  ["Percent", "Fixed"].map((id) => ({ id, title: t(`MARKETPLACE_COMMISSIONS.CALCULATION_TYPES.${id}`) })),
);

const isEmpty = (v: unknown) => v === undefined || v === null || `${v}` === "";
// legacy validatePriorityAsync: an integer is required
const priorityError = computed(() =>
  isEmpty(item.value.priority)
    ? t("MARKETPLACE_COMMISSIONS.DETAILS.REQUIRED")
    : Number.isInteger(Number(item.value.priority))
      ? undefined
      : t("MARKETPLACE_COMMISSIONS.DETAILS.PRIORITY_NOT_INTEGER"),
);
const valid = computed(() => !!item.value.name?.trim() && !isEmpty(item.value.fee) && !priorityError.value);

async function load() {
  loading.value = true;
  try {
    // a new commission comes from the server with the condition tree prototype (blocks and their available children)
    item.value = param.value
      ? ((await api.fees.getFeeById(param.value)) as DynamicCommissionFee)
      : await api.fees.getNewFee();
    original.value = { ...item.value };
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

type TreeNode = { availableChildren?: unknown; children?: TreeNode[] };
// the designer's prototypes aren't stored: the server merges them back on read (legacy stripOffUiInformation)
function withoutPrototypes<T extends TreeNode>(node: T): T {
  const { availableChildren: _, children, ...rest } = node;
  return { ...rest, ...(children ? { children: children.map(withoutPrototypes) } : {}) } as T;
}

async function save() {
  loading.value = true;
  try {
    const feeDetails = {
      ...item.value,
      fee: Number(item.value.fee),
      priority: Number(item.value.priority),
      expressionTree: item.value.expressionTree ? withoutPrototypes(item.value.expressionTree as TreeNode) : undefined,
    };
    if (param.value) {
      await api.fees.updateFee({ feeDetails: feeDetails as never });
    } else {
      await api.fees.createFee({ feeDetails: feeDetails as never });
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
    // a new commission can be created as it comes (legacy "Create" button only checked the form)
    disabled: (!!param.value && !form.canSave.value) || !valid.value,
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
    // nothing to delete before it's saved
    isVisible: !!param.value,
    permissions: "operator:resources",
    disabled: loading.value,
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

/* lines the switch up with the labelled input next to it */
.mc-form__switch {
  padding-top: 26px;
}

.mc-form__conditions :deep(.vc-card__body) {
  padding: 0;
}
</style>
