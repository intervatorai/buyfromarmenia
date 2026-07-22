"use client";

import { useEffect } from "react";
import { useParams, useRouter } from "next/navigation";

export default function EditProductRedirect() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  useEffect(() => {
    router.replace(`/products/${params.id}`);
  }, [router, params.id]);
  return <p style={{ padding: 24 }}>Opening product…</p>;
}
