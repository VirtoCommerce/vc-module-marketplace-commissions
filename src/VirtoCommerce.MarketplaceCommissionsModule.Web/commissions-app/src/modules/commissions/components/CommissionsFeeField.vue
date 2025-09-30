<template>
  <VcField
    v-model="computedFee"
    :label="$t('COMMISSIONS.CONTROL.COMMISSION.LABEL')"
    variant="text"
  />
</template>

<script lang="ts" setup>
import { VcField } from "@vc-shell/framework";
import { onMounted } from "vue";
import { useCommissions } from "../composables/useCommissions";

const { computedFee, getCommissions } = useCommissions();

onMounted(async () => {
  await getCommissions();
});
</script>
