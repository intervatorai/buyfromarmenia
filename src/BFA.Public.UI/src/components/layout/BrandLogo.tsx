"use client";

import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/components/providers/LanguageProvider";

export function BrandLogo({ compact = false }: { compact?: boolean }) {
  const { translate } = useLanguage();

  return (
    <Link href="/" className={`brand${compact ? " brand-compact" : ""}`}>
      <Image
        src="/brand/logo-512.png"
        alt=""
        width={512}
        height={512}
        className="brand-logo"
        priority
      />
      <span className="brand-text">
        <strong>
          BuyFrom<span>Armenia</span>
        </strong>
        {!compact ? <small>{translate("brandTagline")}</small> : null}
      </span>
    </Link>
  );
}
