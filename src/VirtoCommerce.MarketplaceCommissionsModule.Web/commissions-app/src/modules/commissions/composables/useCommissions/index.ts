import { useApiClient } from "@vc-shell/framework";
import { VcmpSellerCommissionClient } from "@vcmp-marketplace-commissions/api/marketplacecommissions";
import { type Ref, computed, inject, toRef, ref, onBeforeMount } from "vue";
import { useRoute } from "vue-router";
import * as _ from "lodash-es";
import { useI18n } from "vue-i18n";

const { getApiClient } = useApiClient(VcmpSellerCommissionClient);

export const useCommissions = () => {
  const route = useRoute();
  const { t } = useI18n();
  const currentSeller = inject("currentSeller", toRef({ id: route?.params?.sellerId })) as Ref<{
    [x: string]: unknown;
    id: string;
  }>;

  const fee = ref();

  async function getCommissions() {
    const commission = await (await getApiClient()).getSellerCommission(currentSeller.value.id);
    fee.value = commission;
  }

  const computedFee = computed(() => {
    if (fee.value && fee.value.fee) {
      return `${fee.value.name} (${fee.value.fee} ${fee.value.calculationType === "Percent" ? "%" : "Fixed"})`;
    }
    return t("COMMISSIONS.CONTROL.COMMISSION.NOT_AVAILABLE");
  });

  return {
    getCommissions,
    computedFee,
  };
};
