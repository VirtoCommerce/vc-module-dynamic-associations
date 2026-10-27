<template>
  <VcBlade
    :title="$t('DYNAMIC_ASSOCIATIONS.PREVIEW.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="keyword"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      state-key="DYNAMIC_ASSOCIATION_PREVIEW"
      :empty-state="{ icon: 'lucide-package', title: $t('DYNAMIC_ASSOCIATIONS.PREVIEW.EMPTY') }"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="imgSrc"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.IMAGE')"
        type="image"
        width="84px"
        mobile-role="image"
        always-visible
      />
      <VcColumn
        id="name"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.PRODUCT')"
        mobile-role="title"
        always-visible
      />
      <VcColumn
        id="code"
        :title="$t('DYNAMIC_ASSOCIATIONS.FIELDS.CODE')"
        mobile-role="field"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
// dynamicAssociation-viewer: the products a rule selects (preview → product ids → products)
import { useAsync, useBlade, useDataTablePagination, useFunctions, useLatestRequest, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { client, http, idsQuery, type PreviewQuery } from "../api";

// the rule comes only in options, which a reload doesn't bring back
defineBlade({ url: "/dynamic-association-preview", name: "DynamicAssociationPreview", routable: false });

interface Product {
  id: string;
  name?: string;
  code?: string;
  imgSrc?: string;
}

const PAGE_SIZE = 20;
const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ query: PreviewQuery }>();
const { debounce } = useFunctions();

const items = ref<Product[]>([]);
const totalCount = ref(0);
const keyword = ref("");
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });
const search = useLatestRequest();

// relative asset urls would resolve under /apps/admin-shell/
const assetUrl = (url?: string) => (!url || /^([a-z][a-z0-9+.-]*:|\/)/i.test(url) ? url : "/" + url);

const { loading, action: load } = useAsync(async () => {
  if (!options.value?.query) return;
  const found = await search.latest(
    client.previewDynamicAssociations({ ...options.value.query, keyword: keyword.value || undefined, skip: pagination.skip, take: PAGE_SIZE }),
  );
  if (!found) return;
  const ids = found.results ?? [];
  const products = ids.length
    ? await http<Product[]>("GET", "/api/catalog/products?respGroup=ItemInfo,ItemAssets&" + idsQuery(ids))
    : [];
  items.value = ids
    .map((id) => products.find((p) => p.id === id))
    .filter((p): p is Product => !!p)
    .map((p) => ({ ...p, imgSrc: assetUrl(p.imgSrc) }));
  totalCount.value = found.totalCount ?? 0;
});

load();
watch(keyword, () => pagination.setPage(1));
watch([keyword, () => pagination.skip], debounce(load, 300));

const toolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("DYNAMIC_ASSOCIATIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);
</script>
