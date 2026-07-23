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
    <Suspense fallback={null}>
      <ProductsCatalog initialCategorySlug={slug} />
    </Suspense>
  );
}
