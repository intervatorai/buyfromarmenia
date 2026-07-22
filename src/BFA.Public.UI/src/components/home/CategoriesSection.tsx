"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { apiFetch, type PublicCategory } from "@/lib/api";

const CATEGORY_ART: Record<string, string> = {
  "food-grocery": "🍯",
  "wine-spirits": "🍾",
  "beauty-wellness": "🧴",
  "home-decor": "🪴",
  jewelry: "💎",
  souvenirs: "🏺",
  textiles: "🧣",
  handicrafts: "🎨",
};

function categoryIcon(slug: string) {
  return CATEGORY_ART[slug] ?? "🧺";
}

export function CategoriesSection() {
  const { translate } = useLanguage();
  const [categories, setCategories] = useState<PublicCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setCategories(await apiFetch<PublicCategory[]>("/api/categories"));
      } catch {
        setError(translate("failedToLoadCategories"));
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [translate]);

  return (
    <section className="section container" id="categories">
      <div className="section-heading">
        <div>
          <p className="eyebrow">{translate("exploreArmenia")}</p>
          <h2>{translate("popularCategories")}</h2>
        </div>

        <Link href="/categories" className="view-all">
          {translate("viewAllCategories")}
        </Link>
      </div>

      {isLoading ? <p className="catalog-message">{translate("loading")}</p> : null}
      {error ? <p className="catalog-message">{error}</p> : null}

      {!isLoading && !error && categories.length === 0 ? (
        <p className="catalog-message">{translate("noCategories")}</p>
      ) : null}

      {!isLoading && categories.length > 0 ? (
        <div className="category-grid">
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${category.slug}`}
              className="category-card"
            >
              <div className="category-art">{categoryIcon(category.slug)}</div>
              <div className="category-content">
                <h3>{category.name}</h3>
                {category.description ? <p>{category.description}</p> : null}
              </div>
            </Link>
          ))}
        </div>
      ) : null}
    </section>
  );
}
