"use client";

import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/components/providers/LanguageProvider";

const benefits = [
  { icon: "◈", titleKey: "benefitProductsTitle" as const, descKey: "benefitProductsDesc" as const },
  { icon: "✈", titleKey: "benefitDeliveryTitle" as const, descKey: "benefitDeliveryDesc" as const },
  { icon: "◇", titleKey: "benefitPaymentsTitle" as const, descKey: "benefitPaymentsDesc" as const },
  { icon: "◌", titleKey: "benefitSupportTitle" as const, descKey: "benefitSupportDesc" as const },
] as const;

export function HeroSection() {
  const { translate } = useLanguage();

  return (
    <section className="hero container">
      <div className="hero-content">
        <p className="eyebrow">{translate("madeInArmenia")}</p>

        <h1>
          <span>{translate("heroTitle")}</span>
          <em>{translate("heroSubtitle")}</em>
        </h1>

        <p className="hero-description">{translate("heroDescription")}</p>

        <div className="hero-actions">
          <Link href="/products" className="button button-primary">
            {translate("startShopping")}
          </Link>

          <a href="#how" className="button button-secondary">
            <span>▶</span>
            <span>{translate("howItWorks")}</span>
          </a>
        </div>
      </div>

      <div className="hero-visual">
        <Image
          src="/images/hero-armenia.png"
          alt=""
          fill
          priority
          sizes="(max-width: 900px) 100vw, 50vw"
          className="hero-visual-image"
        />
      </div>

      <div className="hero-benefits">
        {benefits.map((benefit) => (
          <article key={benefit.titleKey} className="benefit">
            <span className="benefit-icon">{benefit.icon}</span>
            <div>
              <strong>{translate(benefit.titleKey)}</strong>
              <p>{translate(benefit.descKey)}</p>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
