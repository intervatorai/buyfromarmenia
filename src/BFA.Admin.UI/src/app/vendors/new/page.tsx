"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function NewVendorRedirect() {
  const router = useRouter();
  useEffect(() => {
    router.replace("/vendors");
  }, [router]);
  return <p style={{ padding: 24 }}>Opening vendors…</p>;
}
