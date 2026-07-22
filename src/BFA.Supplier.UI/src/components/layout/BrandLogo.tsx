import Image from "next/image";
import Link from "next/link";

export function BrandLogo({
  href = "/",
  subtitle = "Supplier Portal",
  showSubdomain = true,
  compact = false,
}: {
  href?: string;
  subtitle?: string;
  showSubdomain?: boolean;
  compact?: boolean;
}) {
  return (
    <Link href={href} className="supplier-brand">
      <Image
        src="/brand/logo-512.png"
        alt="BuyFromArmenia"
        width={512}
        height={512}
        className={`supplier-brand-logo${compact ? " compact" : ""}`}
        priority
      />
      <div>
        {subtitle ? <span className="supplier-brand-title">{subtitle}</span> : null}
        {showSubdomain ? (
          <span className="supplier-brand-sub">seller.buyfromarmenia.com</span>
        ) : null}
      </div>
    </Link>
  );
}
