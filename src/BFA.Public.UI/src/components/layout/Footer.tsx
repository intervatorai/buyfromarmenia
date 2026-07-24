"use client";

import { FormEvent } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { BrandLogo } from "./BrandLogo";

const SUPPLIER_PORTAL_URL = (
  process.env.NEXT_PUBLIC_SUPPLIER_URL ?? ""
).replace(/\/$/, "");

export function Footer() {
  const { translate } = useLanguage();

  function handleNewsletter(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const form = event.currentTarget;
    const input = form.querySelector("input");

    if (input instanceof HTMLInputElement && input.value.trim()) {
      alert(translate("subscribeThanks"));
      input.value = "";
    }
  }

  return (
    <footer className="footer" id="help">
      <div className="container footer-grid">
        <div>
          <BrandLogo />
        </div>

        <div>
          <h3>{translate("forCustomers")}</h3>
          <a href="/#how">{translate("howItWorks")}</a>
          <a href="/how-to-order">{translate("howToOrder")}</a>
          <a href="/delivery-payment">{translate("deliveryPayment")}</a>
          <a href="/returns">{translate("returns")}</a>
        </div>

        <div>
          <h3>{translate("forSellers")}</h3>
          <a href="/sell">{translate("sellOnPlatform")}</a>
          {SUPPLIER_PORTAL_URL ? (
            <a href={SUPPLIER_PORTAL_URL} target="_blank" rel="noopener noreferrer">
              {translate("partnerPortal")}
            </a>
          ) : null}
          <a href="/seller-terms">{translate("sellerTerms")}</a>
          <a href="/seller-support">{translate("sellerSupport")}</a>
        </div>

        <div>
          <h3>{translate("company")}</h3>
          <a href="/about">{translate("aboutUs")}</a>
          <a href="/contacts">{translate("contacts")}</a>
          <a href="/privacy">{translate("privacyPolicy")}</a>
        </div>

        <div>
          <h3>{translate("stayInformed")}</h3>
          <p>{translate("newsletterDesc")}</p>

          <form className="newsletter" onSubmit={handleNewsletter}>
            <input
              type="email"
              placeholder={translate("emailPlaceholder")}
              aria-label={translate("emailPlaceholder")}
            />
            <button type="submit">→</button>
          </form>
        </div>
      </div>

      <div className="container footer-bottom">
        <span>© {new Date().getFullYear()} BuyFromArmenia</span>
        <span>{translate("allRightsReserved")}</span>
      </div>
    </footer>
  );
}
