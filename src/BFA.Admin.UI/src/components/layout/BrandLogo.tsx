import Image from "next/image";
import Link from "next/link";

export function BrandLogo({
  href = "/dashboard",
  subtitle = "Admin",
  compact = false,
}: {
  href?: string;
  subtitle?: string;
  compact?: boolean;
}) {
  return (
    <Link href={href} className="admin-brand">
      <Image
        src="/brand/logo-512.png"
        alt="BuyFromArmenia"
        width={512}
        height={512}
        className={`admin-brand-logo${compact ? " compact" : ""}`}
        priority
      />
      {subtitle ? <span className="admin-brand-title">{subtitle}</span> : null}
    </Link>
  );
}
