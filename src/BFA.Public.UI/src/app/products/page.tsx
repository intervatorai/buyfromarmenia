import { Suspense } from "react";
import { ProductsCatalog } from "./ProductsCatalog";

export default function ProductsPage() {
  return (
    <Suspense fallback={<p style={{ padding: 48, textAlign: "center" }}>Loading...</p>}>
      <ProductsCatalog />
    </Suspense>
  );
}
