<template>
  <!-- picks the static commission to assign the selected vendors to; the blade runs the bulk update on confirm.
       In the "delete" mode the vendors are all vendors of a commission being deleted: the blade deletes it after the move -->
  <VcPopup
    :title="
      isDelete
        ? $t('MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.TITLE', { name: commissionName ?? '' })
        : $t('MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.TITLE')
    "
    modal-width="tw-max-w-lg"
    @close="$emit('close')"
  >
    <template #content>
      <div class="mc-bulk">
        <VcBanner
          v-if="isDelete"
          variant="warning"
          icon="lucide-triangle-alert"
        >
          {{ $t("MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.BLOCKED", { name: commissionName ?? "", count }, count) }}
          <VcLink
            class="mc-bulk__link"
            @click="$emit('viewVendors')"
          >
            {{ $t("MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.VIEW_VENDORS") }}
          </VcLink>
        </VcBanner>

        <dl class="mc-bulk__summary">
          <dt>{{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.FROM") }}</dt>
          <dd>{{ commissionName }}</dd>
          <dt>{{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.VENDORS") }}</dt>
          <dd>
            {{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.VENDORS_COUNT", { count }, count) }}
            <template v-if="keyword">
              · {{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.MATCHING", { keyword }) }}
            </template>
          </dd>
        </dl>

        <!-- a radio list, not a VcSelect: the select's dropdown is teleported under the popup and can't be seen -->
        <VcLoading
          v-if="loading"
          active
        />
        <VcRadioGroup
          v-else-if="targets.length"
          v-model="targetId"
          :label="$t('MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.TARGET')"
          :options="targets.map((x) => ({ label: x.label, value: x.id }))"
          orientation="vertical"
          required
        />
        <p
          v-else
          class="mc-bulk__empty"
        >
          {{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.NO_TARGETS") }}
        </p>

        <!-- the summary of the change, once the target is known -->
        <p
          v-if="isDelete && target"
          class="mc-bulk__outcome"
        >
          {{
            $t(
              "MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.SUMMARY",
              { count, from: commissionName ?? "", to: target.name },
              count,
            )
          }}
        </p>

        <VcBanner
          variant="info"
          icon="lucide-info"
        >
          {{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.NOTE") }}
        </VcBanner>
      </div>
    </template>

    <template #footer>
      <div class="mc-bulk__footer">
        <VcButton
          variant="secondary"
          @click="$emit('close')"
        >
          {{ $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.CANCEL") }}
        </VcButton>
        <VcButton
          :disabled="!target"
          @click="confirm"
        >
          {{
            !target
              ? $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.CONFIRM_DISABLED")
              : isDelete
                ? $t("MARKETPLACE_COMMISSIONS.DELETE_WITH_VENDORS.CONFIRM", { count, name: commissionName ?? "" }, count)
                : $t("MARKETPLACE_COMMISSIONS.BULK_SELLER_COMMISSION.CONFIRM", { count, name: target.name }, count)
          }}
        </VcButton>
      </div>
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
import { usePopup } from "@vc-shell/framework";
import { VcBanner, VcButton, VcLink, VcLoading, VcPopup, VcRadioGroup } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { CommissionFeeType, FeeCalculationType } from "../../api_client/virtocommerce.marketplacecommissions";

const props = defineProps<{
  /** the static commission the vendors are assigned to now: not offered as the target */
  commissionFeeId: string;
  commissionName?: string;
  /** the number of vendors to update */
  count: number;
  /** the vendor name filter of a "select all" selection */
  keyword?: string;
  /** "delete": all vendors of the commission are moved before it's deleted; the default commission is preselected */
  mode?: "move" | "delete";
}>();
const emit = defineEmits<{ close: []; confirm: [target: { id: string; name: string }]; viewVendors: [] }>();

const { t } = useI18n({ useScope: "global" });
const { showError } = usePopup();

const isDelete = computed(() => props.mode === "delete");

type Target = { id: string; name: string; label: string; isDefault: boolean };

// static commissions are few: all of them, the default one first
const targets = ref<Target[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const res = await api.fees.searchFee({ type: CommissionFeeType.Static, sort: "isDefault:desc;name", take: 1000 });
    targets.value = (res.results ?? [])
      .filter((x) => x.id && x.id !== props.commissionFeeId)
      .map((x) => {
        const fee =
          x.calculationType === FeeCalculationType.Percent ? `${x.fee}%` : `${x.fee} ${t("MARKETPLACE_COMMISSIONS.CALCULATION_TYPES.Fixed")}`;
        const name = x.name ?? x.id!;
        const label = `${name} (${fee})${x.isDefault ? ` · ${t("MARKETPLACE_COMMISSIONS.STATIC_LIST.DEFAULT")}` : ""}`;
        return { id: x.id!, name, label, isDefault: !!x.isDefault };
      });
    // the default commission is the one new vendors get: offer it first
    if (isDelete.value) {
      targetId.value = targets.value.find((x) => x.isDefault)?.id;
    }
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
});

const targetId = ref<string>();
const target = computed(() => targets.value.find((x) => x.id === targetId.value));

function confirm() {
  if (target.value) emit("confirm", { id: target.value.id, name: target.value.name });
}
</script>

<style scoped>
.mc-bulk {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.mc-bulk__summary {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 6px 16px;
  margin: 0;
}

.mc-bulk__summary dt {
  color: var(--color-neutral-500);
}

.mc-bulk__summary dd {
  margin: 0;
  min-width: 0;
}

.mc-bulk__link {
  margin-left: 4px;
}

.mc-bulk__outcome {
  margin: 0;
  font-weight: 500;
}

.mc-bulk__empty {
  margin: 0;
  color: var(--color-neutral-500);
}

.mc-bulk__footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  width: 100%;
}
</style>
