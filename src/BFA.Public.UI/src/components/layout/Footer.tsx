"use client";

import { FormEvent } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { BrandLogo } from "./BrandLogo";

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
          <a href="#">{translate("howToOrder")}</a>
          <a href="#">{translate("deliveryPayment")}</a>
          <a href="#">{translate("returns")}</a>
        </div>

        <div>
          <h3>{translate("forSellers")}</h3>
          <a href="/sell">{translate("sellOnPlatform")}</a>
          <a href="#">{translate("sellerTerms")}</a>
          <a href="#">{translate("sellerSupport")}</a>
        </div>

        <div>
          <h3>{translate("company")}</h3>
          <a href="#">{translate("aboutUs")}</a>
          <a href="#">{translate("contacts")}</a>
          <a href="#">{translate("privacyPolicy")}</a>
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
