"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { apiFetch, type PublicCategory } from "@/lib/api";

export default function CategoriesPage() {
  const { translate } = useLanguage();
  const [categories, setCategories] = useState<PublicCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadCategories() {
      try {
        setCategories(await apiFetch<PublicCategory[]>("/api/categories"));
      } finally {
        setIsLoading(false);
      }
    }

    void loadCategories();
  }, []);

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <h1>{translate("categoriesTitle")}</h1>
        {isLoading ? <p className="catalog-message">{translate("loading")}</p> : null}

        <div className="catalog-grid">
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${category.slug}`}
              className="catalog-card"
            >
              <h2>{category.name}</h2>
              {category.description ? <p>{category.description}</p> : null}
            </Link>
          ))}
        </div>
      </section>
    </PublicSiteLayout>
  );
}
