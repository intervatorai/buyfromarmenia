"use client";

import { useLanguage } from "@/components/providers/LanguageProvider";

export function Topbar() {
  const { translate } = useLanguage();

  return (
    <div className="topbar">
      <div className="container topbar-inner">
        <span>{translate("topbarDelivery")}</span>

        <div className="topbar-links">
          <a href="/about">{translate("aboutUs")}</a>
          <a href="#help">{translate("help")}</a>
        </div>
      </div>
    </div>
  );
}
