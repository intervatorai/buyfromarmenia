"use client";

import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/components/providers/LanguageProvider";

export function SellerSection() {
  const { translate } = useLanguage();

  return (
    <section className="seller container" id="about">
      <div className="seller-text">
        <p className="eyebrow">{translate("sellGlobally")}</p>
        <h2>{translate("sellerTitle")}</h2>
        <p className="seller-description">{translate("sellerDescription")}</p>

        <Link href="/sell" className="button button-primary">
          {translate("becomeSeller")}
        </Link>
      </div>

      <div className="seller-visual" aria-hidden="true">
        <Image
          src="/images/seller-armenia.png"
          alt=""
          fill
          sizes="(max-width: 900px) 100vw, 42vw"
          className="seller-visual-image"
        />
      </div>
    </section>
  );
}
