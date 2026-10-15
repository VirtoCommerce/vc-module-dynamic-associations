<template>
  <VcBlade
    :title="options?.title"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="dyn-form">
        <VcCard :header="$t('DYNAMIC_ASSOCIATIONS.RULE.CATEGORIES')">
          <VcDataTable
            v-model:selection="selection"
            selection-mode="multiple"
            :items="categoryRows"
            state-key="DYNAMIC_ASSOCIATION_RULE_CATEGORIES"
            :empty-state="{ icon: 'lucide-folder', title: $t('DYNAMIC_ASSOCIATIONS.RULE.NO_CATEGORIES') }"
          >
            <VcColumn
              id="name"
              :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.NAME')"
              mobile-role="title"
              always-visible
            />
          </VcDataTable>
        </VcCard>

        <VcCard :header="$t('DYNAMIC_ASSOCIATIONS.RULE.PROPERTIES')">
          <div class="dyn-card">
            <VcHint v-if="rule.categoryIds.length && !available.length && !rule.properties.length">
              {{ $t("DYNAMIC_ASSOCIATIONS.RULE.NO_PROPERTIES") }}
            </VcHint>
            <template v-else>
              <VcHint>{{ $t("DYNAMIC_ASSOCIATIONS.RULE.PROPERTIES_HINT") }}</VcHint>
              <VcSelect
                :model-value="rule.properties.map((p) => p.name!)"
                :label="$t('DYNAMIC_ASSOCIATIONS.RULE.SELECT_PROPERTIES')"
                :placeholder="$t('DYNAMIC_ASSOCIATIONS.RULE.SELECT_PROPERTIES_PLACEHOLDER')"
                :options="propertyOptions"
                :disabled="!rule.categoryIds.length"
                multiple
                searchable
                @update:model-value="(v) => selectProperties((v as string[] | undefined) ?? [])"
              />
              <VcDynamicProperty
                v-for="property in rule.properties"
                :key="property.name"
                :data-property="property.name"
                :property="property"
                :model-value="valueOf(property)"
                :options-getter="loadDictionaries"
                :required="false"
                :multivalue="property.multivalue"
                :multilanguage="false"
                current-language="en-US"
                :value-type="property.valueType ?? 'ShortText'"
                :dictionary="property.dictionary"
                :name="property.name ?? ''"
                :display-names="property.displayNames"
                @update:model-value="(ev: unknown) => onChange(property, ev)"
              />
            </template>
          </div>
        </VcCard>
        <div class="dyn-rule__count">{{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.COUNT", { count: count ?? 0 }) }}</div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// rule-creation: the categories (Catalog's picker) and property values a rule's products must have, with the
// matching products count and preview (legacy catalog-items-select, step-select-properties and step-edit-properties
// blades); applied to the association by the caller
import { useAsync, useBlade, useBladeForm, useDynamicProperties, useFunctions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCard, VcColumn, VcContainer, VcDataTable, VcDynamicProperty, VcForm, VcHint, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import {
  clone,
  countProducts,
  getCategories,
  http,
  previewQuery,
  propertiesOf,
  type Category,
  type Rule,
  type RuleProperty,
} from "../api";

// the rule and its onApply come only in options, which a reload doesn't bring back
defineBlade({ url: "/dynamic-association-rule", name: "DynamicAssociationRule", routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeSelf } = useBlade<{
  title: string;
  catalogId?: string;
  rule: Rule;
  onApply: (rule: Rule) => void;
}>();
const { debounce } = useFunctions();

const rule = ref<Rule>(clone(options.value?.rule) ?? { categoryIds: [], properties: [] });
const categories = ref<Category[]>([]);
const names = ref<Record<string, string>>({});
const selection = ref<{ id: string; name: string }[]>([]);
const count = ref<number>();
const form = useBladeForm({
  data: rule,
  canSaveOverride: computed(() => rule.value.categoryIds.length > 0),
  closeConfirmMessage: computed(() => t("DYNAMIC_ASSOCIATIONS.ALERTS.CLOSE_RULE")),
});
form.setBaseline();

const categoryRows = computed(() => rule.value.categoryIds.map((id) => ({ id, name: names.value[id] ?? id })));

async function loadCategories() {
  categories.value = await getCategories(rule.value.categoryIds);
  categories.value.forEach((c) => (names.value[c.id] = c.name ?? c.id));
}
watch(() => rule.value.categoryIds.join(), loadCategories);

// the categories' product and variation properties
const available = computed(() => propertiesOf(categories.value));
const propertyOptions = computed(() =>
  [...available.value, ...rule.value.properties.filter((p) => !available.value.some((a) => a.name === p.name))].map((p) => ({
    id: p.name!,
    title: p.name!,
  })),
);

// legacy prepareProperties: filter values are optional, editable and, for dictionaries, multiple
function selectProperties(selected: string[]) {
  const kept = rule.value.properties.filter((p) => selected.includes(p.name!));
  const added = selected
    .filter((name) => !kept.some((p) => p.name === name))
    .map((name) => {
      const p = clone(available.value.find((a) => a.name === name)!);
      return { ...p, values: [], required: false, isReadOnly: false, multivalue: p.dictionary || p.multivalue, multilanguage: false };
    });
  rule.value.properties = [...kept, ...added];
}

const { getPropertyValue, setPropertyValue, loadDictionaries } = useDynamicProperties({
  searchDictionary: async (criteria) =>
    (await http<{ results?: never[] }>("POST", "/api/catalog/dictionaryitems/search", { ...criteria, take: criteria.take ?? 100 }))
      .results,
});

// VcMultivalue (non-dictionary multivalue) labels its chips by `title`, the property values carry `value`
const isFreeMultivalue = (p: RuleProperty) => !!p.multivalue && !p.dictionary;
function valueOf(property: RuleProperty) {
  const value = getPropertyValue(property as never, "en-US");
  return isFreeMultivalue(property) && Array.isArray(value)
    ? value.map((v) => ({ ...(v as object), title: (v as { value?: unknown }).value }))
    : value;
}
function onChange(property: RuleProperty, ev: unknown) {
  const change = (ev && typeof ev === "object" && "value" in ev ? { ...ev } : { value: ev }) as { value: unknown };
  if (isFreeMultivalue(property) && Array.isArray(change.value))
    change.value = change.value.map(({ title, ...v }: { title?: unknown; value?: unknown }) => ({ ...v, value: v.value ?? title }));
  setPropertyValue({ property, ...change } as Parameters<typeof setPropertyValue>[0]);
  // the search index holds a dictionary value's alias (legacy stored the alias as the value)
  if (property.dictionary) property.values = (property.values ?? []).map((v) => ({ ...v, value: v.alias ?? v.value }));
  rule.value.properties = [...rule.value.properties];
}

// legacy: the number of products in the current filter
const recount = debounce(async () => (count.value = await countProducts(options.value?.catalogId, rule.value)), 300);
watch(() => JSON.stringify(rule.value), recount);

function addCategories() {
  openBlade({
    name: "CatalogBrowse",
    param: options.value?.catalogId,
    options: {
      pick: {
        title: t("DYNAMIC_ASSOCIATIONS.RULE.PICK_CATEGORIES"),
        mode: "entries",
        types: ["category"],
        onPick: (entries: { id?: string; name?: string }[]) => {
          entries.forEach((e) => e.id && (names.value[e.id] = e.name ?? e.id));
          rule.value.categoryIds = [...new Set([...rule.value.categoryIds, ...entries.map((e) => e.id!).filter(Boolean)])];
        },
      },
    },
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: !form.canSave.value,
    async clickHandler() {
      options.value?.onApply(clone(rule.value));
      form.setBaseline();
      await closeSelf();
    },
  },
  {
    id: "addCategories",
    title: t("DYNAMIC_ASSOCIATIONS.RULE.ADD_CATEGORIES"),
    icon: "lucide-folder-plus",
    disabled: !options.value?.catalogId,
    clickHandler: addCategories,
  },
  {
    id: "removeCategories",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.REMOVE"),
    icon: "lucide-folder-minus",
    disabled: !selection.value.length,
    clickHandler() {
      const removed = selection.value.map((s) => s.id);
      rule.value.categoryIds = rule.value.categoryIds.filter((id) => !removed.includes(id));
      selection.value = [];
    },
  },
  {
    id: "preview",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.PREVIEW"),
    icon: "lucide-eye",
    disabled: !rule.value.categoryIds.length,
    clickHandler: () =>
      openBlade({ name: "DynamicAssociationPreview", options: { query: previewQuery(options.value?.catalogId, rule.value) } }),
  },
  {
    id: "reset",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value,
    clickHandler: () => form.revert(),
  },
]);

const { loading, action: init } = useAsync(async () => {
  await loadCategories();
  recount();
});
onMounted(init);
</script>
