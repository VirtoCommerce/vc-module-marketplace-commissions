// Small typed fetches to other modules' APIs (Catalog, MarketplaceVendor, Orders) used by the condition pickers and the
// order widget; same-origin cookie auth, as the generated clients. Same approach as the Marketing plugin's lookups.ts.

export interface Option {
  id: string;
  name: string;
  code?: string;
}
export interface Page {
  results: Option[];
  totalCount: number;
}

export async function http<T>(url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method: body === undefined ? "GET" : "POST",
    headers: body === undefined ? { Accept: "application/json" } : { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}: ${url}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

const qs = (name: string, values: string[]) => values.map((v) => `${name}=${encodeURIComponent(v)}`).join("&");
const PAGE = 20;

// The legacy pickers browsed the marketplace master catalog only; loaded once, retried after a failure
let masterCatalog: Promise<string | undefined> | undefined;
function masterCatalogId() {
  masterCatalog ??= http<{ masterCatalogId?: string }>("/api/vcmp/settings")
    .then((x) => x?.masterCatalogId)
    .catch(() => {
      masterCatalog = undefined;
      return undefined;
    });
  return masterCatalog;
}

type CatalogEntry = { id: string; name: string; code?: string };
const toOption = (x: CatalogEntry): Option => ({ id: x.id, name: x.name || x.id, code: x.code });

export async function searchCategories(keyword?: string, skip = 0, ids?: string[]): Promise<Page> {
  if (ids?.length && !keyword) {
    const items = await http<CatalogEntry[]>(`/api/catalog/categories?${qs("ids", ids)}&respGroup=Info`);
    return { results: (items ?? []).map(toOption), totalCount: items?.length ?? 0 };
  }
  const r = await http<{ items?: CatalogEntry[]; totalCount?: number }>("/api/catalog/search/categories", {
    catalogId: await masterCatalogId(),
    keyword: keyword || undefined,
    skip,
    take: PAGE,
  });
  return { results: (r.items ?? []).map(toOption), totalCount: r.totalCount ?? 0 };
}

export async function searchProducts(keyword?: string, skip = 0, ids?: string[]): Promise<Page> {
  if (ids?.length && !keyword) {
    const items = await http<CatalogEntry[]>(`/api/catalog/products?${qs("ids", ids)}&respGroup=ItemInfo`);
    return { results: (items ?? []).map(toOption), totalCount: items?.length ?? 0 };
  }
  const r = await http<{ items?: CatalogEntry[]; totalCount?: number }>("/api/catalog/search/products", {
    catalogId: await masterCatalogId(),
    keyword: keyword || undefined,
    skip,
    take: PAGE,
    responseGroup: "ItemInfo",
  });
  return { results: (r.items ?? []).map(toOption), totalCount: r.totalCount ?? 0 };
}

export async function searchVendors(keyword?: string, skip = 0, ids?: string[]): Promise<Page> {
  const byIds = !!ids?.length && !keyword;
  const r = await http<{ results?: { id: string; name?: string }[]; totalCount?: number }>("/api/vcmp/security/seller/search", {
    keyword: keyword || undefined,
    objectIds: byIds ? ids : undefined,
    skip: byIds ? 0 : skip,
    take: byIds ? ids!.length : PAGE,
  });
  return { results: (r.results ?? []).map((x) => ({ id: x.id, name: x.name || x.id })), totalCount: r.totalCount ?? 0 };
}

export interface OrderLineItem {
  id?: string;
  name?: string;
  sku?: string;
  currency?: string;
  fee?: number;
  feeDetails?: { feeId?: string; description?: string; amount?: number; currency?: string }[];
}

// the order's line items with their commission fees (Orders is another module: no generated client here)
export const getOrder = (id: string) =>
  http<{ id?: string; number?: string; currency?: string; items?: OrderLineItem[] }>(`/api/order/customerOrders/${encodeURIComponent(id)}`);
