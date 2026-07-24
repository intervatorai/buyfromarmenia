"use client";

import { ContentPage } from "@/components/content/ContentPage";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { getFooterPage } from "@/lib/content/footer-pages";

export default function ContactsPage() {
  const { language } = useLanguage();
  const content = getFooterPage("contacts", language);

  return (
    <ContentPage
      title={content.title}
      lead={content.lead}
      updated={content.updated}
      sections={content.sections}
      contacts={content.contacts}
      links={content.links}
    />
  );
}
