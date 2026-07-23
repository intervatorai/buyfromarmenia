"use client";

import Link from "next/link";
import { FormEvent, useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type CustomerItem = {
  id: string;
  email: string;
  fullName: string;
  phone?: string | null;
  status: string;
  createdAtUtc: string;
  lastLoginAtUtc?: string | null;
};

export default function CustomersPage() {
  const [customers, setCustomers] = useState<CustomerItem[]>([]);
  const [statusFilter, setStatusFilter] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<CustomerItem | null>(null);
  const [formError, setFormError] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState("");

  const loadCustomers = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const query = statusFilter ? `?status=${encodeURIComponent(statusFilter)}` : "";
      setCustomers(await apiFetch<CustomerItem[]>(`/api/customers${query}`));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load customers.");
    } finally {
      setIsLoading(false);
    }
  }, [statusFilter]);

  useEffect(() => {
    void loadCustomers();
  }, [loadCustomers]);

  function openCreate() {
    setEditing(null);
    setEmail("");
    setPassword("");
    setFullName("");
    setPhone("");
    setFormError("");
    setModalOpen(true);
  }

  function openEdit(customer: CustomerItem) {
    setEditing(customer);
    setEmail(customer.email);
    setPassword("");
    setFullName(customer.fullName);
    setPhone(customer.phone ?? "");
    setFormError("");
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    try {
      if (editing) {
        await apiFetch(`/api/customers/${editing.id}`, {
          method: "PUT",
          body: JSON.stringify({
            fullName,
            phone: phone || null,
            newPassword: password || null,
          }),
        });
      } else {
        await apiFetch("/api/customers", {
          method: "POST",
          body: JSON.stringify({
            email,
            password,
            fullName,
            phone: phone || null,
          }),
        });
      }
      setModalOpen(false);
      await loadCustomers();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save customer.");
    } finally {
      setIsSaving(false);
    }
  }

  async function toggleActive(customer: CustomerItem) {
    setError("");
    const isActive = customer.status === "Active";
    try {
      await apiFetch(
        `/api/customers/${customer.id}/${isActive ? "suspend" : "activate"}`,
        { method: "POST" },
      );
      await loadCustomers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    }
  }

  return (
    <AdminShell title="Customers">
      <div
        style={{
          marginBottom: 16,
          display: "flex",
          justifyContent: "space-between",
          gap: 12,
          flexWrap: "wrap",
        }}
      >
        <label style={{ margin: 0, minWidth: 180 }}>
          <select
            className="form-control"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            aria-label="Filter by status"
          >
            <option value="">All statuses</option>
            <option value="Active">Active</option>
            <option value="Suspended">Suspended</option>
          </select>
        </label>
        <button type="button" className="button-primary" onClick={openCreate}>
          Add customer
        </button>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading customers...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Email</th>
                <th>Name</th>
                <th>Phone</th>
                <th>Status</th>
                <th>Registered</th>
                <th>Last login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {customers.length === 0 ? (
                <tr>
                  <td colSpan={7}>No customers found.</td>
                </tr>
              ) : (
                customers.map((customer) => (
                  <tr key={customer.id}>
                    <td>{customer.email}</td>
                    <td>{customer.fullName || "—"}</td>
                    <td>{customer.phone || "—"}</td>
                    <td>{customer.status}</td>
                    <td>{new Date(customer.createdAtUtc).toLocaleString("en-GB")}</td>
                    <td>
                      {customer.lastLoginAtUtc
                        ? new Date(customer.lastLoginAtUtc).toLocaleString("en-GB")
                        : "—"}
                    </td>
                    <td style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                      <Link href={`/customers/${customer.id}`} className="button-ghost">
                        View
                      </Link>
                      <button
                        type="button"
                        className="button-ghost"
                        onClick={() => openEdit(customer)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="button-ghost"
                        onClick={() => void toggleActive(customer)}
                      >
                        {customer.status === "Active" ? "Suspend" : "Activate"}
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
        title={editing ? "Edit customer" : "Add customer"}
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
              form="customer-form-modal"
              className="button-primary"
              disabled={isSaving}
            >
              {isSaving ? "Saving..." : editing ? "Save changes" : "Create customer"}
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <form id="customer-form-modal" onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="customer-email">Email</label>
            <input
              id="customer-email"
              type="email"
              required
              value={email}
              disabled={Boolean(editing)}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="customer-fullName">Full name</label>
            <input
              id="customer-fullName"
              required
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="customer-phone">Phone</label>
            <input
              id="customer-phone"
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="customer-password">
              {editing ? "New password (optional)" : "Password"}
            </label>
            <input
              id="customer-password"
              type="password"
              required={!editing}
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
        </form>
      </Modal>
    </AdminShell>
  );
}
