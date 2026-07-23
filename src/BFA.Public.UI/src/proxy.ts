import { NextResponse, type NextRequest } from "next/server";

const AUTH_COOKIE_NAME = "bfa_customer_token";

/**
 * Request-level guard for customer-only pages. API authorization remains
 * the source of truth; this proxy provides an immediate login redirect.
 */
export function proxy(request: NextRequest) {
  const token = request.cookies.get(AUTH_COOKIE_NAME)?.value;

  if (!token) {
    const loginUrl = new URL("/account/login", request.url);
    loginUrl.searchParams.set(
      "returnTo",
      request.nextUrl.pathname + request.nextUrl.search,
    );
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/account",
    "/account/addresses/:path*",
    "/cart/:path*",
    "/wishlist/:path*",
    "/orders/:path*",
    "/checkout/:path*",
  ],
};
