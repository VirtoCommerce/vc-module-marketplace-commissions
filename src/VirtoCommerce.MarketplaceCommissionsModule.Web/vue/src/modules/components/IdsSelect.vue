<template>
  <!-- a searchable picker of catalog/vendor ids; VcSelect asks `options` for the labels of the selected ids too -->
  <VcSelect
    class="mc-ids"
    :class="{ 'mc-ids--wide': wide }"
    size="small"
    searchable
    :multiple="multiple"
    :options="search"
    option-value="id"
    :option-label="label"
    :placeholder="placeholder"
    :aria-label="placeholder"
    :model-value="multiple ? modelValue : (modelValue[0] ?? null)"
    :disabled="disabled"
    @update:model-value="update"
  />
</template>

<script setup lang="ts">
import { VcSelect } from "@vc-shell/framework/ui";
import type { Option, Page } from "../lookups";

const props = defineProps<{
  modelValue: string[];
  search: (keyword?: string, skip?: number, ids?: string[]) => Promise<Page>;
  multiple?: boolean;
  wide?: boolean;
  placeholder?: string;
  disabled?: boolean;
}>();
const emit = defineEmits<{ "update:modelValue": [value: string[]] }>();

const label = (x: Option) => (x.code ? `${x.name} (${x.code})` : x.name);

// the API keeps id lists even for single pickers (includingCategories): a single value is a one-item list
function update(value: unknown) {
  if (props.multiple) emit("update:modelValue", (value as string[] | null) ?? []);
  else emit("update:modelValue", value ? [value as string] : []);
}
</script>

<style scoped>
.mc-ids {
  flex: 0 0 auto;
  width: 240px;
}

.mc-ids--wide {
  width: 320px;
}
</style>
