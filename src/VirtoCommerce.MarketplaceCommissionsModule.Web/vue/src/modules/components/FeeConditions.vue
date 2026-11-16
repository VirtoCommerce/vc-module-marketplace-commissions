<template>
  <!-- the root (DynamicCommissionFeeTree) is fixed: its two blocks (vendor, commission) are edited, not added -->
  <div class="mc-cond-tree">
    <ConditionNode
      v-for="(block, i) in tree?.children ?? []"
      :key="i"
      :node="block"
      :disabled="disabled"
    />
  </div>
</template>

<script setup lang="ts">
import ConditionNode, { type ConditionTreeNode } from "./ConditionNode.vue";

defineProps<{ tree?: { children?: ConditionTreeNode[] } | null; disabled?: boolean }>();
</script>
