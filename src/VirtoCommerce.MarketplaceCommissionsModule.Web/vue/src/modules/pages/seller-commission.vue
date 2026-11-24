<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.SELLER_COMMISSION.TITLE')"
    :subtitle="options?.sellerName"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="mc-form">
        <VcSelect
          v-model="item.commissionFeeId"
          :label="$t('MARKETPLACE_COMMISSIONS.SELLER_COMMISSION.FEE')"
          :placeholder="$t('MARKETPLACE_COMMISSIONS.SELLER_COMMISSION.FEE_PLACEHOLDER')"
          :options="fees"
          option-value="id"
          option-label="name"
          :clearable="false"
          :disabled="readonly"
          name="commissionFeeId"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { CommissionFeeType, type CommissionFee } from "../../api_client/virtocommerce.marketplacecommissions";
import type { SellerCommissionOptions } from "../types";

// legacy seller-commissions: the static commission applied to a seller
defineBlade({ url: "/seller-commission", name: "MarketplaceCommissionsSellerCommission", permissions: ["operator:resources"] });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<SellerCommissionOptions>();
const { showError } = usePopup();
const { hasAccess } = usePermissions();

const item = ref<{ commissionFeeId?: string }>({});
const fees = ref<CommissionFee[]>([]);
const loading = ref(false);
const readonly = computed(() => !hasAccess("operator:resources"));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("MARKETPLACE_COMMISSIONS.SELLER_COMMISSION.CLOSE_CONFIRM")) });

// all static commissions to choose from (legacy took up to 1000)
async function loadFees() {
  const res = await api.fees.searchFee({ type: CommissionFeeType.Static, sort: "name", skip: 0, take: 1000 });
  fees.value = res.results ?? [];
}

async function load() {
  if (!options.value?.sellerId) return;
  loading.value = true;
  try {
    const [current] = await Promise.all([api.sellerCommissions.getSellerCommission(options.value.sellerId), loadFees()]);
    item.value = { commissionFeeId: current?.id };
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  loading.value = true;
  try {
    await api.sellerCommissions.updateSellerCommission({
      sellerId: options.value!.sellerId,
      sellerName: options.value!.sellerName,
      commissionFeeId: item.value.commissionFeeId,
    });
    form.setBaseline();
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
    title: t("MARKETPLACE_COMMISSIONS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "operator:resources",
    disabled: !form.canSave.value || !item.value.commissionFeeId,
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
    // legacy pencil next to the field: manage the static commissions, then pick from the refreshed list
    id: "edit",
    title: t("MARKETPLACE_COMMISSIONS.SELLER_COMMISSION.MANAGE"),
    icon: "lucide-pencil",
    permissions: "operator:resources",
    clickHandler: () => openBlade({ name: "MarketplaceCommissionsStaticFees", onClose: () => loadFees().catch(() => undefined) }),
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
</style>
