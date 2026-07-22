"use client";

import { useEffect } from "react";
import { useParams, useRouter } from "next/navigation";

export default function EditVendorRedirect() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  useEffect(() => {
    router.replace(`/vendors/${params.id}`);
  }, [router, params.id]);
  return <p style={{ padding: 24 }}>Opening vendor…</p>;
}
