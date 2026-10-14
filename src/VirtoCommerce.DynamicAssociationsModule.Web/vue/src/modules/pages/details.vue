<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="dyn-form">
        <VcCard :header="$t('DYNAMIC_ASSOCIATIONS.DETAILS.GENERAL')">
          <div class="dyn-card">
            <VcInput
              v-model="item.name"
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.NAME')"
              :placeholder="$t('DYNAMIC_ASSOCIATIONS.FIELDS.NAME_PLACEHOLDER')"
              :hint="$t('DYNAMIC_ASSOCIATIONS.FIELDS.NAME_HINT')"
              :maxlength="128"
              required
            />
            <VcTextarea
              v-model="item.description"
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.DESCRIPTION')"
            />
          </div>
        </VcCard>

        <!-- legacy mainParameters blade -->
        <VcCard :header="$t('DYNAMIC_ASSOCIATIONS.DETAILS.MAIN_PARAMETERS')">
          <div class="dyn-card">
            <VcSwitch
              v-model="item.isActive"
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.IS_ACTIVE')"
              :hint="$t('DYNAMIC_ASSOCIATIONS.FIELDS.IS_ACTIVE_HINT')"
            />
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcDatePicker
                  v-model="item.startDate"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.START_DATE')"
                  clearable
                />
              </VcCol>
              <VcCol>
                <VcDatePicker
                  v-model="item.endDate"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.END_DATE')"
                  clearable
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  :model-value="item.storeId"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.STORE')"
                  :placeholder="$t('DYNAMIC_ASSOCIATIONS.FIELDS.STORE_PLACEHOLDER')"
                  :hint="$t('DYNAMIC_ASSOCIATIONS.FIELDS.STORE_HINT')"
                  :options="searchStores"
                  option-value="id"
                  option-label="name"
                  :clearable="false"
                  searchable
                  required
                  @update:model-value="onStore"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.associationType"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.GROUP')"
                  :placeholder="$t('DYNAMIC_ASSOCIATIONS.FIELDS.GROUP_PLACEHOLDER')"
                  :options="groups"
                  :clearable="false"
                  required
                >
                  <template
                    v-if="hasAccess('platform:setting:update')"
                    #append
                  >
                    <VcButton
                      icon="lucide-pencil"
                      icon-size="s"
                      text
                      :aria-label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.EDIT_GROUPS')"
                      @click="openGroups"
                    />
                  </template>
                </VcSelect>
              </VcCol>
              <VcCol>
                <VcInput
                  :model-value="item.priority"
                  type="number"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.PRIORITY')"
                  :hint="$t('DYNAMIC_ASSOCIATIONS.FIELDS.PRIORITY_HINT')"
                  required
                  @update:model-value="(v) => (item.priority = v === '' || v == null ? undefined : Math.trunc(Number(v)))"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <!-- legacy "Products to match" / "Products to display" items, each opening the rule blade -->
        <VcCard
          v-for="r in rules"
          :key="r.id"
          :header="r.title"
        >
          <div
            class="dyn-card"
            :data-rule="r.id"
          >
            <VcHint>{{ r.locked || r.hint }}</VcHint>
            <VcField
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.CATEGORIES')"
              :model-value="r.categories || $t('DYNAMIC_ASSOCIATIONS.DETAILS.NOT_SET')"
            />
            <VcField
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.PROPERTIES')"
              :model-value="r.properties || '—'"
            />
            <div class="dyn-rule__footer">
              <span class="dyn-rule__count">{{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.COUNT", { count: r.count ?? 0 }) }}</span>
              <div class="dyn-rule__actions">
                <VcButton
                  size="sm"
                  variant="secondary"
                  icon="lucide-eye"
                  :disabled="!r.exists"
                  @click="openPreview(r.id)"
                >
                  {{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.PREVIEW") }}
                </VcButton>
                <VcButton
                  size="sm"
                  icon="lucide-pencil"
                  :disabled="!!r.locked"
                  @click="openRule(r.id)"
                >
                  {{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.EDIT_RULE") }}
                </VcButton>
              </div>
            </div>
          </div>
        </VcCard>

        <!-- legacy outputTuning blade: two sort levels by a property of the products to display, and the limit -->
        <VcCard :header="$t('DYNAMIC_ASSOCIATIONS.DETAILS.OUTPUT_TUNING')">
          <div class="dyn-card">
            <VcHint>{{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.OUTPUT_TUNING_HINT") }}</VcHint>
            <VcRow
              v-for="(level, i) in sortLevels"
              :key="i"
              class="tw-gap-4"
            >
              <VcCol>
                <VcSelect
                  :model-value="level.field"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.SORT_BY')"
                  :placeholder="$t('DYNAMIC_ASSOCIATIONS.FIELDS.SORT_BY_PLACEHOLDER')"
                  :options="sortOptions"
                  :disabled="!tuning || (i === 1 && !sortLevels[0].field)"
                  searchable
                  @update:model-value="(v) => setSort(i, { field: (v as string) || undefined })"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  :model-value="level.direction"
                  :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.DIRECTION')"
                  :options="directions"
                  :clearable="false"
                  :disabled="!tuning || (i === 1 && !sortLevels[0].field)"
                  @update:model-value="(v) => setSort(i, { direction: v as 'asc' | 'desc' })"
                />
              </VcCol>
            </VcRow>
            <VcInput
              :model-value="tuning?.outputLimit"
              type="number"
              :label="$t('DYNAMIC_ASSOCIATIONS.FIELDS.LIMIT')"
              :disabled="!tuning"
              required
              @update:model-value="(v) => tuning && (tuning.outputLimit = Math.max(0, Math.trunc(Number(v) || 0)))"
            />
          </div>
        </VcCard>
        <VcHint v-if="!canSaveRule">{{ $t("DYNAMIC_ASSOCIATIONS.DETAILS.SAVE_HINT") }}</VcHint>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// dynamicAssociations-detail with its mainParameters and outputTuning blades as cards; the two rules open DynamicAssociationRule
import { useAsync, useBlade, useBladeForm, useFunctions, useLoading, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcButton,
  VcCard,
  VcCol,
  VcContainer,
  VcDatePicker,
  VcField,
  VcForm,
  VcHint,
  VcInput,
  VcRow,
  VcSelect,
  VcSwitch,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { Association } from "../../api_client/virtocommerce.dynamicassociationsmodule";
import {
  MATCHING,
  RESULTING,
  TUNING,
  block,
  clearRules,
  client,
  clone,
  formatSort,
  getCategories,
  getRule,
  getStore,
  hasRule,
  countProducts,
  parseSort,
  previewQuery,
  propertiesOf,
  ruleCatalogId,
  searchStores,
  setRule,
  settingValues,
  type Category,
  type Rule,
  type SortLevel,
} from "../api";

defineBlade({ url: "/dynamic-association", name: "DynamicAssociationDetails" });

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, replaceWith } = useBlade();
const { hasAccess } = usePermissions();
const { debounce } = useFunctions();

const item = ref<Association>({});
const catalogId = ref<string>();
const groups = ref<{ id: string; title: string }[]>([]);
const categories = ref<Category[]>([]);
const counts = ref<Record<string, number | undefined>>({});
const isNew = computed(() => !item.value.id);
const title = computed(() => (isNew.value ? t("DYNAMIC_ASSOCIATIONS.DETAILS.NEW") : item.value.name || ""));

const matchExists = computed(() => hasRule(item.value, MATCHING));
const displayExists = computed(() => hasRule(item.value, RESULTING));
// legacy canSave: name, store, group, priority and both rules
const canSaveRule = computed(
  () =>
    !!item.value.name?.trim() &&
    !!item.value.storeId &&
    !!item.value.associationType &&
    item.value.priority != null &&
    matchExists.value &&
    displayExists.value,
);
const form = useBladeForm({
  data: item,
  canSaveOverride: canSaveRule,
  closeConfirmMessage: computed(() => t("DYNAMIC_ASSOCIATIONS.ALERTS.CLOSE")),
});

const { loading: fetching, action: load } = useAsync(async () => {
  item.value = param.value ? await client.getAssociationById(param.value) : await client.getNewAssociation();
  form.setBaseline();
});

const { loading: saving, action: save } = useAsync(async () => {
  const [saved] = await client.saveAssociations([item.value]);
  callParent("reload");
  if (isNew.value && saved?.id) {
    form.setBaseline();
    await replaceWith({ name: "DynamicAssociationDetails", param: saved.id });
  } else await load();
});
const loading = useLoading(fetching, saving);

// the store's catalog scopes the categories of both rules (legacy: stores.get on load, onStoreSelected on change)
watch(
  () => item.value.storeId,
  async (id) => (catalogId.value = id ? (await getStore(id))?.catalog : undefined),
);
async function onStore(id: unknown) {
  const storeId = (id as string) || undefined;
  const store = storeId ? await getStore(storeId) : undefined;
  const catalog = store?.catalog;
  // legacy: another catalog clears the rules; a failed store lookup knows no catalog, so it keeps them
  if ((!storeId || store) && catalog !== (ruleCatalogId(item.value, MATCHING) ?? ruleCatalogId(item.value, RESULTING) ?? catalog))
    clearRules(item.value);
  item.value.storeId = storeId;
  // re-selecting the same store after a failed lookup doesn't trigger the storeId watch
  catalogId.value = catalog;
}

const loadGroups = async () =>
  (groups.value = (await settingValues("Catalog.AssociationGroups")).map((g) => ({ id: g, title: g })));
const openGroups = () => openBlade({ name: "PlatformSettingDictionary", param: "Catalog.AssociationGroups", onClose: loadGroups });

// category names (rule summaries) and properties (sorting options) of both rules
const ruleOf = (id: string) => getRule(item.value, id);
watch(
  () => [...ruleOf(MATCHING).categoryIds, ...ruleOf(RESULTING).categoryIds].join(),
  async () => (categories.value = await getCategories([...ruleOf(MATCHING).categoryIds, ...ruleOf(RESULTING).categoryIds])),
);
const names = (ids: string[]) => ids.map((id) => categories.value.find((c) => c.id === id)?.name ?? id).join(", ");
const valuesText = (r: Rule) =>
  r.properties
    .map((p) => `${p.name}: ${(p.values ?? []).map((v) => String(v.value)).filter(Boolean).join(", ") || "—"}`)
    .join("; ");

// the number of products of each rule, recounted when that rule changes
const catalogOf = (id: string) => ruleCatalogId(item.value, id) ?? catalogId.value;
for (const id of [MATCHING, RESULTING]) {
  watch(
    () => JSON.stringify(ruleOf(id)),
    debounce(async () => (counts.value[id] = await countProducts(catalogOf(id), ruleOf(id))), 300),
  );
}

const rules = computed(() =>
  [
    { id: MATCHING, title: t("DYNAMIC_ASSOCIATIONS.DETAILS.MATCHING"), hint: t("DYNAMIC_ASSOCIATIONS.DETAILS.MATCHING_HINT"), exists: matchExists.value },
    { id: RESULTING, title: t("DYNAMIC_ASSOCIATIONS.DETAILS.DISPLAY"), hint: t("DYNAMIC_ASSOCIATIONS.DETAILS.DISPLAY_HINT"), exists: displayExists.value },
  ].map((r) => ({
    ...r,
    categories: names(ruleOf(r.id).categoryIds),
    properties: valuesText(ruleOf(r.id)),
    count: counts.value[r.id],
    // legacy: the rules need the store's catalog, the display rule needs the match rule
    locked: !catalogId.value
      ? t("DYNAMIC_ASSOCIATIONS.DETAILS.STORE_FIRST")
      : r.id === RESULTING && !matchExists.value
        ? t("DYNAMIC_ASSOCIATIONS.DETAILS.DISPLAY_LOCKED")
        : "",
  })),
);

function openRule(id: string) {
  openBlade({
    name: "DynamicAssociationRule",
    options: {
      title: t(id === MATCHING ? "DYNAMIC_ASSOCIATIONS.RULE.TITLE_MATCHING" : "DYNAMIC_ASSOCIATIONS.RULE.TITLE_DISPLAY"),
      catalogId: catalogId.value,
      rule: clone(ruleOf(id)),
      onApply: (rule: Rule) => setRule(item.value, id, catalogId.value, rule),
    },
  });
}
const openPreview = (id: string) =>
  openBlade({ name: "DynamicAssociationPreview", options: { query: previewQuery(catalogOf(id), ruleOf(id)) } });

// output tuning
const tuning = computed(() => block(item.value, TUNING));
const sortLevels = computed(() => parseSort(tuning.value?.sort));
function setSort(i: number, change: Partial<SortLevel>) {
  const levels = sortLevels.value.map((l, n) => (n === i ? { ...l, ...change } : l));
  tuning.value!.sort = formatSort(levels);
}
// legacy: the properties of the display rule's categories
const sortOptions = computed(() =>
  propertiesOf(categories.value.filter((c) => ruleOf(RESULTING).categoryIds.includes(c.id))).map((p) => ({ id: p.name!, title: p.name! })),
);
const directions = [
  { id: "asc", title: "asc" },
  { id: "desc", title: "desc" },
];

const bladeToolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t(isNew.value ? "DYNAMIC_ASSOCIATIONS.TOOLBAR.CREATE" : "DYNAMIC_ASSOCIATIONS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: isNew.value ? "dynamic-association:create" : "dynamic-association:update",
    disabled: !form.canSave.value,
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: !form.isModified.value,
    clickHandler: () => form.revert(),
  },
]);

onMounted(() => {
  load();
  loadGroups();
});
</script>

