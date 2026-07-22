import { Suspense } from "react";
import { ProductsCatalog } from "@/app/products/ProductsCatalog";

type CategoryProductsPageProps = {
  params: Promise<{ slug: string }>;
};

export default async function CategoryProductsPage({
  params,
}: CategoryProductsPageProps) {
  const { slug } = await params;

  return (
    <Suspense fallback={<p style={{ padding: 48, textAlign: "center" }}>Loading...</p>}>
      <ProductsCatalog initialCategorySlug={slug} />
    </Suspense>
  );
}
