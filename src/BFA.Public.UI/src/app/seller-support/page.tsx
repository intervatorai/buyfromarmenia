"use client";

import { ContentPage } from "@/components/content/ContentPage";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { getFooterPage } from "@/lib/content/footer-pages";

const SUPPLIER_PORTAL_URL = (
  process.env.NEXT_PUBLIC_SUPPLIER_URL ?? ""
).replace(/\/$/, "");

export default function SellerSupportPage() {
  const { language } = useLanguage();
  const content = getFooterPage("sellerSupport", language);

  const links = [
    ...(content.links ?? []),
    ...(SUPPLIER_PORTAL_URL
      ? [
          {
            href: SUPPLIER_PORTAL_URL,
            label: language === "hy" ? "Բացել գործընկերային պորտալը" : "Open partner portal",
            external: true as const,
          },
        ]
      : []),
  ];

  return (
    <ContentPage
      title={content.title}
      lead={content.lead}
      updated={content.updated}
      sections={content.sections}
      contacts={content.contacts}
      links={links}
    />
  );
}
