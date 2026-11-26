<template>
  <div
    class="mc-cond"
    :class="depth === 0 ? 'mc-cond--block' : 'mc-cond--item'"
    :data-condition="node.id"
  >
    <div class="mc-cond__head">
      <div class="mc-cond__line">
        <!-- "Vendor matching all/any of the following criteria" / "If all/any of these conditions are true" -->
        <template v-if="isBlock">
          <span class="mc-cond__text">{{ $t(blockText[0]) }}</span>
          <VcSelect
            class="mc-cond__all"
            size="small"
            :options="allAny"
            option-value="value"
            option-label="label"
            :clearable="false"
            :aria-label="$t(blockText[0])"
            :model-value="String(node.all !== false)"
            :disabled="disabled"
            @update:model-value="(v) => (node.all = v === 'true')"
          />
          <span class="mc-cond__text">{{ $t(blockText[1]) }}</span>
        </template>

        <template v-else-if="node.id === 'VcmpConditionVendorIs'">
          <span class="mc-cond__text">{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.VENDOR_IS") }}</span>
          <IdsSelect
            :model-value="list('vendorIds')"
            @update:model-value="(v: string[]) => (node.vendorIds = v)"
            :search="searchVendors"
            multiple
            wide
            :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.SELECT_VENDORS')"
            :disabled="disabled"
          />
        </template>

        <template v-else-if="node.id === 'VcmpConditionCategoryIs'">
          <span class="mc-cond__text">{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.CATEGORY_IS") }}</span>
          <IdsSelect
            :model-value="list('includingCategories')"
            @update:model-value="(v: string[]) => (node.includingCategories = v)"
            :search="searchCategories"
            wide
            :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.SELECT_CATEGORY')"
            :disabled="disabled"
          />
        </template>

        <template v-else-if="node.id === 'VcmpConditionProductIs'">
          <span class="mc-cond__text">{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.PRODUCT_IS") }}</span>
          <IdsSelect
            :model-value="list('includingProducts')"
            @update:model-value="(v: string[]) => (node.includingProducts = v)"
            :search="searchProducts"
            multiple
            wide
            :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.SELECT_PRODUCTS')"
            :disabled="disabled"
          />
        </template>

        <template v-else-if="node.id === 'UserGroupsContainsCondition'">
          <span class="mc-cond__text">{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.USER_GROUP_IS") }}</span>
          <IdsSelect
            :model-value="node.group ? [String(node.group)] : []"
            @update:model-value="(v: string[]) => (node.group = v[0] ?? null)"
            :search="searchUserGroups"
            :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.SELECT_USER_GROUP')"
            :disabled="disabled"
          />
        </template>

        <!-- a node without an editor: shown by its id, can only be removed -->
        <span
          v-else
          class="mc-cond__text"
          >{{ humanize(node.id) }}</span
        >
      </div>
      <VcButton
        v-if="removable && !disabled"
        class="mc-cond__remove"
        variant="ghost"
        size="icon-sm"
        icon="lucide-x"
        :aria-label="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.REMOVE')"
        @click="$emit('remove')"
      />
    </div>

    <!-- legacy "Excluding category ..." / "Excluding product ..." children of "Category is ..." -->
    <div
      v-if="node.id === 'VcmpConditionCategoryIs'"
      class="mc-cond__line mc-cond__excluding"
    >
      <span class="mc-cond__text">{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.EXCLUDING") }}</span>
      <IdsSelect
        :model-value="list('excludingCategories')"
            @update:model-value="(v: string[]) => (node.excludingCategories = v)"
        :search="searchCategories"
        multiple
        :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.EXCLUDE_CATEGORIES')"
        :disabled="disabled"
      />
      <IdsSelect
        :model-value="list('excludingProducts')"
            @update:model-value="(v: string[]) => (node.excludingProducts = v)"
        :search="searchProducts"
        multiple
        :placeholder="$t('MARKETPLACE_COMMISSIONS.CONDITIONS.EXCLUDE_PRODUCTS')"
        :disabled="disabled"
      />
    </div>

    <div
      v-if="isBlock && children.length"
      class="mc-cond__children"
    >
      <ConditionNode
        v-for="(child, i) in children"
        :key="keyOf(child)"
        :node="child"
        :depth="depth + 1"
        :disabled="disabled"
        removable
        @remove="node.children!.splice(i, 1)"
      />
    </div>

    <VcDropdown
      v-if="addable.length && !disabled"
      v-model="menuOpen"
      :items="addable"
      :item-text="(x: AddItem) => x.label"
      floating
      close-on-select
      @item-click="add"
    >
      <template #trigger="{ toggle }">
        <VcButton
          class="mc-cond__add"
          variant="link"
          size="sm"
          icon="lucide-plus"
          @click="toggle"
          >{{ $t("MARKETPLACE_COMMISSIONS.CONDITIONS.ADD_CONDITION") }}</VcButton
        >
      </template>
    </VcDropdown>
  </div>
</template>

<script setup lang="ts">
import { VcButton, VcDropdown, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import IdsSelect from "./IdsSelect.vue";
import { searchCategories, searchProducts, searchUserGroups, searchVendors } from "../lookups";

// A node of the commission expression tree, edited in place on the API object (legacy va-dynamic-expression-tree with
// the templates of Scripts/dynamicConditions/templates.html)
export type ConditionTreeNode = Record<string, unknown> & {
  id?: string;
  all?: boolean;
  children?: ConditionTreeNode[];
  availableChildren?: ConditionTreeNode[];
};
type AddItem = { label: string; prototype: ConditionTreeNode };

defineOptions({ name: "ConditionNode" });
const props = defineProps<{ node: ConditionTreeNode; depth?: number; removable?: boolean; disabled?: boolean }>();
defineEmits<{ remove: [] }>();
const { t } = useI18n({ useScope: "global" });

const depth = computed(() => props.depth ?? 0);
const children = computed(() => props.node.children ?? []);

const BLOCKS: Record<string, [string, string]> = {
  VendorCommissionCondition: ["MARKETPLACE_COMMISSIONS.CONDITIONS.VENDOR_MATCHING", "MARKETPLACE_COMMISSIONS.CONDITIONS.OF_FOLLOWING_CRITERIA"],
  BlockCommissionCondition: ["MARKETPLACE_COMMISSIONS.CONDITIONS.IF", "MARKETPLACE_COMMISSIONS.CONDITIONS.OF_CONDITIONS_TRUE"],
};
const isBlock = computed(() => !!BLOCKS[props.node.id ?? ""]);
const blockText = computed(() => BLOCKS[props.node.id ?? ""]);
const allAny = computed(() => [
  { value: "true", label: t("MARKETPLACE_COMMISSIONS.CONDITIONS.ALL") },
  { value: "false", label: t("MARKETPLACE_COMMISSIONS.CONDITIONS.ANY") },
]);

// the conditions this editor knows; others the server may offer are left out of the "Add condition" menu
const EDITABLE = ["VcmpConditionVendorIs", "UserGroupsContainsCondition", "VcmpConditionCategoryIs", "VcmpConditionProductIs"];

// the "Add condition" menu lists the prototypes the API sends with the block (availableChildren)
const addable = computed<AddItem[]>(() =>
  isBlock.value
    ? (props.node.availableChildren ?? [])
        .filter((x) => EDITABLE.includes(x.id ?? ""))
        .map((prototype) => ({ label: t(`MARKETPLACE_COMMISSIONS.CONDITIONS.NAMES.${prototype.id}`), prototype }))
    : [],
);
const menuOpen = ref(false);

// a new child is a copy of the server's prototype, so it carries exactly the server's field set
function add(item: AddItem) {
  props.node.children ??= [];
  props.node.children.push(JSON.parse(JSON.stringify(item.prototype)) as ConditionTreeNode);
}

// an id-list property of the node (missing lists read as empty)
const list = (key: string) => (props.node[key] as string[] | undefined) ?? [];

const humanize = (id?: string) => String(id ?? "").replace(/([a-z])([A-Z])/g, "$1 $2");

// stable keys without writing anything onto the API objects
const keys = new WeakMap<object, number>();
let seq = 0;
const keyOf = (o: object) => keys.get(o) ?? (keys.set(o, ++seq), seq);
</script>

<style scoped>
.mc-cond {
  font-size: 14px;
  line-height: 20px;
  color: var(--color-neutral-700);
}

.mc-cond--block {
  padding: 14px 16px;
}

.mc-cond--block + .mc-cond--block {
  border-top: 1px solid var(--color-neutral-200);
}

.mc-cond--block > .mc-cond__head .mc-cond__text {
  font-weight: 600;
  color: var(--color-neutral-900);
}

/* the sentence wraps; the remove button stays at the end of its first line */
.mc-cond__head {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}

.mc-cond__line {
  flex: 1 1 auto;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px 8px;
  min-height: 32px;
}

.mc-cond__all {
  flex: 0 0 auto;
  width: 96px;
}

.mc-cond__excluding {
  margin-top: 6px;
}

.mc-cond__children {
  margin: 8px 0 4px 6px;
  padding-inline-start: 14px;
  border-inline-start: 2px solid var(--color-neutral-200);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.mc-cond__remove {
  flex: none;
  margin-top: 4px;
}

.mc-cond__add {
  margin-top: 6px;
}
</style>
