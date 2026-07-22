"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type CategoryItem = {
  id: string;
  name: string;
  slug: string;
  description: string;
  status: string;
  sortOrder: number;
  parentCategoryId?: string | null;
};

function slugify(value: string) {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "");
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<CategoryItem[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formError, setFormError] = useState("");

  const [name, setName] = useState("");
  const [slug, setSlug] = useState("");
  const [description, setDescription] = useState("");
  const [sortOrder, setSortOrder] = useState("0");

  const [isSeeding, setIsSeeding] = useState(false);
  const [seedMessage, setSeedMessage] = useState("");

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      setCategories(await apiFetch<CategoryItem[]>("/api/categories"));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load categories.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  function openCreate() {
    setEditingId(null);
    setName("");
    setSlug("");
    setDescription("");
    setSortOrder("0");
    setFormError("");
    setModalOpen(true);
  }

  function openEdit(category: CategoryItem) {
    setEditingId(category.id);
    setName(category.name);
    setSlug(category.slug);
    setDescription(category.description);
    setSortOrder(String(category.sortOrder));
    setFormError("");
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    const payload = {
      name,
      slug: slug || slugify(name),
      description,
      sortOrder: Number(sortOrder) || 0,
    };

    try {
      if (editingId) {
        await apiFetch(`/api/categories/${editingId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch("/api/categories", {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }

      setModalOpen(false);
      await load();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save category.");
    } finally {
      setIsSaving(false);
    }
  }

  async function toggleStatus(category: CategoryItem) {
    setError("");
    try {
      await apiFetch(
        `/api/categories/${category.id}/${category.status === "Active" ? "hide" : "activate"}`,
        { method: "POST" },
      );
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update category status.");
    }
  }

  async function seedDefaults() {
    setIsSeeding(true);
    setError("");
    setSeedMessage("");
    try {
      const result = await apiFetch<{ added: number; skipped: number; addedSlugs: string[] }>(
        "/api/categories/seed-defaults",
        { method: "POST" },
      );
      setSeedMessage(
        result.added > 0
          ? `Added ${result.added} default categor${result.added === 1 ? "y" : "ies"}` +
            (result.skipped > 0 ? ` (skipped ${result.skipped} existing).` : ".")
          : `All default categories already exist (skipped ${result.skipped}).`,
      );
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to seed default categories.");
    } finally {
      setIsSeeding(false);
    }
  }

  return (
    <AdminShell title="Categories">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "flex-end", gap: 8, flexWrap: "wrap" }}>
        <button
          type="button"
          className="button-secondary"
          disabled={isSeeding}
          onClick={() => void seedDefaults()}
        >
          {isSeeding ? "Seeding..." : "Add default categories"}
        </button>
        <button type="button" className="button-primary" onClick={openCreate}>
          Add category
        </button>
      </div>

      {seedMessage ? (
        <p style={{ color: "var(--admin-muted)", marginBottom: 16, fontSize: 13 }}>{seedMessage}</p>
      ) : null}

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Slug</th>
                <th>Status</th>
                <th>Sort</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {categories.length === 0 ? (
                <tr>
                  <td colSpan={5}>No categories yet.</td>
                </tr>
              ) : (
                categories.map((category) => (
                  <tr key={category.id}>
                    <td>
                      <strong>{category.name}</strong>
                      {category.description ? (
                        <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                          {category.description}
                        </div>
                      ) : null}
                    </td>
                    <td>{category.slug}</td>
                    <td>{category.status}</td>
                    <td>{category.sortOrder}</td>
                    <td style={{ display: "flex", gap: 8 }}>
                      <button
                        type="button"
                        className="button-ghost"
                        onClick={() => openEdit(category)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="button-ghost"
                        onClick={() => void toggleStatus(category)}
                      >
                        {category.status === "Active" ? "Hide" : "Activate"}
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      ) : null}

      <Modal
        open={modalOpen}
        title={editingId ? "Edit category" : "Add category"}
        onClose={() => setModalOpen(false)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setModalOpen(false)}
              disabled={isSaving}
            >
              Cancel
            </button>
            <button
              type="submit"
              form="category-form-modal"
              className="button-primary"
              disabled={isSaving}
            >
              {isSaving ? "Saving..." : editingId ? "Save changes" : "Create category"}
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <form id="category-form-modal" onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="category-name">Name</label>
            <input
              id="category-name"
              required
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                if (!editingId) {
                  setSlug(slugify(e.target.value));
                }
              }}
            />
          </div>
          <div className="form-field">
            <label htmlFor="category-slug">Slug</label>
            <input
              id="category-slug"
              required
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="category-description">Description</label>
            <input
              id="category-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="category-sortOrder">Sort order</label>
            <input
              id="category-sortOrder"
              type="number"
              value={sortOrder}
              onChange={(e) => setSortOrder(e.target.value)}
            />
          </div>
        </form>
      </Modal>
    </AdminShell>
  );
}
