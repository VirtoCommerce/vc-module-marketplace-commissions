import { useApiClient } from "@vc-shell/framework";
import { VcmpSellerCommissionClient } from "@vcmp-marketplace-commissions/api/marketplacecommissions";
import { type Ref, computed, inject, toRef, onMounted, ref, watch, onBeforeMount } from "vue";
import { useRoute } from "vue-router";
import * as _ from "lodash-es";

interface ICommissionsScope {
  computedFee?: string;
}

const { getApiClient } = useApiClient(VcmpSellerCommissionClient);

export const useCommissions = (arg: { scope: Record<string, unknown>; item: any }) => {
  const route = useRoute();
  const currentSeller = inject("currentSeller", toRef({ id: route?.params?.sellerId })) as Ref<{
    [x: string]: unknown;
    id: string;
  }>;

  const fee = ref();
  const scope: Ref<ICommissionsScope> = ref({});

  async function getCommissions() {
    const commission = await (await getApiClient()).getSellerCommission(currentSeller.value.id);
    return commission;
  }

  const computedFee = computed(() => {
    if (fee.value && fee.value.fee) {
      return `${fee.value.name} (${fee.value.fee} ${fee.value.calculationType === "Percent" ? "%" : "Fixed"})`;
    }
    return "";
  });

  onBeforeMount(async () => {
    fee.value = await getCommissions();
    scope.value.computedFee = computedFee.value;
  });

  watch(
    () => arg.item,
    (newVal) => {
      console.log("newVal", newVal.value);
    },
    { deep: true },
  );

  return {
    scope: _.merge(scope.value, arg.scope),
  };
};
