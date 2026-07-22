"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type AdminUserItem = {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
};

const ROLES = ["Moderator", "Admin", "SuperAdmin"] as const;

export default function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUserItem[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<AdminUserItem | null>(null);
  const [formError, setFormError] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [role, setRole] = useState<string>("Moderator");

  const loadUsers = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      setUsers(await apiFetch<AdminUserItem[]>("/api/users"));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load users.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadUsers();
  }, [loadUsers]);

  function openCreate() {
    setEditing(null);
    setEmail("");
    setPassword("");
    setFullName("");
    setRole("Moderator");
    setFormError("");
    setModalOpen(true);
  }

  function openEdit(user: AdminUserItem) {
    setEditing(user);
    setEmail(user.email);
    setPassword("");
    setFullName(user.fullName);
    setRole(user.role);
    setFormError("");
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    try {
      if (editing) {
        await apiFetch(`/api/users/${editing.id}`, {
          method: "PUT",
          body: JSON.stringify({ fullName, role }),
        });
      } else {
        await apiFetch("/api/users", {
          method: "POST",
          body: JSON.stringify({ email, password, fullName, role }),
        });
      }
      setModalOpen(false);
      await loadUsers();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save user.");
    } finally {
      setIsSaving(false);
    }
  }

  async function toggleActive(user: AdminUserItem) {
    setError("");
    try {
      await apiFetch(
        `/api/users/${user.id}/${user.isActive ? "suspend" : "activate"}`,
        { method: "POST" },
      );
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    }
  }

  return (
    <AdminShell title="Admin users">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "flex-end" }}>
        <button type="button" className="button-primary" onClick={openCreate}>
          Add user
        </button>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading users...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Email</th>
                <th>Name</th>
                <th>Role</th>
                <th>Status</th>
                <th>Last login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>{user.fullName}</td>
                  <td>{user.role}</td>
                  <td>{user.isActive ? "Active" : "Suspended"}</td>
                  <td>
                    {user.lastLoginAt
                      ? new Date(user.lastLoginAt).toLocaleString("en-GB")
                      : "—"}
                  </td>
                  <td style={{ display: "flex", gap: 8 }}>
                    <button type="button" className="button-ghost" onClick={() => openEdit(user)}>
                      Edit
                    </button>
                    <button type="button" className="button-ghost" onClick={() => void toggleActive(user)}>
                      {user.isActive ? "Suspend" : "Activate"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      <Modal
        open={modalOpen}
        title={editing ? "Edit user" : "Add user"}
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
              form="user-form-modal"
              className="button-primary"
              disabled={isSaving}
            >
              {isSaving ? "Saving..." : editing ? "Save changes" : "Create user"}
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <form id="user-form-modal" onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="user-email">Email</label>
            <input
              id="user-email"
              type="email"
              required
              value={email}
              disabled={Boolean(editing)}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="user-fullName">Full name</label>
            <input
              id="user-fullName"
              required
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
            />
          </div>
          {!editing ? (
            <div className="form-field">
              <label htmlFor="user-password">Password</label>
              <input
                id="user-password"
                type="password"
                required
                minLength={6}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          ) : null}
          <div className="form-field">
            <label htmlFor="user-role">Role</label>
            <select
              id="user-role"
              className="form-control"
              value={role}
              onChange={(e) => setRole(e.target.value)}
            >
              {ROLES.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </div>
        </form>
      </Modal>
    </AdminShell>
  );
}
