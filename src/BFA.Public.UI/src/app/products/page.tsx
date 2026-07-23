import { Suspense } from "react";
import { ProductsCatalog } from "./ProductsCatalog";

export default function ProductsPage() {
  return (
    <Suspense fallback={null}>
      <ProductsCatalog />
    </Suspense>
  );
}
