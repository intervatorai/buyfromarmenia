"use client";

import { ContentPage } from "@/components/content/ContentPage";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { getFooterPage } from "@/lib/content/footer-pages";

export default function DeliveryPaymentPage() {
  const { language } = useLanguage();
  const content = getFooterPage("deliveryPayment", language);

  return (
    <ContentPage
      title={content.title}
      lead={content.lead}
      updated={content.updated}
      sections={content.sections}
      links={content.links}
    />
  );
}
