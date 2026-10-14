<template>
  <VcBlade
    :title="$t('DYNAMIC_ASSOCIATIONS.MENU.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading || deleting"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :search-placeholder="$t('DYNAMIC_ASSOCIATIONS.LIST.SEARCH')"
      state-key="DYNAMIC_ASSOCIATIONS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :row-actions="rowActions"
      row-actions-mode="dropdown"
      row-actions-position="column"
      :empty-state="{ icon: 'lucide-git-compare-arrows', title: $t('DYNAMIC_ASSOCIATIONS.LIST.EMPTY') }"
      @row-click="(e: { data: Association }) => openDetails(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.NAME')"
        mobile-role="title"
        sortable
        always-visible
      />
      <VcColumn
        id="associationType"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.GROUP')"
        mobile-role="field"
        sortable
      />
      <VcColumn
        id="priority"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.PRIORITY')"
        type="number"
        mobile-role="field"
        sortable
      />
      <VcColumn
        id="isActive"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.IS_ACTIVE')"
        mobile-role="status"
        sortable
        always-visible
      >
        <template #body="{ data }">
          <div class="dash-inline">
            <VcStatus :variant="data.isActive ? 'success' : 'info'">
              {{ $t(data.isActive ? "DYNAMIC_ASSOCIATIONS.FIELDS.ACTIVE" : "DYNAMIC_ASSOCIATIONS.FIELDS.INACTIVE") }}
            </VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="modifiedDate"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.MODIFIED')"
        type="date"
        mobile-role="field"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// dynamicAssociations-list: the rules (legacy Marketing blade item); add, open, delete (toolbar and row menu)
import {
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  useLatestRequest,
  usePermissions,
  usePopup,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { Association } from "../../api_client/virtocommerce.dynamicassociationsmodule";
import { client } from "../api";

defineBlade({
  url: "/dynamic-associations",
  name: "DynamicAssociationsList",
  isWorkspace: true,
  permissions: ["dynamic-association:access"],
  menuItem: {
    title: "DYNAMIC_ASSOCIATIONS.MENU.TITLE",
    icon: "lucide-git-compare-arrows",
    priority: 12,
  },
});

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren } = useBlade();
const { showConfirmation } = usePopup();
const { hasAccess } = usePermissions();
const { debounce } = useFunctions();

const items = ref<Association[]>([]);
const totalCount = ref(0);
const selection = ref<Association[]>([]);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "DYNAMIC_ASSOCIATIONS" });
const { searchValue } = useTableSearch({ stateKey: "DYNAMIC_ASSOCIATIONS" });
const pagination = useDataTablePagination({ stateKey: "DYNAMIC_ASSOCIATIONS", pageSize: PAGE_SIZE, totalCount });
const search = useLatestRequest();

const { loading, action: load } = useAsync(async () => {
  const r = await search.latest(
    client.searchAssociation({ keyword: searchValue.value || undefined, sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE }),
  );
  if (!r) return;
  items.value = r.results ?? [];
  totalCount.value = r.totalCount ?? 0;
});

load();
watch(searchValue, () => pagination.setPage(1));
watch([sortExpression, searchValue, () => pagination.skip], debounce(load, 300));

function openDetails(id?: string) {
  openBlade({
    name: "DynamicAssociationDetails",
    param: id,
    onOpen: () => (activeItemId.value = id),
    onClose: () => (activeItemId.value = undefined),
  });
}

const { loading: deleting, action: deleteItems } = useAsync(async (ids?: string[]) => client.deleteAssociation(ids ?? []));

async function remove(list: Association[]) {
  if (!(await showConfirmation(t("DYNAMIC_ASSOCIATIONS.ALERTS.DELETE")))) return;
  await closeChildren();
  // a failed delete is shown by useAsync; either way the list shows what is left
  await deleteItems(list.map((x) => x.id!)).catch(() => undefined);
  selection.value = [];
  await load();
}

// legacy: the row context menu's Delete
const rowActions = (item: Association) => [
  {
    id: "delete",
    icon: "lucide-trash-2",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.DELETE"),
    type: "danger" as const,
    variant: "danger" as const,
    hidden: !hasAccess("dynamic-association:delete"),
    clickHandler: () => remove([item]),
  },
];

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "dynamic-association:create",
    async clickHandler() {
      await closeChildren();
      openDetails();
    },
  },
  {
    id: "delete",
    title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "dynamic-association:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => remove(selection.value),
  },
]);

exposeToChildren({ reload: () => load() });
</script>
