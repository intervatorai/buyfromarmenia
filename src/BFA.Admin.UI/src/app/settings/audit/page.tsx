"use client";

import { useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type AuditEntry = {
  id: string;
  actorType: string;
  actorId?: string | null;
  action: string;
  entityType: string;
  entityId?: string | null;
  detailsJson?: string | null;
  occurredAtUtc: string;
};

export default function AuditPage() {
  const [entries, setEntries] = useState<AuditEntry[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadAudit() {
      try {
        const data = await apiFetch<AuditEntry[]>("/api/audit?take=200");
        setEntries(data);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load audit log.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadAudit();
  }, []);

  return (
    <AdminShell title="Audit log">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading audit entries...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Time (UTC)</th>
                <th>Actor</th>
                <th>Action</th>
                <th>Entity</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              {entries.length === 0 ? (
                <tr>
                  <td colSpan={5} style={{ textAlign: "center", color: "#94a3b8" }}>
                    No audit entries yet
                  </td>
                </tr>
              ) : (
                entries.map((entry) => (
                  <tr key={entry.id}>
                    <td>{new Date(entry.occurredAtUtc).toLocaleString("en-GB")}</td>
                    <td>
                      {entry.actorType}
                      {entry.actorId ? ` · ${entry.actorId.slice(0, 8)}` : ""}
                    </td>
                    <td>{entry.action}</td>
                    <td>
                      {entry.entityType}
                      {entry.entityId ? ` · ${entry.entityId.slice(0, 8)}` : ""}
                    </td>
                    <td style={{ maxWidth: 280, wordBreak: "break-word" }}>
                      {entry.detailsJson ?? "—"}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      ) : null}
    </AdminShell>
  );
}
