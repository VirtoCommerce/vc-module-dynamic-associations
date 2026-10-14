import { AssociationsClient, type Association } from "../api_client/virtocommerce.dynamicassociationsmodule";

// The generated client is a plain fetch wrapper (same-origin cookie auth), so one instance is enough.
export const client = new AssociationsClient();

/** Small JSON fetch for other modules' endpoints (stores, catalog categories and products). */
export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: { Accept: "application/json", ...(body !== undefined ? { "Content-Type": "application/json" } : {}) },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

/** "ids=a&ids=b" for the GET endpoints of other modules */
export const idsQuery = (ids: string[]) => ids.map((id) => "ids=" + encodeURIComponent(id)).join("&");

/** Deep copy that also works on reactive proxies and blade options. */
export const clone = <T>(x: T): T => (x === undefined ? x : JSON.parse(JSON.stringify(x)));

// ---- the rule tree: three blocks, each rule block holds a category condition and a property values condition
export const MATCHING = "BlockMatchingRules";
export const RESULTING = "BlockResultingRules";
export const TUNING = "BlockOutputTuning";
const CATEGORY = "ConditionProductCategory";
const PROPERTIES = "ConditionPropertyValues";

export interface PropertyValue {
  value?: unknown;
  valueId?: string;
  alias?: string;
  languageCode?: string;
  propertyId?: string;
  propertyName?: string;
  valueType?: string;
}
/** A catalog property (from the categories) with the values a product must have */
export interface RuleProperty {
  id?: string;
  name?: string;
  type?: string;
  valueType?: string;
  dictionary?: boolean;
  multivalue?: boolean;
  multilanguage?: boolean;
  required?: boolean;
  isReadOnly?: boolean;
  measureId?: string;
  displayNames?: { name?: string; languageCode?: string }[];
  values?: PropertyValue[];
}
interface Node {
  id?: string;
  children?: Node[];
  availableChildren?: Node[];
  catalogId?: string | null;
  categoryIds?: string[] | null;
  properties?: RuleProperty[] | null;
  sort?: string | null;
  outputLimit?: number;
}
export interface Rule {
  categoryIds: string[];
  properties: RuleProperty[];
}

export const block = (a: Association, id: string) => (a.expressionTree?.children as Node[] | undefined)?.find((b) => b.id === id);
const condition = (b: Node | undefined, id: string) => b?.children?.find((c) => c.id === id);

export function getRule(a: Association, blockId: string): Rule {
  const b = block(a, blockId);
  return { categoryIds: condition(b, CATEGORY)?.categoryIds ?? [], properties: condition(b, PROPERTIES)?.properties ?? [] };
}

/** legacy checkExistingRules: a rule exists when its category condition has a catalog and categories */
export function hasRule(a: Association, blockId: string) {
  const c = condition(block(a, blockId), CATEGORY);
  return !!c?.catalogId && !!c.categoryIds?.length;
}

/** legacy getCondition + onSelected: the block's conditions are created on first use */
export function setRule(a: Association, blockId: string, catalogId: string | undefined, rule: Rule) {
  const b = block(a, blockId)!;
  b.children ??= [];
  const ensure = (id: string, init: Node) => condition(b, id) ?? (b.children!.push({ id, availableChildren: [], children: [], ...init }), condition(b, id)!);
  Object.assign(ensure(CATEGORY, {}), { catalogId, categoryIds: rule.categoryIds });
  Object.assign(ensure(PROPERTIES, {}), { properties: rule.properties });
}

/** legacy mainParameters onSelected: another catalog makes the categories meaningless */
export function clearRules(a: Association) {
  for (const id of [MATCHING, RESULTING]) {
    const b = block(a, id);
    if (b) b.children = [];
  }
}

export const ruleCatalogId = (a: Association, blockId: string) => condition(block(a, blockId), CATEGORY)?.catalogId ?? undefined;

/** The preview/count request of a rule (legacy prepareQuery); the server reads property values as strings */
export function previewQuery(catalogId: string | undefined, rule: Rule) {
  return {
    catalogId,
    categoryIds: rule.categoryIds,
    propertyValues: Object.fromEntries(
      rule.properties.map((p) => [p.name!, (p.values ?? []).filter((v) => v.value != null && v.value !== "").map((v) => String(v.value))]),
    ),
  };
}
export type PreviewQuery = ReturnType<typeof previewQuery>;

/** legacy GetMatchingProductsCount: the number of products of a rule (0 without categories, none when the search fails) */
export const countProducts = async (catalogId: string | undefined, rule: Rule) =>
  rule.categoryIds.length
    ? (await client.previewDynamicAssociations({ ...previewQuery(catalogId, rule), take: 0 }).catch(() => undefined))?.totalCount
    : 0;

// ---- output tuning: sort = "field:dir;field:dir;" (legacy format), two levels
export interface SortLevel {
  field?: string;
  direction: "asc" | "desc";
}
export function parseSort(sort?: string | null): SortLevel[] {
  const levels = (sort ?? "")
    .split(";")
    .filter(Boolean)
    .map((pair) => {
      const [field, direction] = pair.split(":");
      return { field, direction: direction === "desc" ? "desc" : "asc" } as SortLevel;
    });
  return [0, 1].map((i) => levels[i] ?? { direction: "asc" });
}
export const formatSort = (levels: SortLevel[]) => levels.filter((l) => l.field).map((l) => `${l.field}:${l.direction};`).join("");

// ---- other modules' data
export interface Category {
  id: string;
  name?: string;
  properties?: RuleProperty[];
}
const categoryCache = new Map<string, Category>();
/** Categories with their (inherited) properties; legacy categories.getByIds */
export async function getCategories(ids: string[]): Promise<Category[]> {
  const missing = ids.filter((id) => !categoryCache.has(id));
  if (missing.length) {
    const found = await http<Category[]>("GET", "/api/catalog/categories?" + idsQuery(missing)).catch(() => []);
    (found ?? []).forEach((c) => categoryCache.set(c.id, c));
  }
  return ids.map((id) => categoryCache.get(id)).filter((c): c is Category => !!c);
}

/** Product and variation properties of the categories, one per name (legacy prepareProperties) */
export function propertiesOf(categories: Category[]): RuleProperty[] {
  const byName = new Map<string, RuleProperty>();
  for (const p of categories.flatMap((c) => c.properties ?? []))
    if (p.name && (p.type === "Product" || p.type === "Variation") && !byName.has(p.name)) byName.set(p.name, p);
  return [...byName.values()].sort((a, b) => a.name!.localeCompare(b.name!));
}

export interface Store {
  id: string;
  name?: string;
  catalog?: string;
}
const storeCache = new Map<string, Promise<Store | undefined>>();
export function getStore(id: string) {
  // a failed lookup (undefined) isn't cached: the next one asks again
  if (!storeCache.has(id))
    storeCache.set(
      id,
      http<Store>("GET", `/api/stores/${encodeURIComponent(id)}`).catch(() => {
        storeCache.delete(id);
        return undefined;
      }),
    );
  return storeCache.get(id)!;
}
export function searchStores(keyword?: string, skip = 0, ids?: string[]) {
  return http<{ totalCount?: number; results?: Store[] }>("POST", "/api/stores/search", { keyword, objectIds: ids, skip, take: ids?.length || 20 });
}

/**
 * Values of a global platform setting (Catalog.AssociationGroups), as legacy settings.getValues: the values endpoint
 * needs no setting permission; a failed lookup is no values
 */
export async function settingValues(name: string): Promise<string[]> {
  return ((await http<unknown[]>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`).catch(() => undefined)) ?? []).map(String);
}
