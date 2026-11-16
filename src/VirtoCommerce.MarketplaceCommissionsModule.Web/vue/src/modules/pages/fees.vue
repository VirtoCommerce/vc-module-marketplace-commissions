<template>
  <VcBlade
    :title="$t('MARKETPLACE_COMMISSIONS.FEES.TITLE')"
    :subtitle="$t('MARKETPLACE_COMMISSIONS.FEES.SUBTITLE')"
    width="30%"
  >
    <VcContainer>
      <ul class="mc-fees">
        <li
          v-for="entry in entries"
          :key="entry.blade"
        >
          <button
            type="button"
            class="mc-fees__item"
            :class="{ 'mc-fees__item--active': active === entry.blade }"
            @click="open(entry.blade)"
          >
            <VcIcon
              icon="lucide-percent"
              size="m"
            />
            <span>{{ $t(entry.title) }}</span>
          </button>
        </li>
      </ul>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcIcon } from "@vc-shell/framework/ui";
import { onMounted, ref } from "vue";

// legacy fees: the commissions workspace, a list of the two commission kinds (opened from the main menu)
defineBlade({
  url: "/marketplace-commissions",
  name: "MarketplaceCommissionsFees",
  isWorkspace: true,
  permissions: ["operator:commissions"],
  menuItem: { title: "MARKETPLACE_COMMISSIONS.MENU.TITLE", icon: "lucide-percent", priority: 300 },
});

const { openBlade, exposeToChildren } = useBlade();

const entries = [
  { blade: "MarketplaceCommissionsStaticFees", title: "MARKETPLACE_COMMISSIONS.FEES.STATIC" },
  { blade: "MarketplaceCommissionsDynamicFees", title: "MARKETPLACE_COMMISSIONS.FEES.DYNAMIC" },
];
const active = ref<string>();

function open(blade: string) {
  active.value = blade;
  openBlade({ name: blade, onClose: () => (active.value === blade ? (active.value = undefined) : undefined) });
}

// the list blades report themselves, so a list restored from the URL is highlighted too
exposeToChildren({ listOpened: (blade: string) => (active.value = blade) });

// the legacy blade opened the static commissions right away; a list restored from the URL is kept
onMounted(async () => {
  await new Promise((resolve) => setTimeout(resolve));
  if (!active.value) open(entries[0].blade);
});
</script>

<style scoped>
.mc-fees {
  display: flex;
  flex-direction: column;
  padding: 8px 0;
}

.mc-fees__item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 12px 16px;
  font-size: 14px;
  text-align: start;
  color: var(--color-neutral-800);
  border-bottom: 1px solid var(--color-neutral-100);
}

.mc-fees__item:hover {
  background-color: var(--color-neutral-50);
}

.mc-fees__item--active {
  background-color: var(--color-primary-50);
  color: var(--color-primary-700);
  font-weight: 600;
}
</style>
