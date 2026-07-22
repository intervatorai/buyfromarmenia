const CART_ID_KEY = "bfa_cart_id";
export const CART_UPDATED_EVENT = "bfa-cart-updated";

export function getCartId(): string {
  if (typeof window === "undefined") {
    return "";
  }

  const existing = localStorage.getItem(CART_ID_KEY);
  if (existing) {
    return existing;
  }

  const id = crypto.randomUUID();
  localStorage.setItem(CART_ID_KEY, id);
  return id;
}

export function notifyCartUpdated() {
  window.dispatchEvent(new Event(CART_UPDATED_EVENT));
}
