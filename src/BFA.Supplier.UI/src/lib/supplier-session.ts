const SUPPLIER_ID_KEY = "bfa_supplier_id";

export function getSupplierId(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return localStorage.getItem(SUPPLIER_ID_KEY);
}

export function setSupplierId(id: string): void {
  localStorage.setItem(SUPPLIER_ID_KEY, id);
}

export function clearSupplierId(): void {
  localStorage.removeItem(SUPPLIER_ID_KEY);
}
